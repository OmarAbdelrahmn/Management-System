using Application.Contracts.Beneficiaries;
using Application.Service.Beneficiaries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

public class BeneficiaryServiceTests
{
    [Fact]
    public async Task CreateAsync_GeneratesBeneficiaryNumberAndDashboardCountsProfile()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BeneficiaryService(dbcontext);

        var result = await service.CreateAsync(new CreateBeneficiaryRequest(
            "أحمد محمد",
            null,
            "1002003000",
            "ذكر",
            new DateTime(1990, 1, 1),
            "0500000000",
            "ahmad@example.com",
            "الرياض",
            "حي الاختبار",
            "أ",
            "1",
            1500,
            3,
            null));

        var dashboard = await service.GetDashboardAsync();

        Assert.True(result.IsSuccess);
        Assert.StartsWith("B-2026-", result.Value.BeneficiaryNumber);
        Assert.True(dashboard.IsSuccess);
        Assert.Equal(1, dashboard.Value.ProfilesCount);
        Assert.Equal(1, dashboard.Value.ActiveProfilesCount);
    }

    [Fact]
    public async Task ArchiveAndRestoreAsync_UpdatesProfileStatus()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var profile = await SeedProfileAsync(dbcontext);
        var service = new BeneficiaryService(dbcontext);

        var archived = await service.ArchiveAsync(profile.Id, new ArchiveBeneficiaryRequest("ملف مكرر"));
        var afterArchive = await service.GetAsync(profile.Id);
        var restored = await service.RestoreAsync(profile.Id);
        var afterRestore = await service.GetAsync(profile.Id);

        Assert.True(archived.IsSuccess);
        Assert.Equal("Archived", afterArchive.Value.Status);
        Assert.True(restored.IsSuccess);
        Assert.Equal("Active", afterRestore.Value.Status);
    }

    [Fact]
    public async Task AddGuardianAsync_PrimaryGuardianReplacesPreviousPrimary()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var profile = await SeedProfileAsync(dbcontext);
        var service = new BeneficiaryService(dbcontext);

        await service.AddGuardianAsync(new AddBeneficiaryGuardianRequest(profile.Id, "الوصي الأول", "111", "0501", "أخ", true, null));
        await service.AddGuardianAsync(new AddBeneficiaryGuardianRequest(profile.Id, "الوصي الثاني", "222", "0502", "ابن", true, null));

        var guardians = await service.GetGuardiansAsync(profile.Id);

        Assert.True(guardians.IsSuccess);
        var primaryGuardian = Assert.Single(guardians.Value, x => x.IsPrimary);
        Assert.Equal("الوصي الثاني", primaryGuardian.FullName);
    }

    [Fact]
    public async Task DecideUpdateRequestAsync_ApprovalAppliesKnownField()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var profile = await SeedProfileAsync(dbcontext);
        var service = new BeneficiaryService(dbcontext);
        var request = await service.CreateUpdateRequestAsync(new CreateBeneficiaryUpdateRequest(profile.Id, "Mobile", null, "0599999999", "تحديث رقم التواصل"));

        var decision = await service.DecideUpdateRequestAsync(request.Value.Id, new DecideBeneficiaryUpdateRequest(true, "موافق"));
        var updatedProfile = await dbcontext.BeneficiaryProfiles.AsNoTracking().SingleAsync(x => x.Id == profile.Id);

        Assert.True(decision.IsSuccess);
        Assert.Equal("Approved", decision.Value.Status);
        Assert.Equal("0599999999", updatedProfile.Mobile);
    }

    [Fact]
    public async Task SaveEntityAsync_RejectsDuplicateArabicName()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BeneficiaryService(dbcontext);
        var request = new UpsertBeneficiaryEntityRequest("جمعية الاختبار", null, "ممثل", "0500", null, "جدة", null, BeneficiaryEntityStatus.Active, null);

        var first = await service.SaveEntityAsync(null, request);
        var duplicate = await service.SaveEntityAsync(null, request);

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task CreateAccountArtifactAsync_CreatesCardAndUpdatesStatus()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var profile = await SeedProfileAsync(dbcontext);
        var service = new BeneficiaryService(dbcontext);

        var created = await service.CreateAccountArtifactAsync(new CreateBeneficiaryAccountArtifactRequest(
            BeneficiaryAccountArtifactType.Card,
            profile.Id,
            null,
            null,
            "Rafed",
            "بطاقة اختبار",
            null));
        var issued = await service.UpdateAccountArtifactStatusAsync(created.Value.Id, new UpdateBeneficiaryAccountArtifactStatusRequest(BeneficiaryAccountArtifactStatus.Issued, "تم الإصدار"));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(created.IsSuccess);
        Assert.StartsWith("CARD-2026-", created.Value.ReferenceNumber);
        Assert.Equal("Ready", created.Value.Status);
        Assert.True(issued.IsSuccess);
        Assert.Equal("Issued", issued.Value.Status);
        Assert.True(dashboard.IsSuccess);
        Assert.Equal(1, dashboard.Value.AccountArtifactsCount);
    }

    [Fact]
    public async Task DecideGuardianOperationAsync_RemoveGuardianSoftDeletesGuardian()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var profile = await SeedProfileAsync(dbcontext);
        var service = new BeneficiaryService(dbcontext);
        var guardian = await service.AddGuardianAsync(new AddBeneficiaryGuardianRequest(profile.Id, "وصي للحذف", "333", "0503", "أخ", true, null));

        var operation = await service.CreateGuardianOperationAsync(new CreateBeneficiaryGuardianOperationRequest(
            BeneficiaryGuardianOperationType.RemoveGuardian,
            null,
            guardian.Value.Id,
            null,
            "طلب حذف وصي"));
        var decided = await service.DecideGuardianOperationAsync(operation.Value.Id, new DecideBeneficiaryGuardianOperationRequest(true, "اعتماد الحذف"));
        var activeGuardians = await service.GetGuardiansAsync(profile.Id);

        Assert.True(operation.IsSuccess);
        Assert.True(decided.IsSuccess);
        Assert.Equal("Completed", decided.Value.Status);
        Assert.Empty(activeGuardians.Value);
        Assert.True(await dbcontext.BeneficiaryGuardians.IgnoreQueryFilters().AnyAsync(x => x.Id == guardian.Value.Id && x.IsDeleted));
    }

    [Fact]
    public async Task CreateUpdateBatchAsync_TracksProgress()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BeneficiaryService(dbcontext);

        var created = await service.CreateUpdateBatchAsync(new CreateBeneficiaryUpdateBatchRequest(
            BeneficiaryUpdateBatchKind.SelfService,
            "تحديث ذاتي ربع سنوي",
            "فريق المستفيدين",
            12,
            new DateTime(2026, 8, 1),
            null));
        var progressed = await service.UpdateBatchProgressAsync(created.Value.Id, new UpdateBeneficiaryBatchProgressRequest(BeneficiaryOperationStatus.Approved, 5, "جاري التنفيذ"));

        Assert.True(created.IsSuccess);
        Assert.StartsWith("UPD-2026-", created.Value.BatchNumber);
        Assert.True(progressed.IsSuccess);
        Assert.Equal("Approved", progressed.Value.Status);
        Assert.Equal(5, progressed.Value.CompletedProfiles);
    }

    private static async Task<BeneficiaryProfile> SeedProfileAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var profile = new BeneficiaryProfile
        {
            BeneficiaryNumber = $"B-{Guid.NewGuid():N}",
            FullName = "مستفيد تجريبي",
            Mobile = "0500000000",
            City = "الرياض",
            Category = "أ",
            FamilyMembersCount = 1
        };

        dbcontext.BeneficiaryProfiles.Add(profile);
        await dbcontext.SaveChangesAsync();
        return profile;
    }
}
