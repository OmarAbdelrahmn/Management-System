using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Admin;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Service.Admin;

public class AdminService(
    ApplicationDbcontext dbcontext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ICurrentUserContext currentUserContext) : IAdminService
{
    public async Task<Result<IEnumerable<AdminUserResponse>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbcontext.Users.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(cancellationToken);
        var responses = new List<AdminUserResponse>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            responses.Add(new AdminUserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.IsActive, roles.ToList(), user.CreatedAt));
        }

        return Result.Success<IEnumerable<AdminUserResponse>>(responses);
    }

    public async Task<Result<AdminUserResponse>> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<AdminUserResponse>(AdminErrors.InvalidRequest);

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure<AdminUserResponse>(IdentityError(createResult));

        await ReplaceRolesAsync(user, request.Roles);
        var roles = await userManager.GetRolesAsync(user);
        return Result.Success(new AdminUserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.IsActive, roles.ToList(), user.CreatedAt));
    }

    public async Task<Result<AdminUserResponse>> UpdateUserAsync(string id, UpdateAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure<AdminUserResponse>(AdminErrors.UserNotFound);

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.IsActive = request.IsActive;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure<AdminUserResponse>(IdentityError(updateResult));

        await ReplaceRolesAsync(user, request.Roles);
        var roles = await userManager.GetRolesAsync(user);
        return Result.Success(new AdminUserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.IsActive, roles.ToList(), user.CreatedAt));
    }

    public async Task<Result<IEnumerable<RoleResponse>>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await dbcontext.Roles.AsNoTracking().OrderBy(x => x.Name).Select(x => new RoleResponse(x.Id, x.Name ?? string.Empty)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<RoleResponse>>(roles);
    }

    public async Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<RoleResponse>(AdminErrors.InvalidRequest);

        var role = new ApplicationRole { Name = request.Name.Trim() };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(IdentityError(result));

        return Result.Success(new RoleResponse(role.Id, role.Name ?? string.Empty));
    }

    public async Task<Result<RoleResponse>> UpdateRoleAsync(string id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<RoleResponse>(AdminErrors.InvalidRequest);

        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
            return Result.Failure<RoleResponse>(AdminErrors.RoleNotFound);

        role.Name = request.Name.Trim();
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(IdentityError(result));

        return Result.Success(new RoleResponse(role.Id, role.Name ?? string.Empty));
    }

    public async Task<Result> DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
            return Result.Failure(AdminErrors.RoleNotFound);

        if (await dbcontext.UserRoles.AnyAsync(x => x.RoleId == id, cancellationToken))
            return Result.Failure(AdminErrors.RoleInUse);

        var grants = await dbcontext.RolePermissions.Where(x => x.RoleId == id).ToListAsync(cancellationToken);
        if (grants.Count > 0)
        {
            dbcontext.RolePermissions.RemoveRange(grants);
            await dbcontext.SaveChangesAsync(cancellationToken);
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return Result.Failure(IdentityError(result));
        return Result.Success();
    }

    public async Task<Result<IEnumerable<PermissionResponse>>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        await SeedDefaultPermissionsAsync(cancellationToken);
        var permissions = await dbcontext.AppPermissions.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<PermissionResponse>>(permissions.Select(MapPermission));
    }

    public async Task<Result<PermissionResponse>> SavePermissionAsync(SavePermissionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<PermissionResponse>(AdminErrors.InvalidRequest);

        var permission = await dbcontext.AppPermissions.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken);
        if (permission is null)
        {
            permission = new AppPermission { Key = request.Key.Trim() };
            dbcontext.AppPermissions.Add(permission);
        }

        permission.NameAr = request.NameAr.Trim();
        permission.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        permission.Description = request.Description?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPermission(permission));
    }

    public async Task<Result> GrantRolePermissionAsync(GrantRolePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var roleExists = await dbcontext.Roles.AnyAsync(x => x.Id == request.RoleId, cancellationToken);
        var permissionExists = await dbcontext.AppPermissions.AnyAsync(x => x.Id == request.PermissionId, cancellationToken);
        if (!roleExists || !permissionExists)
            return Result.Failure(AdminErrors.InvalidRequest);

        var grant = await dbcontext.RolePermissions.FirstOrDefaultAsync(x => x.RoleId == request.RoleId && x.AppPermissionId == request.PermissionId, cancellationToken);
        if (grant is null)
        {
            grant = new RolePermission { RoleId = request.RoleId, AppPermissionId = request.PermissionId };
            dbcontext.RolePermissions.Add(grant);
        }

        grant.IsGranted = request.IsGranted;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<RolePermissionResponse>>> GetRolePermissionsAsync(CancellationToken cancellationToken = default)
    {
        var grants = await dbcontext.RolePermissions
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.AppPermission)
            .OrderBy(x => x.Role!.Name)
            .ThenBy(x => x.AppPermission!.Category)
            .ThenBy(x => x.AppPermission!.Key)
            .Select(x => new RolePermissionResponse(x.RoleId, x.Role!.Name ?? string.Empty, x.AppPermissionId, x.AppPermission!.Key, x.AppPermission.NameAr, x.IsGranted))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<RolePermissionResponse>>(grants);
    }

    public async Task<Result<IEnumerable<LoginAuditResponse>>> GetLoginAuditAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbcontext.UserLoginAudits.AsNoTracking().OrderByDescending(x => x.AttemptedAt).Take(300).Select(x =>
            new LoginAuditResponse(x.Id, x.UserId, x.UserName, x.Result.ToString(), x.FailureReason, x.IpAddress, x.UserAgent, x.AttemptedAt)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<LoginAuditResponse>>(logs);
    }

    public async Task<Result<IEnumerable<SystemSettingResponse>>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        await SeedDefaultSettingsAsync(cancellationToken);
        var query = dbcontext.SystemSettings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);

        var settings = await query.OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<SystemSettingResponse>>(settings.Select(MapSetting));
    }

    public async Task<Result<SystemSettingResponse>> SaveSettingAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<SystemSettingResponse>(AdminErrors.InvalidRequest);

        var setting = await dbcontext.SystemSettings.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken);
        if (setting is null)
        {
            setting = new SystemSetting { Key = request.Key.Trim() };
            dbcontext.SystemSettings.Add(setting);
        }

        setting.NameAr = request.NameAr.Trim();
        setting.Value = request.Value;
        setting.ValueType = request.ValueType;
        setting.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        setting.IsEditable = request.IsEditable;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSetting(setting));
    }

    public async Task<Result<PagedResponse<FileAssetResponse>>> GetFilesAsync(FileAssetSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new FileAssetSearchRequest(null, null, null);
        var query = dbcontext.FileAssets.AsNoTracking().Include(x => x.UploadedByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request?.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.OriginalFileName.Contains(search) || x.FileName.Contains(search) || x.StoragePath.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(request?.Category))
            query = query.Where(x => x.Category == request.Category.Trim());
        if (request?.IsPublic is not null)
            query = query.Where(x => x.IsPublic == request.IsPublic.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        query = request!.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.Descending ? query.OrderByDescending(x => x.OriginalFileName) : query.OrderBy(x => x.OriginalFileName),
            "category" => request.Descending ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category),
            "size" => request.Descending ? query.OrderByDescending(x => x.SizeBytes) : query.OrderBy(x => x.SizeBytes),
            _ => request.Descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
        var (page, pageSize) = NormalizePage(request.Page, request.PageSize);
        var files = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return Result.Success(new PagedResponse<FileAssetResponse>(files.Select(MapFile).ToList(), page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<Result<FileAssetResponse>> SaveFileAssetAsync(SaveFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        return Result.Failure<FileAssetResponse>(AdminErrors.InvalidRequest);
    }

    public async Task<Result<FileAssetResponse>> UpdateFileAssetAsync(int id, UpdateFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        return Result.Failure<FileAssetResponse>(AdminErrors.InvalidRequest);
    }

    public async Task<Result<FileAssetResponse>> GetFileAsync(int id, CancellationToken cancellationToken = default)
    {
        var file = await dbcontext.FileAssets.AsNoTracking().Include(x => x.UploadedByUser).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return file is null ? Result.Failure<FileAssetResponse>(AdminErrors.FileNotFound) : Result.Success(MapFile(file));
    }

    public async Task<Result> DeleteFileAssetAsync(int id, CancellationToken cancellationToken = default)
    {
        var file = await dbcontext.FileAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (file is null)
            return Result.Failure(AdminErrors.FileNotFound);

        dbcontext.FileAssets.Remove(file);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<FileAssetLinkResponse>> LinkFileAsync(LinkFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FileAssetId <= 0 || string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.EntityId))
            return Result.Failure<FileAssetLinkResponse>(AdminErrors.InvalidRequest);
        var file = await dbcontext.FileAssets.FirstOrDefaultAsync(x => x.Id == request.FileAssetId, cancellationToken);
        if (file is null) return Result.Failure<FileAssetLinkResponse>(AdminErrors.FileNotFound);
        var link = await dbcontext.FileAssetLinks.FirstOrDefaultAsync(x => x.FileAssetId == request.FileAssetId && x.EntityType == request.EntityType.Trim() && x.EntityId == request.EntityId.Trim(), cancellationToken);
        if (link is null)
        {
            link = new FileAssetLink { FileAssetId = file.Id, EntityType = request.EntityType.Trim(), EntityId = request.EntityId.Trim(), Label = request.Label?.Trim() };
            dbcontext.FileAssetLinks.Add(link);
            dbcontext.AuditLogs.Add(new AuditLog { ActorUserId = currentUserContext.UserId ?? "system", Action = "AttachmentLinked", EntityName = link.EntityType, EntityId = link.EntityId, Details = file.OriginalFileName });
            await dbcontext.SaveChangesAsync(cancellationToken);
        }
        return Result.Success(MapLink(link, file));
    }

    public async Task<Result<IEnumerable<FileAssetLinkResponse>>> GetFileLinksAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId)) return Result.Failure<IEnumerable<FileAssetLinkResponse>>(AdminErrors.InvalidRequest);
        var links = await dbcontext.FileAssetLinks.AsNoTracking().Include(x => x.FileAsset).Where(x => x.EntityType == entityType.Trim() && x.EntityId == entityId.Trim()).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<FileAssetLinkResponse>>(links.Select(x => MapLink(x, x.FileAsset!)));
    }

    public async Task<Result> UnlinkFileAsync(int linkId, CancellationToken cancellationToken = default)
    {
        var link = await dbcontext.FileAssetLinks.FirstOrDefaultAsync(x => x.Id == linkId, cancellationToken);
        if (link is null) return Result.Failure(AdminErrors.FileNotFound);
        dbcontext.AuditLogs.Add(new AuditLog { ActorUserId = currentUserContext.UserId ?? "system", Action = "AttachmentUnlinked", EntityName = link.EntityType, EntityId = link.EntityId, Details = link.FileAssetId.ToString() });
        dbcontext.FileAssetLinks.Remove(link);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<AuditLogResponse>>> GetAuditLogsAsync(AuditLogSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new AuditLogSearchRequest(null, null, null, null, null, null);
        var query = dbcontext.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request?.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Action.Contains(search) || x.EntityName.Contains(search) || x.EntityId.Contains(search) || x.Details.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(request?.ActorUserId)) query = query.Where(x => x.ActorUserId == request.ActorUserId.Trim());
        if (!string.IsNullOrWhiteSpace(request?.EntityName)) query = query.Where(x => x.EntityName == request.EntityName.Trim());
        if (!string.IsNullOrWhiteSpace(request?.EntityId)) query = query.Where(x => x.EntityId == request.EntityId.Trim());
        if (request?.From is not null) query = query.Where(x => x.CreatedAt >= request.From.Value.Date);
        if (request?.To is not null) query = query.Where(x => x.CreatedAt < request.To.Value.Date.AddDays(1));
        var totalCount = await query.CountAsync(cancellationToken);
        query = request!.SortBy?.ToLowerInvariant() switch
        {
            "action" => request.Descending ? query.OrderByDescending(x => x.Action) : query.OrderBy(x => x.Action),
            "entity" => request.Descending ? query.OrderByDescending(x => x.EntityName) : query.OrderBy(x => x.EntityName),
            "user" => request.Descending ? query.OrderByDescending(x => x.ActorUserId) : query.OrderBy(x => x.ActorUserId),
            _ => request.Descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
        var (page, pageSize) = NormalizePage(request.Page, request.PageSize);
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x =>
            new AuditLogResponse(x.Id, x.ActorUserId, x.Action, x.EntityName, x.EntityId, x.Details, x.BeforeJson, x.AfterJson, x.CreatedAt)).ToListAsync(cancellationToken);
        return Result.Success(new PagedResponse<AuditLogResponse>(logs, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<Result<IEnumerable<QueryViewResponse>>> GetQueryViewsAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId) || string.IsNullOrWhiteSpace(screenKey)) return Result.Failure<IEnumerable<QueryViewResponse>>(AdminErrors.InvalidRequest);
        var views = await dbcontext.SavedQueryViews.AsNoTracking().Where(x => x.UserId == currentUserContext.UserId && x.ScreenKey == screenKey.Trim()).OrderBy(x => x.Name)
            .Select(x => new QueryViewResponse(x.Id, x.ScreenKey, x.Name, x.FilterJson, x.CreatedAt)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<QueryViewResponse>>(views);
    }

    public async Task<Result<QueryViewResponse>> SaveQueryViewAsync(SaveQueryViewRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId) || string.IsNullOrWhiteSpace(request.ScreenKey) || string.IsNullOrWhiteSpace(request.Name) || !IsValidFilterJson(request.FilterJson)) return Result.Failure<QueryViewResponse>(AdminErrors.InvalidRequest);
        var userId = currentUserContext.UserId;
        var screenKey = request.ScreenKey.Trim();
        var name = request.Name.Trim();
        var view = await dbcontext.SavedQueryViews.FirstOrDefaultAsync(x => x.UserId == userId && x.ScreenKey == screenKey && x.Name == name, cancellationToken);
        if (view is null) { view = new SavedQueryView { UserId = userId, ScreenKey = screenKey, Name = name, FilterJson = request.FilterJson }; dbcontext.SavedQueryViews.Add(view); }
        else view.FilterJson = request.FilterJson;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new QueryViewResponse(view.Id, view.ScreenKey, view.Name, view.FilterJson, view.CreatedAt));
    }

    public async Task<Result> DeleteQueryViewAsync(int id, CancellationToken cancellationToken = default)
    {
        var view = await dbcontext.SavedQueryViews.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUserContext.UserId, cancellationToken);
        if (view is null) return Result.Failure(AdminErrors.InvalidRequest);
        dbcontext.SavedQueryViews.Remove(view);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public Result<JobDashboardStatusResponse> GetJobDashboardStatus(bool hangfireEnabled, string cron) =>
        Result.Success(new JobDashboardStatusResponse(hangfireEnabled, "/jobs", cron));

    private static FileAssetLinkResponse MapLink(FileAssetLink link, FileAsset file) => new(link.Id, file.Id, file.FileName, file.OriginalFileName, link.EntityType, link.EntityId, link.Label, link.CreatedAt);
    private static (int Page, int PageSize) NormalizePage(int page, int pageSize) => (Math.Max(1, page), Math.Clamp(pageSize, 10, 100));
    private static bool IsValidFilterJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > 20_000) return false;
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task ReplaceRolesAsync(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await userManager.RemoveFromRolesAsync(user, currentRoles);

        var targetRoles = roles.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        foreach (var role in targetRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        if (targetRoles.Length > 0)
            await userManager.AddToRolesAsync(user, targetRoles);
    }

    private async Task SeedDefaultPermissionsAsync(CancellationToken cancellationToken)
    {
        var defaults = new[]
        {
            ("system.members.view", "عرض الأعضاء", "Members"),
            ("system.members.manage", "إدارة الأعضاء", "Members"),
            ("system.tasks.manage", "إدارة المهام", "Tasks"),
            ("system.messaging.manage", "إدارة البريد والتنبيهات", "Messaging"),
            ("system.meetings.manage", "إدارة الاجتماعات", "Meetings"),
            ("system.admin.manage", "إدارة النظام", "Admin"),
            ("system.attachments.view", "عرض المرفقات", "Attachments"),
            ("system.attachments.manage", "إدارة المرفقات", "Attachments")
        };

        foreach (var item in defaults)
        {
            if (await dbcontext.AppPermissions.AnyAsync(x => x.Key == item.Item1, cancellationToken))
                continue;

            dbcontext.AppPermissions.Add(new AppPermission { Key = item.Item1, NameAr = item.Item2, Category = item.Item3 });
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDefaultSettingsAsync(CancellationToken cancellationToken)
    {
        var defaults = new[]
        {
            new SystemSetting { Key = "System.Name", NameAr = "اسم النظام", Value = "نظام الإدارة الإلكتروني", Category = "General" },
            new SystemSetting { Key = "Smtp.Host", NameAr = "SMTP Host", Value = null, Category = "SMTP" },
            new SystemSetting { Key = "Smtp.Email", NameAr = "SMTP Email", Value = null, Category = "SMTP" },
            new SystemSetting { Key = "Smtp.Password", NameAr = "SMTP Password", Value = null, ValueType = SystemSettingValueType.Secret, Category = "SMTP" }
        };

        foreach (var item in defaults)
        {
            if (await dbcontext.SystemSettings.AnyAsync(x => x.Key == item.Key, cancellationToken))
                continue;

            dbcontext.SystemSettings.Add(item);
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
    }

    private static PermissionResponse MapPermission(AppPermission permission) =>
        new(permission.Id, permission.Key, permission.NameAr, permission.Category, permission.Description);

    private static SystemSettingResponse MapSetting(SystemSetting setting) =>
        new(setting.Id, setting.Key, setting.NameAr, setting.ValueType == SystemSettingValueType.Secret && !string.IsNullOrWhiteSpace(setting.Value) ? "********" : setting.Value, setting.ValueType.ToString(), setting.Category, setting.IsEditable);

    private static FileAssetResponse MapFile(FileAsset file) =>
        new(file.Id, file.FileName, file.OriginalFileName, file.ContentType, file.SizeBytes, string.Empty, file.Category, file.UploadedByUserId, file.UploadedByUser?.FullName ?? string.Empty, false, file.CreatedAt);

    private static Error IdentityError(IdentityResult result) =>
        new(AdminErrors.IdentityFailure.Code, string.Join("; ", result.Errors.Select(x => x.Description)), AdminErrors.IdentityFailure.StatuesCode);
}
