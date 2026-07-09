using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ReportsStatistics;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.ReportsStatistics;

public class ReportsStatisticsService(ApplicationDbcontext dbcontext) : IReportsStatisticsService
{
    private static readonly ReportSeed[] DefaultReports =
    [
        new("report_users", "تقارير المستفيدين", SystemReportKind.Report, "Beneficiaries"),
        new("report_users_income", "تقارير مصادر دخل المستفيدين", SystemReportKind.Report, "Beneficiaries"),
        new("report_relatives", "تقارير التابعين", SystemReportKind.Report, "Beneficiaries"),
        new("report_personnel", "تقارير الموظفين", SystemReportKind.Report, "HumanResources"),
        new("report_requests", "تقارير طلبات الإعانة", SystemReportKind.Report, "BeneficiaryServices"),
        new("report_tasks", "تقارير المهام", SystemReportKind.Report, "Tasks"),
        new("report_kafeel", "تقارير الكافلين", SystemReportKind.Report, "Sponsorships"),
        new("report_kafeel_matrix", "تقارير الكفالات", SystemReportKind.Report, "Sponsorships"),
        new("report_supporters", "تقارير الداعمين", SystemReportKind.Report, "FinancialDevelopment"),
        new("report_board", "تقارير الجمعية العمومية", SystemReportKind.Report, "Members"),
        new("report_projects", "تقارير المشاريع", SystemReportKind.Report, "ProgramsProjects"),
        new("report_media_events", "تقارير الفعاليات", SystemReportKind.Report, "PublicRelationsMedia"),
        new("report_media_partners", "تقارير الشراكات", SystemReportKind.Report, "PublicRelationsMedia"),
        new("report_media_visits", "تقارير الزيارات", SystemReportKind.Report, "PublicRelationsMedia"),
        new("report_marketing", "تقارير التسويق", SystemReportKind.Report, "FinancialDevelopment"),
        new("report_mail", "تقارير الصادر و الوارد", SystemReportKind.Report, "Messaging"),
        new("report_website_users", "تقارير حسابات الموقع", SystemReportKind.Report, "PublicRelationsMedia"),
        new("report_orders", "تقارير أوامر الصرف", SystemReportKind.Report, "BeneficiaryServices"),
        new("report_programs", "تقارير البرامج", SystemReportKind.Report, "ProgramsProjects"),
        new("report_followup", "تقارير متابعة الحالات", SystemReportKind.Report, "EvaluationFollowUp"),
        new("report_qual", "تقارير المشاريع التأهيلية", SystemReportKind.Report, "ProgramsProjects"),
        new("report_expenses", "تقارير المصروفات", SystemReportKind.Report, "Accounting"),
        new("report_income", "تقارير المقبوضات", SystemReportKind.Report, "Accounting"),
        new("report_analytics", "تقارير إحصائيات جووجل", SystemReportKind.Report, "Website"),
        new("projects_finance_reports", "التقارير المالية للمشاريع", SystemReportKind.Report, "ProgramsProjects"),
        new("statistics_users", "إحصائيات المستفيدين", SystemReportKind.Statistic, "Beneficiaries"),
        new("statistics_relatives", "إحصائيات التابعين", SystemReportKind.Statistic, "Beneficiaries"),
        new("statistics_orphans", "إحصائيات الأيتام و الكفالات", SystemReportKind.Statistic, "Sponsorships"),
        new("statistics_requests", "إحصائيات طلبات الإعانة", SystemReportKind.Statistic, "BeneficiaryServices"),
        new("statistics_projects", "إحصائيات المشاريع", SystemReportKind.Statistic, "ProgramsProjects"),
        new("statistics_tasks", "إحصائيات المهام", SystemReportKind.Statistic, "Tasks"),
        new("statistics_finance", "الإحصائيات المالية", SystemReportKind.Statistic, "Accounting"),
        new("statistics_areas", "إحصائيات القري - الأحياء", SystemReportKind.Statistic, "Beneficiaries")
    ];

    public async Task<Result<ReportsStatisticsDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDefaultReportsAsync(cancellationToken);
        return Result.Success(new ReportsStatisticsDashboardResponse(
            await dbcontext.SystemReportDefinitions.CountAsync(cancellationToken),
            await dbcontext.SystemReportDefinitions.CountAsync(x => x.Kind == SystemReportKind.Report, cancellationToken),
            await dbcontext.SystemReportDefinitions.CountAsync(x => x.Kind == SystemReportKind.Statistic, cancellationToken),
            await dbcontext.SystemReportRuns.CountAsync(cancellationToken),
            await dbcontext.SystemReportRuns.OrderByDescending(x => x.GeneratedAt).Select(x => (DateTime?)x.GeneratedAt).FirstOrDefaultAsync(cancellationToken)));
    }

    public async Task<Result<IEnumerable<SystemReportDefinitionResponse>>> GetDefinitionsAsync(SystemReportKind? kind = null, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultReportsAsync(cancellationToken);
        var query = dbcontext.SystemReportDefinitions.AsNoTracking().AsQueryable();
        if (kind.HasValue) query = query.Where(x => x.Kind == kind.Value);
        return Result.Success<IEnumerable<SystemReportDefinitionResponse>>(await query.OrderBy(x => x.Kind).ThenBy(x => x.NameAr).Select(x => MapDefinition(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<SystemReportDefinitionResponse>> SaveDefinitionAsync(int? id, SaveSystemReportDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.SourceDomain))
            return Result.Failure<SystemReportDefinitionResponse>(ReportsStatisticsErrors.InvalidRequest);

        var key = request.Key.Trim();
        if (await dbcontext.SystemReportDefinitions.AnyAsync(x => x.Key == key && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<SystemReportDefinitionResponse>(ReportsStatisticsErrors.DuplicateDefinition);

        var entity = id.HasValue
            ? await dbcontext.SystemReportDefinitions.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new SystemReportDefinition();
        if (entity is null)
            return Result.Failure<SystemReportDefinitionResponse>(ReportsStatisticsErrors.DefinitionNotFound);
        if (!id.HasValue) dbcontext.SystemReportDefinitions.Add(entity);

        entity.Key = key;
        entity.NameAr = request.NameAr.Trim();
        entity.Kind = request.Kind;
        entity.SourceDomain = request.SourceDomain.Trim();
        entity.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDefinition(entity));
    }

    public async Task<Result<SystemReportRunResponse>> GenerateReportAsync(GenerateSystemReportRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultReportsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(request.ReportKey))
            return Result.Failure<SystemReportRunResponse>(ReportsStatisticsErrors.InvalidRequest);

        var key = request.ReportKey.Trim();
        var definition = await dbcontext.SystemReportDefinitions.FirstOrDefaultAsync(x => x.Key == key && x.IsActive, cancellationToken);
        if (definition is null)
            return Result.Failure<SystemReportRunResponse>(ReportsStatisticsErrors.DefinitionNotFound);

        var run = new SystemReportRun
        {
            SystemReportDefinition = definition,
            SystemReportDefinitionId = definition.Id,
            ReportKey = definition.Key,
            ReportName = definition.NameAr,
            Format = string.IsNullOrWhiteSpace(request.Format) ? "Table" : request.Format.Trim(),
            FiltersJson = request.FiltersJson?.Trim(),
            RequestedBy = request.RequestedBy?.Trim(),
            RowCount = await CountRowsAsync(definition.Key, cancellationToken),
            Status = SystemReportRunStatus.Generated,
            GeneratedAt = DateTime.UtcNow.AddHours(3)
        };
        dbcontext.SystemReportRuns.Add(run);
        definition.LastGeneratedAt = run.GeneratedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRun(run));
    }

    public async Task<Result<IEnumerable<SystemReportRunResponse>>> GetRunsAsync(string? reportKey = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SystemReportRuns.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(reportKey)) query = query.Where(x => x.ReportKey == reportKey.Trim());
        return Result.Success<IEnumerable<SystemReportRunResponse>>(await query.OrderByDescending(x => x.GeneratedAt).Select(x => MapRun(x)).ToListAsync(cancellationToken));
    }

    private async Task EnsureDefaultReportsAsync(CancellationToken cancellationToken)
    {
        var existingKeys = await dbcontext.SystemReportDefinitions.Select(x => x.Key).ToListAsync(cancellationToken);
        var existing = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in DefaultReports)
        {
            if (existing.Contains(seed.Key))
                continue;

            dbcontext.SystemReportDefinitions.Add(new SystemReportDefinition
            {
                Key = seed.Key,
                NameAr = seed.NameAr,
                Kind = seed.Kind,
                SourceDomain = seed.SourceDomain,
                IsActive = true
            });
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> CountRowsAsync(string key, CancellationToken cancellationToken) => key switch
    {
        "report_users" or "report_users_income" or "statistics_users" or "statistics_areas" => await dbcontext.BeneficiaryProfiles.CountAsync(cancellationToken),
        "report_relatives" or "statistics_relatives" => await dbcontext.BeneficiaryDependents.CountAsync(cancellationToken),
        "report_personnel" => await dbcontext.EmployeeProfiles.CountAsync(cancellationToken),
        "report_requests" or "statistics_requests" => await dbcontext.BeneficiaryAidRequests.CountAsync(cancellationToken),
        "report_tasks" or "statistics_tasks" => await dbcontext.ManagementTasks.CountAsync(cancellationToken),
        "report_kafeel" => await dbcontext.Sponsors.CountAsync(cancellationToken),
        "report_kafeel_matrix" or "statistics_orphans" => await dbcontext.SponsorshipRecords.CountAsync(cancellationToken),
        "report_supporters" => await dbcontext.FinancialSupporters.CountAsync(cancellationToken),
        "report_board" => await dbcontext.MemberProfiles.CountAsync(cancellationToken),
        "report_projects" or "report_programs" or "statistics_projects" => await dbcontext.ProgramProjects.CountAsync(cancellationToken),
        "report_media_events" => await dbcontext.MediaEvents.CountAsync(cancellationToken),
        "report_media_partners" => await dbcontext.MediaPartners.CountAsync(cancellationToken),
        "report_media_visits" => await dbcontext.MediaVisits.CountAsync(cancellationToken),
        "report_marketing" => await dbcontext.FundraisingOpportunities.CountAsync(cancellationToken) + await dbcontext.DigitalMarketingCampaigns.CountAsync(cancellationToken),
        "report_mail" => await dbcontext.InternalMailMessages.CountAsync(cancellationToken),
        "report_website_users" => await dbcontext.WebsiteUserAccounts.CountAsync(cancellationToken),
        "report_orders" => await dbcontext.BeneficiaryPaymentOrders.CountAsync(cancellationToken),
        "report_followup" => await dbcontext.FollowUpCases.CountAsync(cancellationToken),
        "report_qual" => await dbcontext.ProgramQualificationCases.CountAsync(cancellationToken),
        "report_expenses" => await dbcontext.ExpenseVouchers.CountAsync(cancellationToken),
        "report_income" => await dbcontext.ReceiptVouchers.CountAsync(cancellationToken),
        "report_analytics" => await dbcontext.WebsiteContactRequests.CountAsync(cancellationToken),
        "projects_finance_reports" => await dbcontext.ProgramProjectFinanceEntries.CountAsync(cancellationToken),
        "statistics_finance" => await dbcontext.ReceiptVouchers.CountAsync(cancellationToken) + await dbcontext.ExpenseVouchers.CountAsync(cancellationToken),
        _ => 0
    };

    private static SystemReportDefinitionResponse MapDefinition(SystemReportDefinition x) =>
        new(x.Id, x.Key, x.NameAr, x.Kind.ToString(), x.SourceDomain, x.IsActive, x.LastGeneratedAt);

    private static SystemReportRunResponse MapRun(SystemReportRun x) =>
        new(x.Id, x.SystemReportDefinitionId, x.ReportKey, x.ReportName, x.Format, x.FiltersJson, x.RowCount, x.Status.ToString(), x.RequestedBy, x.GeneratedAt);

    private sealed record ReportSeed(string Key, string NameAr, SystemReportKind Kind, string SourceDomain);
}
