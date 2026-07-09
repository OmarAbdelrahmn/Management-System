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
}
