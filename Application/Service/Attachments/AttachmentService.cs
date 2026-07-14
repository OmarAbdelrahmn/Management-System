using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Attachments;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Application.Service.Attachments;

public class AttachmentService(ApplicationDbcontext dbcontext, ICurrentUserContext currentUserContext, IAttachmentStorage storage, IAttachmentMalwareScanner scanner, IOptions<AttachmentOptions>? options = null) : IAttachmentService
{
    private readonly AttachmentOptions attachmentOptions = options?.Value ?? new AttachmentOptions();
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [".csv"] = "text/csv", [".txt"] = "text/plain"
    };

    public async Task<Result<AttachmentAccessResponse>> GetEntityAccessAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || !int.TryParse(entityId, out var parsedEntityId) || parsedEntityId <= 0)
            return Result.Failure<AttachmentAccessResponse>(AttachmentErrors.InvalidRequest);
        if (!TryGetCapability(entityType, out var capability))
            return Result.Failure<AttachmentAccessResponse>(AttachmentErrors.UnsupportedEntity);
        if (!await EntityExistsAsync(capability.EntityType, parsedEntityId, cancellationToken))
            return Result.Failure<AttachmentAccessResponse>(AttachmentErrors.NotFound);

        var canManage = await HasAnyPermissionAsync(capability.ManagePermissions, cancellationToken);
        var canView = canManage || await HasAnyPermissionAsync(capability.ViewPermissions, cancellationToken);
        return Result.Success(new AttachmentAccessResponse(canView, canManage));
    }

    public async Task<Result<AttachmentResponse>> UploadAsync(AttachmentUploadInput input, CancellationToken cancellationToken = default)
    {
        var accessError = await EnsureEntityAccessAsync(input.EntityType, input.EntityId, manage: true, cancellationToken);
        if (accessError is not null) return await FailAsync<AttachmentResponse>(accessError, "AttachmentUploadDenied", input.EntityType, input.EntityId, cancellationToken);
        var prepared = await PrepareAsync(input, cancellationToken);
        if (prepared.Error is not null) return await FailAsync<AttachmentResponse>(prepared.Error, "AttachmentUploadRejected", input.EntityType, input.EntityId, cancellationToken);
        var asset = new FileAsset
        {
            FileName = prepared.Value!.StoredName, OriginalFileName = Path.GetFileName(input.OriginalFileName), ContentType = prepared.Value.ContentType,
            SizeBytes = input.SizeBytes, StoragePath = prepared.Value.StoragePath, Category = NormalizeCategory(input.Category),
            UploadedByUserId = currentUserContext.UserId!, IsPublic = false
        };
        asset.Versions.Add(CreateVersion(asset, prepared.Value, input.SizeBytes, 1));
        asset.Links.Add(new FileAssetLink { EntityType = input.EntityType!.Trim(), EntityId = input.EntityId!.Trim(), Label = input.Label?.Trim() });
        dbcontext.FileAssets.Add(asset);
        Audit("AttachmentScanCompleted", asset, input.EntityType, input.EntityId);
        Audit("AttachmentUploaded", asset, input.EntityType, input.EntityId);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(asset));
    }

    public async Task<Result<AttachmentResponse>> AddVersionAsync(int id, AttachmentUploadInput input, CancellationToken cancellationToken = default)
    {
        var asset = await dbcontext.FileAssets.Include(x => x.Links).Include(x => x.Versions).FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (asset is null) return Result.Failure<AttachmentResponse>(AttachmentErrors.NotFound);
        var accessError = await EnsureLinksAccessAsync(asset.Links, manage: true, cancellationToken);
        if (accessError is not null) return await FailAsync<AttachmentResponse>(accessError, "AttachmentVersionDenied", null, id.ToString(), cancellationToken);
        var prepared = await PrepareAsync(input, cancellationToken);
        if (prepared.Error is not null) return await FailAsync<AttachmentResponse>(prepared.Error, "AttachmentVersionRejected", null, id.ToString(), cancellationToken);
        var versionNumber = asset.Versions.Count == 0 ? 1 : asset.Versions.Max(x => x.VersionNumber) + 1;
        asset.FileName = prepared.Value!.StoredName; asset.ContentType = prepared.Value.ContentType; asset.SizeBytes = input.SizeBytes; asset.StoragePath = prepared.Value.StoragePath;
        asset.Versions.Add(CreateVersion(asset, prepared.Value, input.SizeBytes, versionNumber));
        Audit("AttachmentScanCompleted", asset, null, null);
        Audit("AttachmentVersionUploaded", asset, null, null);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(asset));
    }

    public async Task<Result<IReadOnlyList<AttachmentResponse>>> GetForEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        var accessError = await EnsureEntityAccessAsync(entityType, entityId, manage: false, cancellationToken);
        if (accessError is not null) return await FailAsync<IReadOnlyList<AttachmentResponse>>(accessError, "AttachmentListDenied", entityType, entityId, cancellationToken);
        var assets = await dbcontext.FileAssets.AsNoTracking().Include(x => x.Links).Include(x => x.Versions)
            .Where(x => x.DeletedAt == null && x.Links.Any(link => link.EntityType == entityType && link.EntityId == entityId)).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<AttachmentResponse>>(assets.Select(Map).ToList());
    }

    public async Task<Result<IReadOnlyList<AttachmentVersionResponse>>> GetVersionsAsync(int id, CancellationToken cancellationToken = default)
    {
        var asset = await dbcontext.FileAssets.AsNoTracking().Include(x => x.Links).Include(x => x.Versions).FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (asset is null) return Result.Failure<IReadOnlyList<AttachmentVersionResponse>>(AttachmentErrors.NotFound);
        var accessError = await EnsureAnyLinkAccessAsync(asset.Links, manage: false, cancellationToken);
        if (accessError is not null) return await FailAsync<IReadOnlyList<AttachmentVersionResponse>>(accessError, "AttachmentVersionsDenied", null, id.ToString(), cancellationToken);
        var versions = asset.Versions.OrderByDescending(x => x.VersionNumber)
            .Select(x => new AttachmentVersionResponse(x.Id, x.VersionNumber, x.FileName, x.ContentType, x.SizeBytes, x.Sha256, x.ScanStatus.ToString(), x.ScannedAt, x.CreatedAt)).ToList();
        return versions.Count == 0 ? Result.Failure<IReadOnlyList<AttachmentVersionResponse>>(AttachmentErrors.NotFound) : Result.Success<IReadOnlyList<AttachmentVersionResponse>>(versions);
    }

    public async Task<Result<AttachmentDownloadResponse>> GetDownloadAsync(int id, CancellationToken cancellationToken = default)
    {
        var asset = await dbcontext.FileAssets.Include(x => x.Links).Include(x => x.Versions).FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (asset is null) return Result.Failure<AttachmentDownloadResponse>(AttachmentErrors.NotFound);
        var accessError = await EnsureAnyLinkAccessAsync(asset.Links, manage: false, cancellationToken);
        if (accessError is not null) return await FailAsync<AttachmentDownloadResponse>(accessError, "AttachmentDownloadDenied", null, id.ToString(), cancellationToken);
        var version = asset.Versions.OrderByDescending(x => x.VersionNumber).FirstOrDefault();
        if (version is null || version.ScanStatus != FileAssetScanStatus.Clean) return Result.Failure<AttachmentDownloadResponse>(AttachmentErrors.Unavailable);
        await using var content = await storage.OpenReadAsync(version.StoragePath, cancellationToken);
        if (content is null) return Result.Failure<AttachmentDownloadResponse>(AttachmentErrors.Unavailable);
        Audit("AttachmentDownloaded", asset, null, null); await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new AttachmentDownloadResponse(version.StoragePath, version.ContentType, asset.OriginalFileName));
    }

    public async Task<Result> UnlinkAsync(int linkId, CancellationToken cancellationToken = default)
    {
        var link = await dbcontext.FileAssetLinks.Include(x => x.FileAsset).FirstOrDefaultAsync(x => x.Id == linkId, cancellationToken);
        if (link is null || link.FileAsset is null) return Result.Failure(AttachmentErrors.NotFound);
        var accessError = await EnsureEntityAccessAsync(link.EntityType, link.EntityId, manage: true, cancellationToken);
        if (accessError is not null) return await FailAsync(accessError, "AttachmentUnlinkDenied", link.EntityType, link.EntityId, cancellationToken);
        Audit("AttachmentUnlinked", link.FileAsset, link.EntityType, link.EntityId); dbcontext.FileAssetLinks.Remove(link); await dbcontext.SaveChangesAsync(cancellationToken); return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var asset = await dbcontext.FileAssets.Include(x => x.Links).FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (asset is null) return Result.Failure(AttachmentErrors.NotFound);
        var accessError = await EnsureLinksAccessAsync(asset.Links, manage: true, cancellationToken);
        if (accessError is not null) return await FailAsync(accessError, "AttachmentDeleteDenied", null, id.ToString(), cancellationToken);
        asset.DeletedAt = DateTime.UtcNow.AddHours(3); asset.PurgeAfter = asset.DeletedAt.Value.AddDays(GetRetentionDays()); Audit("AttachmentDeleted", asset, null, null); await dbcontext.SaveChangesAsync(cancellationToken); return Result.Success();
    }

    public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expired = await dbcontext.FileAssets.Include(x => x.Versions).Where(x => x.PurgeAfter != null && x.PurgeAfter <= DateTime.UtcNow.AddHours(3)).ToListAsync(cancellationToken);
        foreach (var asset in expired) foreach (var version in asset.Versions) await storage.DeleteAsync(version.StoragePath, cancellationToken);
        dbcontext.FileAssets.RemoveRange(expired); await dbcontext.SaveChangesAsync(cancellationToken); return expired.Count;
    }

    private async Task<(PreparedAttachment? Value, Error? Error)> PrepareAsync(AttachmentUploadInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId) || input.Content is null || input.SizeBytes <= 0) return (null, AttachmentErrors.InvalidRequest);
        if (input.SizeBytes > GetMaximumSizeBytes()) return (null, AttachmentErrors.TooLarge);
        if (!attachmentOptions.RequireMalwareScanner) return (null, AttachmentErrors.MalwareScanFailed);
        var extension = Path.GetExtension(input.OriginalFileName);
        if (!AllowedTypes.TryGetValue(extension, out var contentType)) return (null, AttachmentErrors.UnsupportedType);
        string? temporaryPath = null;
        try
        {
            temporaryPath = await storage.SaveTemporaryAsync(input.Content, cancellationToken);
            var scan = await scanner.ScanAsync(temporaryPath, cancellationToken);
            if (scan == AttachmentMalwareScanResult.Infected) return (null, AttachmentErrors.Infected);
            if (scan != AttachmentMalwareScanResult.Clean) return (null, AttachmentErrors.MalwareScanFailed);
            await using var stream = await storage.OpenReadAsync(temporaryPath, cancellationToken);
            if (stream is null || !await HasValidSignatureAsync(stream, extension, cancellationToken)) return (null, AttachmentErrors.UnsupportedType);
            stream.Position = 0; var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
            var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            await storage.PromoteAsync(temporaryPath, storedName, cancellationToken);
            return (new PreparedAttachment(storedName, storedName, contentType, hash), null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return (null, AttachmentErrors.MalwareScanFailed);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(temporaryPath))
            {
                try { await storage.DeleteAsync(temporaryPath, CancellationToken.None); }
                catch { /* Cleanup failure must not replace the operation result. */ }
            }
        }
    }

    private static async Task<bool> HasValidSignatureAsync(Stream stream, string extension, CancellationToken cancellationToken)
    {
        var bytes = new byte[8]; var read = await stream.ReadAsync(bytes, cancellationToken); stream.Position = 0;
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => read >= 4 && bytes[..4].SequenceEqual("%PDF"u8.ToArray()),
            ".png" => read >= 8 && bytes.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => read >= 3 && bytes[..3].SequenceEqual(new byte[] { 255, 216, 255 }),
            ".docx" or ".xlsx" => read >= 4 && bytes[..4].SequenceEqual(new byte[] { 80, 75, 3, 4 }),
            _ => true
        };
    }

    private FileAssetVersion CreateVersion(FileAsset asset, PreparedAttachment prepared, long size, int number) => new() { FileAsset = asset, VersionNumber = number, FileName = prepared.StoredName, ContentType = prepared.ContentType, SizeBytes = size, StoragePath = prepared.StoragePath, Sha256 = prepared.Sha256, ScanStatus = FileAssetScanStatus.Clean, ScannedAt = DateTime.UtcNow.AddHours(3), UploadedByUserId = currentUserContext.UserId! };
    private void Audit(string action, FileAsset? asset, string? entityType, string? entityId, string? detail = null) => dbcontext.AuditLogs.Add(new AuditLog { ActorUserId = currentUserContext.UserId ?? "system", Action = action, EntityName = entityType ?? "FileAsset", EntityId = entityId ?? asset?.Id.ToString() ?? string.Empty, Details = $"{detail ?? asset?.OriginalFileName ?? string.Empty}; ip={currentUserContext.RemoteIpAddress ?? "unknown"}" });
    private static string NormalizeCategory(string value) => string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
    private async Task<bool> HasAnyPermissionAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)) return true;
        var roles = currentUserContext.Roles.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return roles.Length > 0 && keys.Count > 0 && await dbcontext.RolePermissions.AsNoTracking().AnyAsync(
            x => x.IsGranted && x.AppPermission != null && x.Role != null && keys.Contains(x.AppPermission.Key) && roles.Contains(x.Role.Name!),
            cancellationToken);
    }
    private async Task<Error?> EnsureEntityAccessAsync(string? entityType, string? entityId, bool manage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityType) || !int.TryParse(entityId, out var parsedEntityId) || parsedEntityId <= 0) return AttachmentErrors.InvalidRequest;
        if (!TryGetCapability(entityType, out var capability)) return AttachmentErrors.UnsupportedEntity;
        if (!await EntityExistsAsync(capability.EntityType, parsedEntityId, cancellationToken)) return AttachmentErrors.NotFound;
        var requiredPermissions = manage
            ? capability.ManagePermissions
            : capability.ViewPermissions.Concat(capability.ManagePermissions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return await HasAnyPermissionAsync(requiredPermissions, cancellationToken) ? null : AttachmentErrors.Forbidden;
    }

    private async Task<Error?> EnsureLinksAccessAsync(IEnumerable<FileAssetLink> links, bool manage, CancellationToken cancellationToken)
    {
        var materialized = links.ToList();
        if (materialized.Count == 0) return AttachmentErrors.Forbidden;
        foreach (var link in materialized)
        {
            var error = await EnsureEntityAccessAsync(link.EntityType, link.EntityId, manage, cancellationToken);
            if (error is not null) return error;
        }
        return null;
    }

    private async Task<Error?> EnsureAnyLinkAccessAsync(IEnumerable<FileAssetLink> links, bool manage, CancellationToken cancellationToken)
    {
        var materialized = links.ToList();
        if (materialized.Count == 0) return AttachmentErrors.Forbidden;
        foreach (var link in materialized)
            if (await EnsureEntityAccessAsync(link.EntityType, link.EntityId, manage, cancellationToken) is null) return null;
        return AttachmentErrors.Forbidden;
    }

    private Task<bool> EntityExistsAsync(string entityType, int id, CancellationToken cancellationToken) => entityType switch
    {
        "LedgerEntry" => dbcontext.LedgerEntries.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "BeneficiaryProfile" => dbcontext.BeneficiaryProfiles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "BoardMeeting" => dbcontext.BoardMeetings.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "EmployeeLeaveRequest" => dbcontext.EmployeeLeaveRequests.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "MemberProfile" => dbcontext.MemberProfiles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "EmployeeProfile" => dbcontext.EmployeeProfiles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "ProgramProject" => dbcontext.ProgramProjects.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "BeneficiaryAidRequest" => dbcontext.BeneficiaryAidRequests.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "FollowUpCase" => dbcontext.FollowUpCases.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        "AdministrativeDecisionRecord" => dbcontext.AdministrativeDecisionRecords.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        _ => Task.FromResult(false)
    };

    private static bool TryGetCapability(string entityType, out AttachmentCapability capability) => Capabilities.TryGetValue(entityType.Trim(), out capability!);
    private long GetMaximumSizeBytes() => attachmentOptions.MaximumSizeBytes > 0 ? attachmentOptions.MaximumSizeBytes : AttachmentOptions.DefaultMaximumSizeBytes;
    private int GetRetentionDays() => attachmentOptions.RetentionDays > 0 ? attachmentOptions.RetentionDays : AttachmentOptions.DefaultRetentionDays;
    private async Task<Result<T>> FailAsync<T>(Error error, string action, string? entityType, string? entityId, CancellationToken cancellationToken) { Audit(action, null, entityType, entityId, error.Code); await dbcontext.SaveChangesAsync(cancellationToken); return Result.Failure<T>(error); }
    private async Task<Result> FailAsync(Error error, string action, string? entityType, string? entityId, CancellationToken cancellationToken) { Audit(action, null, entityType, entityId, error.Code); await dbcontext.SaveChangesAsync(cancellationToken); return Result.Failure(error); }
    private static AttachmentResponse Map(FileAsset asset) { var latest = asset.Versions.OrderByDescending(x => x.VersionNumber).FirstOrDefault(); return new(asset.Id, asset.OriginalFileName, latest?.ContentType ?? asset.ContentType, latest?.SizeBytes ?? asset.SizeBytes, asset.Category, latest?.VersionNumber ?? 0, latest?.Sha256 ?? string.Empty, latest?.ScanStatus.ToString() ?? FileAssetScanStatus.Unavailable.ToString(), asset.CreatedAt, asset.Links.Select(x => new AttachmentLinkResponse(x.Id, x.EntityType, x.EntityId, x.Label, x.CreatedAt)).ToList()); }
    private sealed record PreparedAttachment(string StoredName, string StoragePath, string ContentType, string Sha256);
    private sealed record AttachmentCapability(string EntityType, IReadOnlyCollection<string> ViewPermissions, IReadOnlyCollection<string> ManagePermissions);
    private static readonly IReadOnlyDictionary<string, AttachmentCapability> Capabilities = new Dictionary<string, AttachmentCapability>(StringComparer.OrdinalIgnoreCase)
    {
        ["LedgerEntry"] = new("LedgerEntry",
            ["system.accounting.finance_ledgers", "system.accounting.finance_ledgers_manage"],
            ["system.accounting.finance_ledgers", "system.accounting.finance_ledgers_manage"]),
        ["BeneficiaryProfile"] = new("BeneficiaryProfile",
            ["system.beneficiary-accounts.profiles_database", "system.beneficiary-accounts.profiles_update"],
            ["system.beneficiary-accounts.profiles_update"]),
        ["BoardMeeting"] = new("BoardMeeting",
            ["system.documentation-archive.meetings_manage_schedule"],
            ["system.documentation-archive.meetings_manage_schedule"]),
        ["EmployeeLeaveRequest"] = new("EmployeeLeaveRequest",
            ["system.human-resources.hr_personnel_vacations"],
            ["system.human-resources.hr_personnel_vacations"]),
        ["MemberProfile"] = new("MemberProfile",
            ["system.participating-members.hr_board_database", "system.participating-members.board_database", "system.participating-members.board_members"],
            ["system.participating-members.hr_board_update", "system.participating-members.board_members"]),
        ["EmployeeProfile"] = new("EmployeeProfile",
            ["system.human-resources.hr_personnel_database", "system.human-resources.hr_personnel_update"],
            ["system.human-resources.hr_personnel_update"]),
        ["ProgramProject"] = new("ProgramProject",
            ["system.programs-projects-designs.projects_database", "system.programs-projects-designs.projects_databases", "system.programs-projects-designs.projects_update"],
            ["system.programs-projects-designs.projects_create", "system.programs-projects-designs.projects_update"]),
        ["BeneficiaryAidRequest"] = new("BeneficiaryAidRequest",
            ["system.beneficiary-services.request_zap_database", "system.beneficiary-services.request_update"],
            ["system.beneficiary-services.request_create", "system.beneficiary-services.request_update"]),
        ["FollowUpCase"] = new("FollowUpCase",
            ["system.evaluation-followup.followup_requests", "system.evaluation-followup.followup_zap_database", "system.evaluation-followup.followup_manage"],
            ["system.evaluation-followup.followup_request", "system.evaluation-followup.followup_manage"]),
        ["AdministrativeDecisionRecord"] = new("AdministrativeDecisionRecord",
            ["system.executive-supervision.decisions_management", "system.executive-supervision.decisions_meetings"],
            ["system.executive-supervision.decisions_management"])
    };
}
