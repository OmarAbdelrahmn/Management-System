using Application.Service.SystemCatalog;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

public class SystemCatalogServiceTests
{
    [Fact]
    public async Task SeedFirstPagesAsync_CreatesFullRafedHierarchy()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        var result = await service.SeedFirstPagesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(18, result.Value.TotalModules);
        Assert.Equal(84, result.Value.TotalGroups);
        Assert.Equal(688, result.Value.TotalPages);
        Assert.Equal(18, await dbcontext.SystemModules.CountAsync());
        Assert.Equal(84, await dbcontext.SystemPageGroups.CountAsync());
        Assert.Equal(688, await dbcontext.SystemPages.CountAsync());
    }

    [Fact]
    public async Task SeedFirstPagesAsync_IsIdempotentAndPreservesExactCounts()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();
        var second = await service.SeedFirstPagesAsync();

        Assert.True(second.IsSuccess);
        Assert.Equal(0, second.Value.ModulesCreated);
        Assert.Equal(0, second.Value.GroupsCreated);
        Assert.Equal(0, second.Value.PagesCreated);
        Assert.Equal(688, second.Value.PagesUpdated);
        Assert.Equal(18, second.Value.TotalModules);
        Assert.Equal(84, second.Value.TotalGroups);
        Assert.Equal(688, second.Value.TotalPages);
    }

    [Fact]
    public async Task GetNavigationAsync_ReturnsOrderedRafedSidebar()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);
        await service.SeedFirstPagesAsync();

        var result = await service.GetNavigationAsync();

        Assert.True(result.IsSuccess);
        var modules = result.Value.ToList();
        Assert.Equal("الإدارة الإشرافية و التنفيذية", modules[0].NameAr);
        Assert.Equal("إدارة التأسيس", modules[0].Groups[0].NameAr);
        Assert.Equal("خارطة الملفات المؤسسية", modules[0].Groups[0].Pages[0].NameAr);
        Assert.Equal("إدارة التطوع", modules[^1].NameAr);
    }

    [Fact]
    public async Task GetRouteAccessAsync_RecognizesSharedMappedRoutesAndNormalizesQueries()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);
        await service.SeedFirstPagesAsync();

        var result = await service.GetRouteAccessAsync("/tasks/manage?source=header");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsCatalogRoute);
        Assert.True(result.Value.IsAllowed);
        Assert.Contains("system.executive-supervision.task_update", result.Value.PermissionKeys);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsBeneficiaryAccountsModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var createProfile = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "profiles_create.php");
        var cards = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "profiles_cards.php");
        var guardianRemove = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "guardians_remove.php");
        var updateTasks = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "update_task_create.php");
        var entityExternal = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "gehat_accounts_external.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "beneficiary-accounts")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/beneficiaries/create", createProfile.Route);
        Assert.Equal("BeneficiaryService", createProfile.ServiceName);
        Assert.Equal(SystemPageStatus.Implemented, createProfile.Status);
        Assert.Equal("/beneficiaries/cards", cards.Route);
        Assert.Equal("/beneficiaries/guardian-operations", guardianRemove.Route);
        Assert.Equal("/beneficiaries/update-batches", updateTasks.Route);
        Assert.Equal("/beneficiaries/entities", entityExternal.Route);
        Assert.Equal(SystemPageStatus.Implemented, updateTasks.Status);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsParticipatingMembersModuleToParticipationRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var boardCreate = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "hr_board_create.php");
        var generalAssembly = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "board_database.php");
        var payments = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "board_payments.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "participating-members")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/members/participation", boardCreate.Route);
        Assert.Equal("/members/participation", generalAssembly.Route);
        Assert.Equal("/members/payments", payments.Route);
        Assert.Equal("MemberService", boardCreate.ServiceName);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsHumanResourcesModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var evaluation = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "hr_personnel_evaluation.php");
        var recruitment = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "hr_request_interview.php");
        var humanResourcesModuleId = await dbcontext.SystemModules
            .Where(x => x.Key == "human-resources")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == humanResourcesModuleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/hr/evaluations", evaluation.Route);
        Assert.Equal(SystemPageStatus.Implemented, evaluation.Status);
        Assert.Equal("/hr/recruitment", recruitment.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsProgramsProjectsModuleToServiceRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var projectCreate = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "projects_create.php");
        var supplierDatabase = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "suppliers_database.php");
        var gantt = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "projects_gantt.php");
        var programsModuleId = await dbcontext.SystemModules
            .Where(x => x.Key == "programs-projects-designs")
            .Select(x => x.Id)
            .SingleAsync();
        var plannedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == programsModuleId && x.Status == SystemPageStatus.Planned);
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == programsModuleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/programs-projects/create", projectCreate.Route);
        Assert.Equal(SystemPageStatus.Implemented, projectCreate.Status);
        Assert.Equal("/programs-projects/suppliers", supplierDatabase.Route);
        Assert.Equal(SystemPageStatus.Implemented, supplierDatabase.Status);
        Assert.Equal("/programs-projects/monitoring", gantt.Route);
        Assert.Equal(SystemPageStatus.Implemented, gantt.Status);
        Assert.Equal(0, plannedCount);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsBeneficiaryServicesModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var aidCreate = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "request_create.php");
        var couponDeliver = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "coupons_deliver.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "beneficiary-services")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/beneficiary-services/aid-requests", aidCreate.Route);
        Assert.Equal(SystemPageStatus.Implemented, aidCreate.Status);
        Assert.Equal("/beneficiary-services/coupons", couponDeliver.Route);
        Assert.Equal(SystemPageStatus.Implemented, couponDeliver.Status);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsAccountingModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var settings = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "finance_settings.php");
        var donation = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "sanad_donation.php");
        var reports = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "finance_report_daily.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "accounting")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/accounting/setup", settings.Route);
        Assert.Equal("/accounting/receipts", donation.Route);
        Assert.Equal("/accounting/reports", reports.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsPublicRelationsMediaModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var partners = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "media_partners.php");
        var sms = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "channel_sms.php");
        var content = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "website_news.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "pr-media")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/pr-media/relations", partners.Route);
        Assert.Equal("/pr-media/channels", sms.Route);
        Assert.Equal("/pr-media/content", content.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsFinancialDevelopmentModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var supporters = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "projects_supporters_manage.php");
        var fundraising = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "marketing_project.php");
        var campaign = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "marketing_campaigns_database.php");
        var report = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "donations_device_report.php");
        var awqaf = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "awqaf_invoices_due.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "financial-development")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/financial-development/supporters", supporters.Route);
        Assert.Equal("/financial-development/fundraising", fundraising.Route);
        Assert.Equal("/financial-development/digital-marketing", campaign.Route);
        Assert.Equal("/financial-development/reports", report.Route);
        Assert.Equal("/financial-development/endowments", awqaf.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsInstitutionalExcellenceModulesToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var pps = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "pps_report.php");
        var governance = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "governance_answers.php");
        var strategy = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "strategy_balance_indicators_main.php");
        var performanceModuleId = await dbcontext.SystemModules
            .Where(x => x.Key == "excellence-performance")
            .Select(x => x.Id)
            .SingleAsync();
        var governanceModuleId = await dbcontext.SystemModules
            .Where(x => x.Key == "excellence-governance")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => (x.SystemModuleId == performanceModuleId || x.SystemModuleId == governanceModuleId) && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/institutional-excellence/performance", pps.Route);
        Assert.Equal("/institutional-excellence/governance", governance.Route);
        Assert.Equal("/institutional-excellence/strategy", strategy.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsEvaluationFollowUpModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var request = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "followup_request.php");
        var running = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "followup_zap_database_running.php");
        var activity = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "followup_records_supporter.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "evaluation-followup")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/evaluation-followup/cases", request.Route);
        Assert.Equal("/evaluation-followup/cases", running.Route);
        Assert.Equal("/evaluation-followup/activities", activity.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsMovementMaintenanceModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var fleet = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "cars_management.php");
        var hand = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "cars_hand.php");
        var maintenance = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "maintenance_request.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "movement-maintenance")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/movement-maintenance/fleet", fleet.Route);
        Assert.Equal("/movement-maintenance/fleet", hand.Route);
        Assert.Equal("/movement-maintenance/maintenance", maintenance.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsReportsStatisticsModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var report = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "report_users.php");
        var projectFinance = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "projects_finance_reports.php");
        var statistic = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "statistics_finance.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "reports-statistics")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/reports-statistics/catalog", report.Route);
        Assert.Equal("/reports-statistics/catalog", projectFinance.Route);
        Assert.Equal("/reports-statistics/statistics", statistic.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsDocumentationArchiveModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var meetingApproval = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "meetings_approval.php");
        var meetingArchive = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "meetings_database_general.php");
        var archive = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "archives_secret.php");
        var mail = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "mail_operations_database.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "documentation-archive")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/meetings/approvals", meetingApproval.Route);
        Assert.Equal("/meetings/archive", meetingArchive.Route);
        Assert.Equal("/documentation-archive/archives", archive.Route);
        Assert.Equal("/documentation-archive/mail", mail.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsVolunteeringModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var users = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "volunteer_users_manage.php");
        var external = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "volunteer_requests_external.php");
        var opportunities = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "volunteer_chances_manage.php");
        var attendance = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "volunteer_manage_attendance.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "volunteering")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/volunteering/portal", users.Route);
        Assert.Equal("/volunteering/portal", external.Route);
        Assert.Equal("/volunteering/opportunities", opportunities.Route);
        Assert.Equal("/volunteering/opportunities", attendance.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsTechEnablementModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var systemInfo = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "system_information.php");
        var website = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "seo_settings.php");
        var organization = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "system_organization_committees.php");
        var visual = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "system_template_certificates.php");
        var communication = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "system_channel_records_whatsapp.php");
        var security = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "sensitive_pages_report.php");
        var ncnp = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "ncnp_helps_register.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "tech-enablement")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/tech-enablement/system", systemInfo.Route);
        Assert.Equal("/tech-enablement/website", website.Route);
        Assert.Equal("/tech-enablement/organization", organization.Route);
        Assert.Equal("/tech-enablement/visual-assets", visual.Route);
        Assert.Equal("/tech-enablement/communication", communication.Route);
        Assert.Equal("/tech-enablement/cybersecurity", security.Route);
        Assert.Equal("/tech-enablement/ncnp", ncnp.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsExecutiveSupervisionModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var foundation = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "foundation_helper.php");
        var committee = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "request_credit.php");
        var approval = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "forms_purchase.php");
        var tameed = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "tameed_database.php");
        var canceledNotifications = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "notifications_canceled.php");
        var taskCreate = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "task_create.php");
        var taskComplete = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "task_complete.php");
        var taskRedirect = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "task_redirect.php");
        var taskDatabase = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "tasks_database.php");
        var taskRestore = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "task_removed.php");
        var decisions = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "decisions_export.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "executive-supervision")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/executive-supervision/foundation", foundation.Route);
        Assert.Equal("/executive-supervision/aid-committee", committee.Route);
        Assert.Equal("/executive-supervision/approvals", approval.Route);
        Assert.Equal("/executive-supervision/payment-authorizations", tameed.Route);
        Assert.Equal("/notifications/database", canceledNotifications.Route);
        Assert.Equal("/tasks/create", taskCreate.Route);
        Assert.Equal("/tasks/complete", taskComplete.Route);
        Assert.Equal("/tasks/redirect", taskRedirect.Route);
        Assert.Equal("/tasks/database", taskDatabase.Route);
        Assert.Equal("/tasks/restore", taskRestore.Route);
        Assert.Equal("/executive-supervision/decisions", decisions.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsElectronicOfficeModuleToImplementedRoutes()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var attendance = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "personnel_attendance.php");
        var admin = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "common_evaluation.php");
        var mail = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "personnel_emails_preferences.php");
        var taskMine = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "common_tasks.php");
        var taskCreate = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "common_tasks_create.php");
        var taskComplete = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "common_tasks_complete.php");
        var taskRemove = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "common_tasks_remove.php");
        var request = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "demands_request_finance.php");
        var transaction = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "operations_step_required.php");
        var report = await dbcontext.SystemPages.SingleAsync(x => x.OriginalHref == "personnel_ngo_information.php");
        var moduleId = await dbcontext.SystemModules
            .Where(x => x.Key == "electronic-office")
            .Select(x => x.Id)
            .SingleAsync();
        var nonImplementedCount = await dbcontext.SystemPages
            .CountAsync(x => x.SystemModuleId == moduleId && x.Status != SystemPageStatus.Implemented);

        Assert.Equal("/electronic-office/services", attendance.Route);
        Assert.Equal("/electronic-office/admin-office", admin.Route);
        Assert.Equal("/electronic-office/services", mail.Route);
        Assert.Equal("/tasks/mine", taskMine.Route);
        Assert.Equal("/tasks/create", taskCreate.Route);
        Assert.Equal("/tasks/complete", taskComplete.Route);
        Assert.Equal("/tasks/delete", taskRemove.Route);
        Assert.Equal("/electronic-office/requests", request.Route);
        Assert.Equal("/electronic-office/transactions", transaction.Route);
        Assert.Equal("/electronic-office/reports", report.Route);
        Assert.Equal(0, nonImplementedCount);
    }

    [Fact]
    public async Task SeedFirstPagesAsync_MapsEveryRafedPageToImplementedRoute()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);

        await service.SeedFirstPagesAsync();

        var gaps = await dbcontext.SystemModules
            .Select(module => new
            {
                module.Key,
                module.NameAr,
                Count = module.Pages.Count(page => page.Status != SystemPageStatus.Implemented)
            })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        Assert.True(gaps.Count == 0, string.Join(Environment.NewLine, gaps.Select(x => $"{x.Key} | {x.NameAr}: {x.Count}")));
    }

    [Fact]
    public async Task SeedFirstPagesAsync_ImplementedRoutesExistAsBlazorPages()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext);
        await service.SeedFirstPagesAsync();

        var pageRoutes = GetBlazorPageRoutes();
        var catalogRoutes = await dbcontext.SystemPages
            .Where(x => x.Status == SystemPageStatus.Implemented)
            .Select(x => x.Route)
            .Distinct()
            .ToListAsync();

        var missingRoutes = catalogRoutes
            .Where(route => !pageRoutes.Contains(route))
            .OrderBy(route => route)
            .ToList();

        Assert.True(missingRoutes.Count == 0, string.Join(Environment.NewLine, missingRoutes));
    }

    [Fact]
    public async Task SeedFirstPagesAsync_PrunesOldPartialCatalogRows()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.SystemModules.Add(new SystemModule
        {
            Key = "old-partial-module",
            NameAr = "قديم",
            NameEn = "Old",
            Description = "Old partial seed",
            Pages = { new SystemPage { Key = "old.page", NameAr = "قديم", Route = "/old", PermissionKey = "old", ServiceName = "Old", ServicePlan = "", UiPlan = "" } }
        });
        await dbcontext.SaveChangesAsync();

        var service = new SystemCatalogService(dbcontext);
        await service.SeedFirstPagesAsync();

        Assert.False(await dbcontext.SystemModules.AnyAsync(x => x.Key == "old-partial-module"));
        Assert.False(await dbcontext.SystemPages.AnyAsync(x => x.Key == "old.page"));
    }

    [Fact]
    public async Task GetRouteAccessAsync_AuditsDeniedDirectRouteWithoutDuplicateSpam()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new SystemCatalogService(dbcontext, new CatalogUserContext("operator-1", ["Operator"]));
        await service.SeedFirstPagesAsync();

        var first = await service.GetRouteAccessAsync("/beneficiaries/create");
        var second = await service.GetRouteAccessAsync("/beneficiaries/create");

        Assert.True(first.IsSuccess);
        Assert.False(first.Value.IsAllowed);
        Assert.False(second.Value.IsAllowed);
        var audit = Assert.Single(dbcontext.AuditLogs.Where(x => x.Action == "PermissionDenied" && x.EntityName == "CatalogRoute"));
        Assert.Equal("/beneficiaries/create", audit.EntityId);
        Assert.Contains("ip=127.0.0.1", audit.Details);
    }

    private static HashSet<string> GetBlazorPageRoutes()
    {
        var root = FindRepositoryRoot();
        var pagesPath = Path.Combine(root, "Express Service", "Components", "Pages");

        return Directory.EnumerateFiles(pagesPath, "*.razor", SearchOption.AllDirectories)
            .SelectMany(File.ReadLines)
            .Select(ExtractPageRoute)
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Select(route => route!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string? ExtractPageRoute(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("@page ", StringComparison.Ordinal))
            return null;

        var firstQuote = trimmed.IndexOf('"');
        if (firstQuote < 0)
            return null;

        var secondQuote = trimmed.IndexOf('"', firstQuote + 1);
        return secondQuote < 0 ? null : trimmed[(firstQuote + 1)..secondQuote];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ManagementSystem.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find ManagementSystem.slnx.");
    }

    private sealed class CatalogUserContext(string userId, IReadOnlyCollection<string> roles) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = roles;
        public string? RemoteIpAddress => "127.0.0.1";
    }
}
