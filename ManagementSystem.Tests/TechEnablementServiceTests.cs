using Application.Contracts.TechEnablement;
using Application.Service.TechEnablement;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class TechEnablementServiceTests
{
    [Fact]
    public async Task SaveSettingAsync_CreatesSearchableSetting()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new TechEnablementService(dbcontext);

        var saved = await service.SaveSettingAsync(null, new SaveTechSystemSettingRequest("Website.SeoTitle", "عنوان SEO", "Website", "جمعية", null, TechSettingStatus.Active));
        var settings = await service.GetSettingsAsync("Website");
        var dashboard = await service.GetDashboardAsync();

        Assert.True(saved.IsSuccess);
        Assert.Single(settings.Value);
        Assert.Equal(1, dashboard.Value.SettingsCount);
    }

    [Fact]
    public async Task OrganizationAndVisualAssets_AreSavedAndFiltered()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new TechEnablementService(dbcontext);

        var assignment = await service.SaveOrganizationAssignmentAsync(null, new SaveOrganizationAssignmentRequest("اللجنة التقنية", "مدير النظام", OrganizationAssignmentType.Committee, "رئيس اللجنة", DateTime.UtcNow.AddHours(3), null, true, null));
        var asset = await service.SaveVisualAssetAsync(null, new SaveVisualAssetTemplateRequest("شهادة تبرع", VisualAssetType.Certificate, "/files/cert.png", "{}", true, null));
        var committees = await service.GetOrganizationAssignmentsAsync(OrganizationAssignmentType.Committee);
        var certificates = await service.GetVisualAssetsAsync(VisualAssetType.Certificate);

        Assert.True(assignment.IsSuccess);
        Assert.True(asset.IsSuccess);
        Assert.Single(committees.Value);
        Assert.Single(certificates.Value);
    }

    [Fact]
    public async Task CybersecurityAndNcnp_WorkflowsUpdateDashboard()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new TechEnablementService(dbcontext);

        var review = await service.SaveCybersecurityReviewAsync(null, new SaveCybersecurityReviewRequest("الصلاحيات", "صلاحية حساسة بحاجة مراجعة", "High", CybersecurityReviewStatus.Open, "مسؤول الأمن", DateTime.UtcNow.AddHours(3).AddDays(7), "مراجعة الدور"));
        var ncnp = await service.SaveNcnpDataAsync(null, new SaveNcnpDataRecordRequest("HELP-1", "مستفيد", "إعانة مالية", DateTime.UtcNow.AddHours(3), 500, NcnpDataStatus.NeedsUpdate, null, null));
        var update = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.ReadyToRegister, "NCNP-1", "جاهز"));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(review.IsSuccess);
        Assert.True(ncnp.IsSuccess);
        Assert.True(update.IsSuccess);
        Assert.Equal(1, dashboard.Value.OpenSecurityReviewsCount);
        Assert.Equal(1, dashboard.Value.NcnpReadyCount);
    }

    [Fact]
    public async Task NcnpWorkflow_RequiresReadyStatusAndPlatformReferenceBeforeRegistration()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new TechEnablementService(dbcontext);
        var ncnp = await service.SaveNcnpDataAsync(null, new SaveNcnpDataRecordRequest("HELP-2", "مستفيد", "إعانة مالية", new DateTime(2026, 7, 8), 500, NcnpDataStatus.NeedsUpdate, null, null));

        var directRegistration = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.Registered, "NCNP-DIRECT", "محاولة مباشرة"));
        var ready = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.ReadyToRegister, null, "جاهز للإرسال"));
        var missingPlatformReference = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.Registered, null, "بدون مرجع منصة"));
        var registered = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.Registered, "NCNP-2", "تم التسجيل"));
        var archivedRegistered = await service.UpdateNcnpStatusAsync(ncnp.Value.Id, new UpdateNcnpDataStatusRequest(NcnpDataStatus.ArchivedExternalSupport, null, "أرشفة بعد التسجيل"));
        var registeredRecords = await service.GetNcnpDataAsync(NcnpDataStatus.Registered);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(ncnp.IsSuccess);
        Assert.False(directRegistration.IsSuccess);
        Assert.True(ready.IsSuccess);
        Assert.False(missingPlatformReference.IsSuccess);
        Assert.True(registered.IsSuccess);
        Assert.False(archivedRegistered.IsSuccess);
        var registeredRecord = Assert.Single(registeredRecords.Value);
        Assert.Equal("NCNP-2", registeredRecord.PlatformReference);
        Assert.Equal(0, dashboard.Value.NcnpReadyCount);
    }

    [Fact]
    public async Task UpdateCybersecurityStatusAsync_ValidatesAndClosesReview()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new TechEnablementService(dbcontext);
        var review = await service.SaveCybersecurityReviewAsync(null, new SaveCybersecurityReviewRequest("الصلاحيات", "صلاحية عالية", "Critical", CybersecurityReviewStatus.Open, null, null, null));

        var invalidMitigation = await service.UpdateCybersecurityStatusAsync(review.Value.Id, new UpdateCybersecurityReviewStatusRequest(CybersecurityReviewStatus.Mitigated, null, null, null));
        var inReview = await service.UpdateCybersecurityStatusAsync(review.Value.Id, new UpdateCybersecurityReviewStatusRequest(CybersecurityReviewStatus.InReview, "مسؤول الأمن", new DateTime(2026, 7, 20), null));
        var mitigated = await service.UpdateCybersecurityStatusAsync(review.Value.Id, new UpdateCybersecurityReviewStatusRequest(CybersecurityReviewStatus.Mitigated, null, null, "تم تقليل الصلاحيات"));
        var dashboard = await service.GetDashboardAsync();

        Assert.False(invalidMitigation.IsSuccess);
        Assert.Equal("InReview", inReview.Value.Status);
        Assert.Equal("Mitigated", mitigated.Value.Status);
        Assert.Equal("تم تقليل الصلاحيات", mitigated.Value.MitigationPlan);
        Assert.Equal(0, dashboard.Value.OpenSecurityReviewsCount);
    }
}
