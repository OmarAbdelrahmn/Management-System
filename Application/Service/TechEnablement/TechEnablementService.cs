using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.TechEnablement;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.TechEnablement;

public class TechEnablementService(ApplicationDbcontext dbcontext) : ITechEnablementService
{
    public async Task<Result<TechEnablementDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        Result.Success(new TechEnablementDashboardResponse(
            await dbcontext.TechSystemSettings.CountAsync(cancellationToken),
            await dbcontext.OrganizationAssignments.CountAsync(cancellationToken),
            await dbcontext.VisualAssetTemplates.CountAsync(cancellationToken),
            await dbcontext.CybersecurityReviews.CountAsync(x => x.Status == CybersecurityReviewStatus.Open || x.Status == CybersecurityReviewStatus.InReview, cancellationToken),
            await dbcontext.NcnpDataRecords.CountAsync(x => x.Status == NcnpDataStatus.NeedsUpdate, cancellationToken),
            await dbcontext.NcnpDataRecords.CountAsync(x => x.Status == NcnpDataStatus.ReadyToRegister, cancellationToken)));

    public async Task<Result<IEnumerable<TechSystemSettingResponse>>> GetSettingsAsync(string? category = null, TechSettingStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.TechSystemSettings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category.Trim());
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<TechSystemSettingResponse>>(await query.OrderBy(x => x.Category).ThenBy(x => x.Key).Select(x => MapSetting(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<TechSystemSettingResponse>> SaveSettingAsync(int? id, SaveTechSystemSettingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.Category))
            return Result.Failure<TechSystemSettingResponse>(TechEnablementErrors.InvalidRequest);

        var key = request.Key.Trim();
        if (await dbcontext.TechSystemSettings.AnyAsync(x => x.Key == key && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<TechSystemSettingResponse>(TechEnablementErrors.DuplicateSettingKey);

        var entity = id.HasValue ? await dbcontext.TechSystemSettings.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new TechSystemSetting();
        if (entity is null) return Result.Failure<TechSystemSettingResponse>(TechEnablementErrors.SettingNotFound);
        if (!id.HasValue) dbcontext.TechSystemSettings.Add(entity);
        entity.Key = key;
        entity.NameAr = request.NameAr.Trim();
        entity.Category = request.Category.Trim();
        entity.Value = TrimOrNull(request.Value);
        entity.Notes = TrimOrNull(request.Notes);
        entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSetting(entity));
    }

    public async Task<Result<IEnumerable<OrganizationAssignmentResponse>>> GetOrganizationAssignmentsAsync(OrganizationAssignmentType? assignmentType = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OrganizationAssignments.AsNoTracking().AsQueryable();
        if (assignmentType.HasValue) query = query.Where(x => x.AssignmentType == assignmentType.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<OrganizationAssignmentResponse>>(await query.OrderBy(x => x.AssignmentType).ThenBy(x => x.UnitName).Select(x => MapOrganization(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OrganizationAssignmentResponse>> SaveOrganizationAssignmentAsync(int? id, SaveOrganizationAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UnitName) || string.IsNullOrWhiteSpace(request.AssigneeName))
            return Result.Failure<OrganizationAssignmentResponse>(TechEnablementErrors.InvalidRequest);

        var entity = id.HasValue ? await dbcontext.OrganizationAssignments.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new OrganizationAssignment();
        if (entity is null) return Result.Failure<OrganizationAssignmentResponse>(TechEnablementErrors.OrganizationAssignmentNotFound);
        if (!id.HasValue) dbcontext.OrganizationAssignments.Add(entity);
        entity.UnitName = request.UnitName.Trim();
        entity.AssigneeName = request.AssigneeName.Trim();
        entity.AssignmentType = request.AssignmentType;
        entity.RoleTitle = TrimOrNull(request.RoleTitle);
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapOrganization(entity));
    }

    public async Task<Result<IEnumerable<VisualAssetTemplateResponse>>> GetVisualAssetsAsync(VisualAssetType? assetType = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VisualAssetTemplates.AsNoTracking().AsQueryable();
        if (assetType.HasValue) query = query.Where(x => x.AssetType == assetType.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<VisualAssetTemplateResponse>>(await query.OrderBy(x => x.AssetType).ThenBy(x => x.Name).Select(x => MapVisualAsset(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<VisualAssetTemplateResponse>> SaveVisualAssetAsync(int? id, SaveVisualAssetTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<VisualAssetTemplateResponse>(TechEnablementErrors.InvalidRequest);

        var entity = id.HasValue ? await dbcontext.VisualAssetTemplates.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new VisualAssetTemplate();
        if (entity is null) return Result.Failure<VisualAssetTemplateResponse>(TechEnablementErrors.VisualAssetNotFound);
        if (!id.HasValue) dbcontext.VisualAssetTemplates.Add(entity);
        entity.Name = request.Name.Trim();
        entity.AssetType = request.AssetType;
        entity.FileUrl = TrimOrNull(request.FileUrl);
        entity.DesignJson = TrimOrNull(request.DesignJson);
        entity.IsActive = request.IsActive;
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapVisualAsset(entity));
    }

    public async Task<Result<IEnumerable<CybersecurityReviewResponse>>> GetCybersecurityReviewsAsync(CybersecurityReviewStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CybersecurityReviews.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<CybersecurityReviewResponse>>(await query.OrderBy(x => x.Status).ThenBy(x => x.DueDate ?? DateTime.MaxValue).Select(x => MapSecurity(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<CybersecurityReviewResponse>> SaveCybersecurityReviewAsync(int? id, SaveCybersecurityReviewRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Area) || string.IsNullOrWhiteSpace(request.Finding) || string.IsNullOrWhiteSpace(request.Severity))
            return Result.Failure<CybersecurityReviewResponse>(TechEnablementErrors.InvalidRequest);

        var entity = id.HasValue ? await dbcontext.CybersecurityReviews.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new CybersecurityReview();
        if (entity is null) return Result.Failure<CybersecurityReviewResponse>(TechEnablementErrors.SecurityReviewNotFound);
        if (!id.HasValue) dbcontext.CybersecurityReviews.Add(entity);
        entity.Area = request.Area.Trim();
        entity.Finding = request.Finding.Trim();
        entity.Severity = request.Severity.Trim();
        entity.Status = request.Status;
        entity.Owner = TrimOrNull(request.Owner);
        entity.DueDate = request.DueDate;
        entity.MitigationPlan = TrimOrNull(request.MitigationPlan);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSecurity(entity));
    }

    public async Task<Result<IEnumerable<NcnpDataRecordResponse>>> GetNcnpDataAsync(NcnpDataStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.NcnpDataRecords.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<NcnpDataRecordResponse>>(await query.OrderBy(x => x.Status).ThenBy(x => x.ReferenceNumber).Select(x => MapNcnp(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<NcnpDataRecordResponse>> SaveNcnpDataAsync(int? id, SaveNcnpDataRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenceNumber) || string.IsNullOrWhiteSpace(request.BeneficiaryName) || string.IsNullOrWhiteSpace(request.SupportType) || request.Cost < 0)
            return Result.Failure<NcnpDataRecordResponse>(TechEnablementErrors.InvalidRequest);

        var reference = request.ReferenceNumber.Trim();
        if (await dbcontext.NcnpDataRecords.AnyAsync(x => x.ReferenceNumber == reference && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<NcnpDataRecordResponse>(TechEnablementErrors.DuplicateNcnpReference);

        var entity = id.HasValue ? await dbcontext.NcnpDataRecords.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new NcnpDataRecord();
        if (entity is null) return Result.Failure<NcnpDataRecordResponse>(TechEnablementErrors.NcnpRecordNotFound);
        if (!id.HasValue) dbcontext.NcnpDataRecords.Add(entity);
        entity.ReferenceNumber = reference;
        entity.BeneficiaryName = request.BeneficiaryName.Trim();
        entity.SupportType = request.SupportType.Trim();
        entity.SupportDate = request.SupportDate;
        entity.Cost = request.Cost;
        entity.Status = request.Status;
        entity.PlatformReference = TrimOrNull(request.PlatformReference);
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapNcnp(entity));
    }

    public async Task<Result> UpdateNcnpStatusAsync(int id, UpdateNcnpDataStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.NcnpDataRecords.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure(TechEnablementErrors.NcnpRecordNotFound);
        entity.Status = request.Status;
        entity.PlatformReference = TrimOrNull(request.PlatformReference) ?? entity.PlatformReference;
        entity.Notes = TrimOrNull(request.Notes) ?? entity.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static TechSystemSettingResponse MapSetting(TechSystemSetting x) => new(x.Id, x.Key, x.NameAr, x.Category, x.Value, x.Notes, x.Status.ToString());
    private static OrganizationAssignmentResponse MapOrganization(OrganizationAssignment x) => new(x.Id, x.UnitName, x.AssigneeName, x.AssignmentType.ToString(), x.RoleTitle, x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.Notes);
    private static VisualAssetTemplateResponse MapVisualAsset(VisualAssetTemplate x) => new(x.Id, x.Name, x.AssetType.ToString(), x.FileUrl, x.DesignJson, x.IsActive, x.Notes);
    private static CybersecurityReviewResponse MapSecurity(CybersecurityReview x) => new(x.Id, x.Area, x.Finding, x.Severity, x.Status.ToString(), x.Owner, x.DueDate, x.MitigationPlan);
    private static NcnpDataRecordResponse MapNcnp(NcnpDataRecord x) => new(x.Id, x.ReferenceNumber, x.BeneficiaryName, x.SupportType, x.SupportDate, x.Cost, x.Status.ToString(), x.PlatformReference, x.Notes);
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
