using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Beneficiaries;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Beneficiaries;

public class BeneficiaryService(ApplicationDbcontext dbcontext) : IBeneficiaryService
{
    public async Task<Result<BeneficiaryDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var profilesCount = await dbcontext.BeneficiaryProfiles.CountAsync(cancellationToken);
        var activeProfilesCount = await dbcontext.BeneficiaryProfiles.CountAsync(x => x.Status == BeneficiaryStatus.Active, cancellationToken);
        var archivedProfilesCount = await dbcontext.BeneficiaryProfiles.CountAsync(x => x.Status == BeneficiaryStatus.Archived || x.Status == BeneficiaryStatus.Deleted, cancellationToken);
        var dependentsCount = await dbcontext.BeneficiaryDependents.CountAsync(x => x.IsActive, cancellationToken);
        var guardiansCount = await dbcontext.BeneficiaryGuardians.CountAsync(cancellationToken);
        var pendingUpdateRequestsCount = await dbcontext.BeneficiaryUpdateRequests.CountAsync(x => x.Status == BeneficiaryUpdateRequestStatus.Pending, cancellationToken);
        var beneficiaryEntitiesCount = await dbcontext.BeneficiaryEntities.CountAsync(x => x.Status == BeneficiaryEntityStatus.Active, cancellationToken);
        var accountArtifactsCount = await dbcontext.BeneficiaryAccountArtifacts.CountAsync(cancellationToken);
        var pendingGuardianOperationsCount = await dbcontext.BeneficiaryGuardianOperations.CountAsync(x => x.Status == BeneficiaryOperationStatus.Pending, cancellationToken);
        var openUpdateBatchesCount = await dbcontext.BeneficiaryUpdateBatches
            .CountAsync(x => x.Status == BeneficiaryOperationStatus.Pending || x.Status == BeneficiaryOperationStatus.Approved, cancellationToken);

        return Result.Success(new BeneficiaryDashboardResponse(
            profilesCount,
            activeProfilesCount,
            archivedProfilesCount,
            dependentsCount,
            guardiansCount,
            pendingUpdateRequestsCount,
            beneficiaryEntitiesCount,
            accountArtifactsCount,
            pendingGuardianOperationsCount,
            openUpdateBatchesCount));
    }

    public async Task<Result<IEnumerable<BeneficiaryResponse>>> SearchAsync(BeneficiarySearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.BeneficiaryNumber.Contains(search) ||
                (x.NationalId != null && x.NationalId.Contains(search)) ||
                (x.Mobile != null && x.Mobile.Contains(search)) ||
                (x.Email != null && x.Email.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(x => x.Category != null && x.Category.Contains(category));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();
            query = query.Where(x => x.City != null && x.City.Contains(city));
        }

        var profiles = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.FullName)
            .Select(x => MapProfile(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryResponse>>(profiles);
    }

    public async Task<Result<BeneficiaryResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return profile is null
            ? Result.Failure<BeneficiaryResponse>(BeneficiaryErrors.ProfileNotFound)
            : Result.Success(MapProfile(profile));
    }

    public async Task<Result<BeneficiaryResponse>> CreateAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.MonthlyIncome < 0 || request.FamilyMembersCount < 0)
            return Result.Failure<BeneficiaryResponse>(BeneficiaryErrors.InvalidRequest);

        var beneficiaryNumber = string.IsNullOrWhiteSpace(request.BeneficiaryNumber)
            ? await GenerateBeneficiaryNumberAsync(cancellationToken)
            : request.BeneficiaryNumber.Trim();

        if (await dbcontext.BeneficiaryProfiles.AnyAsync(x => x.BeneficiaryNumber == beneficiaryNumber, cancellationToken))
            return Result.Failure<BeneficiaryResponse>(BeneficiaryErrors.DuplicateBeneficiaryNumber);

        var profile = new BeneficiaryProfile
        {
            BeneficiaryNumber = beneficiaryNumber,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Gender = request.Gender?.Trim(),
            BirthDate = request.BirthDate?.Date,
            Mobile = request.Mobile?.Trim(),
            Email = request.Email?.Trim(),
            City = request.City?.Trim(),
            Address = request.Address?.Trim(),
            Category = request.Category?.Trim(),
            Grade = request.Grade?.Trim(),
            MonthlyIncome = request.MonthlyIncome,
            FamilyMembersCount = request.FamilyMembersCount,
            Notes = request.Notes?.Trim()
        };

        dbcontext.BeneficiaryProfiles.Add(profile);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapProfile(profile));
    }

    public async Task<Result<BeneficiaryResponse>> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || request.MonthlyIncome < 0 || request.FamilyMembersCount < 0)
            return Result.Failure<BeneficiaryResponse>(BeneficiaryErrors.InvalidRequest);

        profile.FullName = request.FullName.Trim();
        profile.NationalId = request.NationalId?.Trim();
        profile.Gender = request.Gender?.Trim();
        profile.BirthDate = request.BirthDate?.Date;
        profile.Mobile = request.Mobile?.Trim();
        profile.Email = request.Email?.Trim();
        profile.City = request.City?.Trim();
        profile.Address = request.Address?.Trim();
        profile.Category = request.Category?.Trim();
        profile.Grade = request.Grade?.Trim();
        profile.Status = request.Status;
        profile.MonthlyIncome = request.MonthlyIncome;
        profile.FamilyMembersCount = request.FamilyMembersCount;
        profile.Notes = request.Notes?.Trim();

        if (profile.Status is not (BeneficiaryStatus.Archived or BeneficiaryStatus.Deleted))
        {
            profile.ArchivedAt = null;
            profile.ArchiveReason = null;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapProfile(profile));
    }

    public async Task<Result> ArchiveAsync(int id, ArchiveBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (profile is null)
            return Result.Failure(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(BeneficiaryErrors.InvalidRequest);

        profile.Status = BeneficiaryStatus.Archived;
        profile.ArchivedAt = DateTime.UtcNow.AddHours(3);
        profile.ArchiveReason = request.Reason.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (profile is null)
            return Result.Failure(BeneficiaryErrors.ProfileNotFound);

        profile.Status = BeneficiaryStatus.Active;
        profile.ArchivedAt = null;
        profile.ArchiveReason = null;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<BeneficiaryDependentResponse>>> GetDependentsAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryDependents
            .AsNoTracking()
            .Include(x => x.BeneficiaryProfile)
            .AsQueryable();

        if (beneficiaryProfileId.HasValue)
            query = query.Where(x => x.BeneficiaryProfileId == beneficiaryProfileId.Value);

        var dependents = await query
            .OrderBy(x => x.BeneficiaryProfile!.FullName)
            .ThenBy(x => x.FullName)
            .Select(x => MapDependent(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryDependentResponse>>(dependents);
    }

    public async Task<Result<BeneficiaryDependentResponse>> AddDependentAsync(AddBeneficiaryDependentRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryDependentResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Relationship))
            return Result.Failure<BeneficiaryDependentResponse>(BeneficiaryErrors.InvalidRequest);

        var existingDependentsCount = await dbcontext.BeneficiaryDependents
            .CountAsync(x => x.BeneficiaryProfileId == request.BeneficiaryProfileId && x.IsActive, cancellationToken);

        var dependent = new BeneficiaryDependent
        {
            BeneficiaryProfileId = request.BeneficiaryProfileId,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Relationship = request.Relationship.Trim(),
            BirthDate = request.BirthDate?.Date,
            Category = request.Category?.Trim(),
            Grade = request.Grade?.Trim(),
            Notes = request.Notes?.Trim(),
            BeneficiaryProfile = profile
        };

        dbcontext.BeneficiaryDependents.Add(dependent);
        profile.FamilyMembersCount = Math.Max(profile.FamilyMembersCount, existingDependentsCount + 2);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDependent(dependent));
    }

    public async Task<Result<BeneficiaryDependentResponse>> UpdateDependentAsync(int id, UpdateBeneficiaryDependentRequest request, CancellationToken cancellationToken = default)
    {
        var dependent = await dbcontext.BeneficiaryDependents
            .Include(x => x.BeneficiaryProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dependent is null)
            return Result.Failure<BeneficiaryDependentResponse>(BeneficiaryErrors.DependentNotFound);

        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryDependentResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Relationship))
            return Result.Failure<BeneficiaryDependentResponse>(BeneficiaryErrors.InvalidRequest);

        dependent.BeneficiaryProfileId = request.BeneficiaryProfileId;
        dependent.BeneficiaryProfile = profile;
        dependent.FullName = request.FullName.Trim();
        dependent.NationalId = request.NationalId?.Trim();
        dependent.Relationship = request.Relationship.Trim();
        dependent.BirthDate = request.BirthDate?.Date;
        dependent.Category = request.Category?.Trim();
        dependent.Grade = request.Grade?.Trim();
        dependent.IsActive = request.IsActive;
        dependent.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDependent(dependent));
    }

    public async Task<Result<IEnumerable<BeneficiaryGuardianResponse>>> GetGuardiansAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryGuardians
            .AsNoTracking()
            .Include(x => x.BeneficiaryProfile)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (beneficiaryProfileId.HasValue)
            query = query.Where(x => x.BeneficiaryProfileId == beneficiaryProfileId.Value);

        var guardians = await query
            .OrderBy(x => x.BeneficiaryProfile!.FullName)
            .ThenByDescending(x => x.IsPrimary)
            .ThenBy(x => x.FullName)
            .Select(x => MapGuardian(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryGuardianResponse>>(guardians);
    }

    public async Task<Result<BeneficiaryGuardianResponse>> AddGuardianAsync(AddBeneficiaryGuardianRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryGuardianResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Relationship))
            return Result.Failure<BeneficiaryGuardianResponse>(BeneficiaryErrors.InvalidRequest);

        if (request.IsPrimary)
        {
            var existingPrimaryGuardians = await dbcontext.BeneficiaryGuardians
                .Where(x => x.BeneficiaryProfileId == request.BeneficiaryProfileId && x.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var guardian in existingPrimaryGuardians)
                guardian.IsPrimary = false;
        }

        var created = new BeneficiaryGuardian
        {
            BeneficiaryProfileId = request.BeneficiaryProfileId,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Mobile = request.Mobile?.Trim(),
            Relationship = request.Relationship.Trim(),
            IsPrimary = request.IsPrimary,
            Notes = request.Notes?.Trim(),
            BeneficiaryProfile = profile
        };

        dbcontext.BeneficiaryGuardians.Add(created);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapGuardian(created));
    }

    public async Task<Result<BeneficiaryGuardianResponse>> UpdateGuardianAsync(int id, UpdateBeneficiaryGuardianRequest request, CancellationToken cancellationToken = default)
    {
        var guardian = await dbcontext.BeneficiaryGuardians
            .Include(x => x.BeneficiaryProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (guardian is null)
            return Result.Failure<BeneficiaryGuardianResponse>(BeneficiaryErrors.GuardianNotFound);

        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryGuardianResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Relationship))
            return Result.Failure<BeneficiaryGuardianResponse>(BeneficiaryErrors.InvalidRequest);

        if (request.IsPrimary)
        {
            var existingPrimaryGuardians = await dbcontext.BeneficiaryGuardians
                .Where(x => x.BeneficiaryProfileId == request.BeneficiaryProfileId && x.Id != id && x.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var existingGuardian in existingPrimaryGuardians)
                existingGuardian.IsPrimary = false;
        }

        guardian.BeneficiaryProfileId = request.BeneficiaryProfileId;
        guardian.BeneficiaryProfile = profile;
        guardian.FullName = request.FullName.Trim();
        guardian.NationalId = request.NationalId?.Trim();
        guardian.Mobile = request.Mobile?.Trim();
        guardian.Relationship = request.Relationship.Trim();
        guardian.IsPrimary = request.IsPrimary;
        guardian.Notes = request.Notes?.Trim();

        if (request.IsDeleted)
        {
            guardian.IsDeleted = true;
            guardian.DeletedAt ??= DateTime.UtcNow.AddHours(3);
            guardian.DeleteReason = request.DeleteReason?.Trim();
        }
        else
        {
            guardian.IsDeleted = false;
            guardian.DeletedAt = null;
            guardian.DeleteReason = null;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapGuardian(guardian));
    }

    public async Task<Result<IEnumerable<BeneficiaryUpdateRequestResponse>>> GetUpdateRequestsAsync(int? beneficiaryProfileId = null, BeneficiaryUpdateRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryUpdateRequests
            .AsNoTracking()
            .Include(x => x.BeneficiaryProfile)
            .AsQueryable();

        if (beneficiaryProfileId.HasValue)
            query = query.Where(x => x.BeneficiaryProfileId == beneficiaryProfileId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var requests = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapUpdateRequest(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryUpdateRequestResponse>>(requests);
    }

    public async Task<Result<BeneficiaryUpdateRequestResponse>> CreateUpdateRequestAsync(CreateBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.ProfileNotFound);

        if (string.IsNullOrWhiteSpace(request.RequestedField) || string.IsNullOrWhiteSpace(request.RequestedValue))
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.InvalidRequest);

        var updateRequest = new BeneficiaryUpdateRequest
        {
            BeneficiaryProfileId = request.BeneficiaryProfileId,
            RequestedField = request.RequestedField.Trim(),
            CurrentValue = string.IsNullOrWhiteSpace(request.CurrentValue)
                ? ReadFieldValue(profile, request.RequestedField)
                : request.CurrentValue.Trim(),
            RequestedValue = request.RequestedValue.Trim(),
            Reason = request.Reason?.Trim(),
            BeneficiaryProfile = profile
        };

        dbcontext.BeneficiaryUpdateRequests.Add(updateRequest);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateRequest(updateRequest));
    }

    public async Task<Result<BeneficiaryUpdateRequestResponse>> DecideUpdateRequestAsync(int id, DecideBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var updateRequest = await dbcontext.BeneficiaryUpdateRequests
            .Include(x => x.BeneficiaryProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (updateRequest is null)
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.UpdateRequestNotFound);

        if (updateRequest.Status != BeneficiaryUpdateRequestStatus.Pending)
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.UpdateRequestAlreadyDecided);

        if (!request.Approved && string.IsNullOrWhiteSpace(request.Notes))
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.InvalidRequest);

        if (request.Approved && updateRequest.BeneficiaryProfile is not null)
        {
            var applyResult = ApplyFieldUpdate(updateRequest.BeneficiaryProfile, updateRequest.RequestedField, updateRequest.RequestedValue);
            if (applyResult.IsFailure)
                return Result.Failure<BeneficiaryUpdateRequestResponse>(applyResult.Error);
        }

        updateRequest.Status = request.Approved ? BeneficiaryUpdateRequestStatus.Approved : BeneficiaryUpdateRequestStatus.Rejected;
        updateRequest.DecisionNotes = request.Notes?.Trim();
        updateRequest.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateRequest(updateRequest));
    }

    public async Task<Result<BeneficiaryUpdateRequestResponse>> CancelUpdateRequestAsync(int id, CancelBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var updateRequest = await dbcontext.BeneficiaryUpdateRequests
            .Include(x => x.BeneficiaryProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (updateRequest is null)
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.UpdateRequestNotFound);

        if (updateRequest.Status != BeneficiaryUpdateRequestStatus.Pending || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure<BeneficiaryUpdateRequestResponse>(BeneficiaryErrors.InvalidRequest);

        updateRequest.Status = BeneficiaryUpdateRequestStatus.Cancelled;
        updateRequest.DecisionNotes = request.Reason.Trim();
        updateRequest.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateRequest(updateRequest));
    }

    public async Task<Result<IEnumerable<BeneficiaryEntityResponse>>> GetEntitiesAsync(string? search = null, BeneficiaryEntityStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryEntities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x =>
                x.NameAr.Contains(value) ||
                (x.NameEn != null && x.NameEn.Contains(value)) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(value)) ||
                (x.Mobile != null && x.Mobile.Contains(value)) ||
                (x.Email != null && x.Email.Contains(value)));
        }

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var entities = await query
            .OrderBy(x => x.NameAr)
            .Select(x => MapEntity(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryEntityResponse>>(entities);
    }

    public async Task<Result<BeneficiaryEntityResponse>> SaveEntityAsync(int? id, UpsertBeneficiaryEntityRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<BeneficiaryEntityResponse>(BeneficiaryErrors.InvalidRequest);

        var nameAr = request.NameAr.Trim();
        var duplicateExists = await dbcontext.BeneficiaryEntities
            .AnyAsync(x => x.NameAr == nameAr && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicateExists)
            return Result.Failure<BeneficiaryEntityResponse>(BeneficiaryErrors.DuplicateEntityName);

        BeneficiaryEntity entity;
        if (id.HasValue)
        {
            var existing = await dbcontext.BeneficiaryEntities.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (existing is null)
                return Result.Failure<BeneficiaryEntityResponse>(BeneficiaryErrors.EntityNotFound);

            entity = existing;
        }
        else
        {
            entity = new BeneficiaryEntity();
            dbcontext.BeneficiaryEntities.Add(entity);
        }

        entity.NameAr = nameAr;
        entity.NameEn = request.NameEn?.Trim();
        entity.ContactPerson = request.ContactPerson?.Trim();
        entity.Mobile = request.Mobile?.Trim();
        entity.Email = request.Email?.Trim();
        entity.City = request.City?.Trim();
        entity.Address = request.Address?.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEntity(entity));
    }

    public async Task<Result<IEnumerable<BeneficiaryAccountArtifactResponse>>> GetAccountArtifactsAsync(
        BeneficiaryAccountArtifactType? type = null,
        BeneficiaryAccountArtifactStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryAccountArtifacts.AsNoTracking().AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var artifacts = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapAccountArtifact(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryAccountArtifactResponse>>(artifacts);
    }

    public async Task<Result<BeneficiaryAccountArtifactResponse>> CreateAccountArtifactAsync(
        CreateBeneficiaryAccountArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        BeneficiaryProfile? profile = null;
        BeneficiaryDependent? dependent = null;

        if (request.BeneficiaryProfileId.HasValue)
        {
            profile = await dbcontext.BeneficiaryProfiles
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken);
            if (profile is null)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.ProfileNotFound);
        }

        if (request.BeneficiaryDependentId.HasValue)
        {
            dependent = await dbcontext.BeneficiaryDependents
                .Include(x => x.BeneficiaryProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryDependentId.Value, cancellationToken);
            if (dependent is null)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.DependentNotFound);

            if (profile is not null && dependent.BeneficiaryProfileId != profile.Id)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.InvalidRequest);
        }

        var holderName = request.HolderName?.Trim();
        if (string.IsNullOrWhiteSpace(holderName))
            holderName = dependent?.FullName ?? profile?.FullName;

        if (string.IsNullOrWhiteSpace(holderName))
            return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.InvalidRequest);

        var artifact = new BeneficiaryAccountArtifact
        {
            Type = request.Type,
            Status = request.Type is BeneficiaryAccountArtifactType.Card or BeneficiaryAccountArtifactType.Barcode
                ? BeneficiaryAccountArtifactStatus.Ready
                : BeneficiaryAccountArtifactStatus.Draft,
            BeneficiaryProfileId = profile?.Id ?? dependent?.BeneficiaryProfileId,
            BeneficiaryDependentId = dependent?.Id,
            ReferenceNumber = await GenerateArtifactReferenceAsync(request.Type, cancellationToken),
            HolderName = holderName,
            Source = request.Source?.Trim(),
            Payload = request.Payload?.Trim(),
            Notes = request.Notes?.Trim(),
            BeneficiaryProfile = profile ?? dependent?.BeneficiaryProfile,
            BeneficiaryDependent = dependent
        };

        dbcontext.BeneficiaryAccountArtifacts.Add(artifact);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAccountArtifact(artifact));
    }

    public async Task<Result<BeneficiaryAccountArtifactResponse>> UpdateAccountArtifactStatusAsync(
        int id,
        UpdateBeneficiaryAccountArtifactStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await dbcontext.BeneficiaryAccountArtifacts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (artifact is null)
            return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.AccountArtifactNotFound);

        artifact.Status = request.Status;
        artifact.Notes = request.Notes?.Trim() ?? artifact.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAccountArtifact(artifact));
    }

    public async Task<Result<BeneficiaryAccountArtifactResponse>> UpdateAccountArtifactAsync(
        int id,
        UpdateBeneficiaryAccountArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await dbcontext.BeneficiaryAccountArtifacts
            .Include(x => x.BeneficiaryProfile)
            .Include(x => x.BeneficiaryDependent)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (artifact is null)
            return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.AccountArtifactNotFound);

        BeneficiaryProfile? profile = null;
        BeneficiaryDependent? dependent = null;

        if (request.BeneficiaryProfileId.HasValue)
        {
            profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken);
            if (profile is null)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.ProfileNotFound);
        }

        if (request.BeneficiaryDependentId.HasValue)
        {
            dependent = await dbcontext.BeneficiaryDependents
                .Include(x => x.BeneficiaryProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryDependentId.Value, cancellationToken);
            if (dependent is null)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.DependentNotFound);

            if (profile is not null && dependent.BeneficiaryProfileId != profile.Id)
                return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.InvalidRequest);
        }

        var holderName = request.HolderName?.Trim();
        if (string.IsNullOrWhiteSpace(holderName))
            holderName = dependent?.FullName ?? profile?.FullName;

        if (string.IsNullOrWhiteSpace(holderName))
            return Result.Failure<BeneficiaryAccountArtifactResponse>(BeneficiaryErrors.InvalidRequest);

        artifact.Type = request.Type;
        artifact.Status = request.Status;
        artifact.BeneficiaryProfileId = profile?.Id ?? dependent?.BeneficiaryProfileId;
        artifact.BeneficiaryDependentId = dependent?.Id;
        artifact.HolderName = holderName;
        artifact.Source = request.Source?.Trim();
        artifact.Payload = request.Payload?.Trim();
        artifact.Notes = request.Notes?.Trim();
        artifact.BeneficiaryProfile = profile ?? dependent?.BeneficiaryProfile;
        artifact.BeneficiaryDependent = dependent;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAccountArtifact(artifact));
    }

    public async Task<Result<IEnumerable<BeneficiaryGuardianOperationResponse>>> GetGuardianOperationsAsync(
        BeneficiaryGuardianOperationType? type = null,
        BeneficiaryOperationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryGuardianOperations.AsNoTracking().AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var operations = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapGuardianOperation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryGuardianOperationResponse>>(operations);
    }

    public async Task<Result<BeneficiaryGuardianOperationResponse>> CreateGuardianOperationAsync(
        CreateBeneficiaryGuardianOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        BeneficiaryProfile? profile = null;
        BeneficiaryGuardian? guardian = null;

        if (request.Type == BeneficiaryGuardianOperationType.ConvertBeneficiaryToGuardian)
        {
            if (!request.BeneficiaryProfileId.HasValue)
                return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.InvalidRequest);

            profile = await dbcontext.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken);
            if (profile is null)
                return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.ProfileNotFound);
        }
        else
        {
            if (!request.BeneficiaryGuardianId.HasValue)
                return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.InvalidRequest);

            guardian = await dbcontext.BeneficiaryGuardians
                .Include(x => x.BeneficiaryProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryGuardianId.Value, cancellationToken);
            if (guardian is null)
                return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.GuardianNotFound);
        }

        var subjectName = request.SubjectName?.Trim() ?? guardian?.FullName ?? profile?.FullName;
        if (string.IsNullOrWhiteSpace(subjectName))
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.InvalidRequest);

        var operation = new BeneficiaryGuardianOperation
        {
            Type = request.Type,
            BeneficiaryProfileId = profile?.Id ?? guardian?.BeneficiaryProfileId,
            BeneficiaryGuardianId = guardian?.Id,
            ReferenceNumber = await GenerateGuardianOperationReferenceAsync(cancellationToken),
            SubjectName = subjectName,
            Notes = request.Notes?.Trim(),
            BeneficiaryProfile = profile ?? guardian?.BeneficiaryProfile,
            BeneficiaryGuardian = guardian
        };

        dbcontext.BeneficiaryGuardianOperations.Add(operation);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapGuardianOperation(operation));
    }

    public async Task<Result<BeneficiaryGuardianOperationResponse>> DecideGuardianOperationAsync(
        int id,
        DecideBeneficiaryGuardianOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var operation = await dbcontext.BeneficiaryGuardianOperations
            .Include(x => x.BeneficiaryProfile)
            .Include(x => x.BeneficiaryGuardian)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (operation is null)
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.GuardianOperationNotFound);

        if (operation.Status != BeneficiaryOperationStatus.Pending)
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.OperationAlreadyDecided);

        if (!request.Approved && string.IsNullOrWhiteSpace(request.DecisionNotes))
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.InvalidRequest);

        if (!request.Approved)
        {
            operation.Status = BeneficiaryOperationStatus.Rejected;
            operation.DecisionNotes = request.DecisionNotes?.Trim();
            operation.DecidedAt = DateTime.UtcNow.AddHours(3);
            await dbcontext.SaveChangesAsync(cancellationToken);
            return Result.Success(MapGuardianOperation(operation));
        }

        operation.DecisionNotes = request.DecisionNotes?.Trim();
        var applyResult = await ApplyGuardianOperationAsync(operation, cancellationToken);
        if (applyResult.IsFailure)
            return Result.Failure<BeneficiaryGuardianOperationResponse>(applyResult.Error);

        operation.Status = BeneficiaryOperationStatus.Completed;
        operation.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapGuardianOperation(operation));
    }

    public async Task<Result<BeneficiaryGuardianOperationResponse>> CancelGuardianOperationAsync(
        int id,
        CancelBeneficiaryGuardianOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var operation = await dbcontext.BeneficiaryGuardianOperations
            .Include(x => x.BeneficiaryProfile)
            .Include(x => x.BeneficiaryGuardian)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (operation is null)
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.GuardianOperationNotFound);

        if (operation.Status != BeneficiaryOperationStatus.Pending || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure<BeneficiaryGuardianOperationResponse>(BeneficiaryErrors.InvalidRequest);

        operation.Status = BeneficiaryOperationStatus.Cancelled;
        operation.DecisionNotes = request.Reason.Trim();
        operation.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapGuardianOperation(operation));
    }

    public async Task<Result<IEnumerable<BeneficiaryUpdateBatchResponse>>> GetUpdateBatchesAsync(
        BeneficiaryUpdateBatchKind? kind = null,
        BeneficiaryOperationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryUpdateBatches.AsNoTracking().AsQueryable();

        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var batches = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapUpdateBatch(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BeneficiaryUpdateBatchResponse>>(batches);
    }

    public async Task<Result<BeneficiaryUpdateBatchResponse>> CreateUpdateBatchAsync(
        CreateBeneficiaryUpdateBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.TotalProfiles < 0)
            return Result.Failure<BeneficiaryUpdateBatchResponse>(BeneficiaryErrors.InvalidRequest);

        var batch = new BeneficiaryUpdateBatch
        {
            BatchNumber = await GenerateUpdateBatchNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            Kind = request.Kind,
            AssignedTo = request.AssignedTo?.Trim(),
            TotalProfiles = request.TotalProfiles,
            DueDate = request.DueDate?.Date,
            Notes = request.Notes?.Trim()
        };

        dbcontext.BeneficiaryUpdateBatches.Add(batch);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateBatch(batch));
    }

    public async Task<Result<BeneficiaryUpdateBatchResponse>> UpdateUpdateBatchAsync(
        int id,
        UpdateBeneficiaryUpdateBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var batch = await dbcontext.BeneficiaryUpdateBatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (batch is null)
            return Result.Failure<BeneficiaryUpdateBatchResponse>(BeneficiaryErrors.UpdateBatchNotFound);

        if (batch.Status != BeneficiaryOperationStatus.Pending || string.IsNullOrWhiteSpace(request.Title) || request.TotalProfiles < batch.CompletedProfiles)
            return Result.Failure<BeneficiaryUpdateBatchResponse>(BeneficiaryErrors.InvalidRequest);

        batch.Kind = request.Kind;
        batch.Title = request.Title.Trim();
        batch.AssignedTo = request.AssignedTo?.Trim();
        batch.TotalProfiles = request.TotalProfiles;
        batch.DueDate = request.DueDate?.Date;
        batch.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateBatch(batch));
    }

    public async Task<Result<BeneficiaryUpdateBatchResponse>> UpdateBatchProgressAsync(
        int id,
        UpdateBeneficiaryBatchProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var batch = await dbcontext.BeneficiaryUpdateBatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (batch is null)
            return Result.Failure<BeneficiaryUpdateBatchResponse>(BeneficiaryErrors.UpdateBatchNotFound);

        if (request.CompletedProfiles < 0 || request.CompletedProfiles > batch.TotalProfiles)
            return Result.Failure<BeneficiaryUpdateBatchResponse>(BeneficiaryErrors.InvalidRequest);

        batch.Status = request.Status;
        batch.CompletedProfiles = request.CompletedProfiles;
        batch.Notes = request.Notes?.Trim() ?? batch.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUpdateBatch(batch));
    }

    private async Task<string> GenerateBeneficiaryNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryProfiles.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"B-{year}-{count:0000}";
    }

    private async Task<string> GenerateArtifactReferenceAsync(BeneficiaryAccountArtifactType type, CancellationToken cancellationToken)
    {
        var prefix = type switch
        {
            BeneficiaryAccountArtifactType.Card => "CARD",
            BeneficiaryAccountArtifactType.Barcode => "BAR",
            BeneficiaryAccountArtifactType.ExternalJoin => "JOIN",
            BeneficiaryAccountArtifactType.AssociationSearch => "SRCH",
            _ => "ART"
        };
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryAccountArtifacts.CountAsync(cancellationToken) + 1;
        return $"{prefix}-{year}-{count:0000}";
    }

    private async Task<string> GenerateGuardianOperationReferenceAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryGuardianOperations.CountAsync(cancellationToken) + 1;
        return $"GOP-{year}-{count:0000}";
    }

    private async Task<string> GenerateUpdateBatchNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryUpdateBatches.CountAsync(cancellationToken) + 1;
        return $"UPD-{year}-{count:0000}";
    }

    private async Task<Result> ApplyGuardianOperationAsync(BeneficiaryGuardianOperation operation, CancellationToken cancellationToken)
    {
        switch (operation.Type)
        {
            case BeneficiaryGuardianOperationType.ConvertBeneficiaryToGuardian:
                if (operation.BeneficiaryProfile is null)
                    return Result.Failure(BeneficiaryErrors.ProfileNotFound);

                var existingGuardian = await dbcontext.BeneficiaryGuardians.AnyAsync(x =>
                    x.BeneficiaryProfileId == operation.BeneficiaryProfile.Id &&
                    !x.IsDeleted &&
                    x.FullName == operation.BeneficiaryProfile.FullName,
                    cancellationToken);

                if (!existingGuardian)
                {
                    dbcontext.BeneficiaryGuardians.Add(new BeneficiaryGuardian
                    {
                        BeneficiaryProfileId = operation.BeneficiaryProfile.Id,
                        FullName = operation.BeneficiaryProfile.FullName,
                        NationalId = operation.BeneficiaryProfile.NationalId,
                        Mobile = operation.BeneficiaryProfile.Mobile,
                        Relationship = "تحويل من مستفيد",
                        Notes = operation.ReferenceNumber,
                        BeneficiaryProfile = operation.BeneficiaryProfile
                    });
                }

                return Result.Success();

            case BeneficiaryGuardianOperationType.ConvertGuardianToBeneficiary:
                if (operation.BeneficiaryGuardian is null)
                    return Result.Failure(BeneficiaryErrors.GuardianNotFound);

                var beneficiaryNumber = await GenerateBeneficiaryNumberAsync(cancellationToken);
                dbcontext.BeneficiaryProfiles.Add(new BeneficiaryProfile
                {
                    BeneficiaryNumber = beneficiaryNumber,
                    FullName = operation.BeneficiaryGuardian.FullName,
                    NationalId = operation.BeneficiaryGuardian.NationalId,
                    Mobile = operation.BeneficiaryGuardian.Mobile,
                    Category = "محول من وصي",
                    FamilyMembersCount = 1,
                    Notes = operation.ReferenceNumber
                });
                return Result.Success();

            case BeneficiaryGuardianOperationType.RemoveGuardian:
                if (operation.BeneficiaryGuardian is null)
                    return Result.Failure(BeneficiaryErrors.GuardianNotFound);

                operation.BeneficiaryGuardian.IsDeleted = true;
                operation.BeneficiaryGuardian.IsPrimary = false;
                operation.BeneficiaryGuardian.DeletedAt = DateTime.UtcNow.AddHours(3);
                operation.BeneficiaryGuardian.DeleteReason = string.IsNullOrWhiteSpace(operation.DecisionNotes)
                    ? operation.Notes
                    : operation.DecisionNotes;
                return Result.Success();

            default:
                return Result.Failure(BeneficiaryErrors.InvalidRequest);
        }
    }

    private static Result ApplyFieldUpdate(BeneficiaryProfile profile, string requestedField, string? requestedValue)
    {
        var value = requestedValue?.Trim();
        var field = NormalizeFieldName(requestedField);

        switch (field)
        {
            case "fullname":
            case "name":
            case "الاسم":
                if (string.IsNullOrWhiteSpace(value))
                    return Result.Failure(BeneficiaryErrors.InvalidRequest);
                profile.FullName = value;
                return Result.Success();
            case "nationalid":
            case "identity":
            case "الهوية":
                profile.NationalId = value;
                return Result.Success();
            case "gender":
            case "الجنس":
                profile.Gender = value;
                return Result.Success();
            case "birthdate":
            case "تاريخالميلاد":
                if (string.IsNullOrWhiteSpace(value))
                {
                    profile.BirthDate = null;
                    return Result.Success();
                }

                if (!DateTime.TryParse(value, out var birthDate))
                    return Result.Failure(BeneficiaryErrors.InvalidRequest);

                profile.BirthDate = birthDate.Date;
                return Result.Success();
            case "mobile":
            case "phone":
            case "الجوال":
                profile.Mobile = value;
                return Result.Success();
            case "email":
            case "البريد":
                profile.Email = value;
                return Result.Success();
            case "city":
            case "المدينة":
                profile.City = value;
                return Result.Success();
            case "address":
            case "العنوان":
                profile.Address = value;
                return Result.Success();
            case "category":
            case "الفئة":
                profile.Category = value;
                return Result.Success();
            case "grade":
            case "الدرجة":
                profile.Grade = value;
                return Result.Success();
            case "monthlyincome":
            case "الدخلالشهري":
                if (!decimal.TryParse(value, out var monthlyIncome) || monthlyIncome < 0)
                    return Result.Failure(BeneficiaryErrors.InvalidRequest);
                profile.MonthlyIncome = monthlyIncome;
                return Result.Success();
            case "familymemberscount":
            case "familycount":
            case "عددأفرادالأسرة":
                if (!int.TryParse(value, out var familyMembersCount) || familyMembersCount < 0)
                    return Result.Failure(BeneficiaryErrors.InvalidRequest);
                profile.FamilyMembersCount = familyMembersCount;
                return Result.Success();
            case "status":
            case "الحالة":
                if (!Enum.TryParse<BeneficiaryStatus>(value, true, out var status))
                    return Result.Failure(BeneficiaryErrors.InvalidRequest);
                profile.Status = status;
                return Result.Success();
            default:
                return Result.Success();
        }
    }

    private static string? ReadFieldValue(BeneficiaryProfile profile, string requestedField)
    {
        var field = NormalizeFieldName(requestedField);
        return field switch
        {
            "fullname" or "name" or "الاسم" => profile.FullName,
            "nationalid" or "identity" or "الهوية" => profile.NationalId,
            "gender" or "الجنس" => profile.Gender,
            "birthdate" or "تاريخالميلاد" => profile.BirthDate?.ToString("yyyy-MM-dd"),
            "mobile" or "phone" or "الجوال" => profile.Mobile,
            "email" or "البريد" => profile.Email,
            "city" or "المدينة" => profile.City,
            "address" or "العنوان" => profile.Address,
            "category" or "الفئة" => profile.Category,
            "grade" or "الدرجة" => profile.Grade,
            "monthlyincome" or "الدخلالشهري" => profile.MonthlyIncome.ToString("0.##"),
            "familymemberscount" or "familycount" or "عددأفرادالأسرة" => profile.FamilyMembersCount.ToString(),
            "status" or "الحالة" => profile.Status.ToString(),
            _ => null
        };
    }

    private static string NormalizeFieldName(string field) =>
        field.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();

    private static BeneficiaryResponse MapProfile(BeneficiaryProfile profile) =>
        new(
            profile.Id,
            profile.BeneficiaryNumber,
            profile.FullName,
            profile.NationalId,
            profile.Gender,
            profile.BirthDate,
            profile.Mobile,
            profile.Email,
            profile.City,
            profile.Address,
            profile.Category,
            profile.Grade,
            profile.Status.ToString(),
            profile.MonthlyIncome,
            profile.FamilyMembersCount,
            profile.Notes,
            profile.ArchivedAt,
            profile.ArchiveReason,
            profile.CreatedAt);

    private static BeneficiaryDependentResponse MapDependent(BeneficiaryDependent dependent) =>
        new(
            dependent.Id,
            dependent.BeneficiaryProfileId,
            dependent.BeneficiaryProfile?.FullName ?? string.Empty,
            dependent.FullName,
            dependent.NationalId,
            dependent.Relationship,
            dependent.BirthDate,
            dependent.Category,
            dependent.Grade,
            dependent.IsActive,
            dependent.Notes);

    private static BeneficiaryGuardianResponse MapGuardian(BeneficiaryGuardian guardian) =>
        new(
            guardian.Id,
            guardian.BeneficiaryProfileId,
            guardian.BeneficiaryProfile?.FullName ?? string.Empty,
            guardian.FullName,
            guardian.NationalId,
            guardian.Mobile,
            guardian.Relationship,
            guardian.IsPrimary,
            guardian.IsDeleted,
            guardian.DeletedAt,
            guardian.DeleteReason,
            guardian.Notes);

    private static BeneficiaryUpdateRequestResponse MapUpdateRequest(BeneficiaryUpdateRequest request) =>
        new(
            request.Id,
            request.BeneficiaryProfileId,
            request.BeneficiaryProfile?.FullName ?? string.Empty,
            request.RequestedField,
            request.CurrentValue,
            request.RequestedValue,
            request.Reason,
            request.Status.ToString(),
            request.DecisionNotes,
            request.DecidedAt,
            request.CreatedAt);

    private static BeneficiaryEntityResponse MapEntity(BeneficiaryEntity entity) =>
        new(
            entity.Id,
            entity.NameAr,
            entity.NameEn,
            entity.ContactPerson,
            entity.Mobile,
            entity.Email,
            entity.City,
            entity.Address,
            entity.Status.ToString(),
            entity.Notes);

    private static BeneficiaryAccountArtifactResponse MapAccountArtifact(BeneficiaryAccountArtifact artifact) =>
        new(
            artifact.Id,
            artifact.Type.ToString(),
            artifact.Status.ToString(),
            artifact.BeneficiaryProfileId,
            artifact.BeneficiaryDependentId,
            artifact.ReferenceNumber,
            artifact.HolderName,
            artifact.Source,
            artifact.Payload,
            artifact.Notes,
            artifact.CreatedAt);

    private static BeneficiaryGuardianOperationResponse MapGuardianOperation(BeneficiaryGuardianOperation operation) =>
        new(
            operation.Id,
            operation.Type.ToString(),
            operation.Status.ToString(),
            operation.BeneficiaryProfileId,
            operation.BeneficiaryGuardianId,
            operation.ReferenceNumber,
            operation.SubjectName,
            operation.Notes,
            operation.DecisionNotes,
            operation.DecidedAt,
            operation.CreatedAt);

    private static BeneficiaryUpdateBatchResponse MapUpdateBatch(BeneficiaryUpdateBatch batch) =>
        new(
            batch.Id,
            batch.BatchNumber,
            batch.Title,
            batch.Kind.ToString(),
            batch.Status.ToString(),
            batch.AssignedTo,
            batch.TotalProfiles,
            batch.CompletedProfiles,
            batch.DueDate,
            batch.Notes,
            batch.CreatedAt);
}
