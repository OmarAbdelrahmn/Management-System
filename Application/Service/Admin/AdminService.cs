using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Admin;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Result<IEnumerable<FileAssetResponse>>> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        var files = await dbcontext.FileAssets.AsNoTracking().Include(x => x.UploadedByUser).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<FileAssetResponse>>(files.Select(MapFile));
    }

    public async Task<Result<FileAssetResponse>> SaveFileAssetAsync(SaveFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OriginalFileName) || string.IsNullOrWhiteSpace(request.StoragePath))
            return Result.Failure<FileAssetResponse>(AdminErrors.InvalidRequest);

        var userId = currentUserContext.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<FileAssetResponse>(AdminErrors.InvalidRequest);

        var file = new FileAsset
        {
            FileName = Path.GetFileName(request.StoragePath),
            OriginalFileName = request.OriginalFileName.Trim(),
            ContentType = request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            StoragePath = request.StoragePath.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            UploadedByUserId = userId,
            IsPublic = request.IsPublic
        };

        dbcontext.FileAssets.Add(file);
        await dbcontext.SaveChangesAsync(cancellationToken);
        var created = await dbcontext.FileAssets.AsNoTracking().Include(x => x.UploadedByUser).FirstAsync(x => x.Id == file.Id, cancellationToken);
        return Result.Success(MapFile(created));
    }

    public async Task<Result<IEnumerable<AuditLogResponse>>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbcontext.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(300).Select(x =>
            new AuditLogResponse(x.Id, x.ActorUserId, x.Action, x.EntityName, x.EntityId, x.Details, x.CreatedAt)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<AuditLogResponse>>(logs);
    }

    public Result<JobDashboardStatusResponse> GetJobDashboardStatus(bool hangfireEnabled, string cron) =>
        Result.Success(new JobDashboardStatusResponse(hangfireEnabled, "/jobs", cron));

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
            ("system.admin.manage", "إدارة النظام", "Admin")
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
        new(file.Id, file.FileName, file.OriginalFileName, file.ContentType, file.SizeBytes, file.StoragePath, file.Category, file.UploadedByUserId, file.UploadedByUser?.FullName ?? string.Empty, file.IsPublic, file.CreatedAt);

    private static Error IdentityError(IdentityResult result) =>
        new(AdminErrors.IdentityFailure.Code, string.Join("; ", result.Errors.Select(x => x.Description)), AdminErrors.IdentityFailure.StatuesCode);
}
