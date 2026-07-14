using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ReportsStatistics;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.Json;

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
        new("report_board_governance", "تقرير مجالس الإدارة والدورات", SystemReportKind.Report, "BoardGovernance"),
        new("report_meeting_workflow", "تقرير سير عمل الاجتماعات والقرارات", SystemReportKind.Report, "BoardGovernance"),
        new("report_governance_tasks", "تقرير مهام الحوكمة", SystemReportKind.Report, "InstitutionalExcellence"),
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
        var governanceTasks = dbcontext.GovernanceTasks.AsNoTracking();
        var governanceTaskCount = await governanceTasks.CountAsync(cancellationToken);
        var completedGovernanceTasks = await governanceTasks.CountAsync(x => x.Status == GovernanceTaskStatus.Completed, cancellationToken);
        return Result.Success(new ReportsStatisticsDashboardResponse(
            await dbcontext.SystemReportDefinitions.CountAsync(cancellationToken),
            await dbcontext.SystemReportDefinitions.CountAsync(x => x.Kind == SystemReportKind.Report, cancellationToken),
            await dbcontext.SystemReportDefinitions.CountAsync(x => x.Kind == SystemReportKind.Statistic, cancellationToken),
            await dbcontext.SystemReportRuns.CountAsync(x => x.Status != SystemReportRunStatus.Archived, cancellationToken),
            await dbcontext.SystemReportRuns.Where(x => x.Status != SystemReportRunStatus.Archived).OrderByDescending(x => x.GeneratedAt).Select(x => (DateTime?)x.GeneratedAt).FirstOrDefaultAsync(cancellationToken),
            await dbcontext.Boards.CountAsync(x => x.Status == BoardStatus.Active, cancellationToken),
            await dbcontext.BoardMeetings.CountAsync(x => x.Status == MeetingStatus.InProgress || x.Status == MeetingStatus.WaitingChairmanApproval || x.Status == MeetingStatus.PendingApproval, cancellationToken),
            await dbcontext.VoteSessions.CountAsync(x => x.Status == VoteSessionStatus.Open, cancellationToken),
            await governanceTasks.CountAsync(x => x.Status == GovernanceTaskStatus.Pending || x.Status == GovernanceTaskStatus.InProgress, cancellationToken),
            await governanceTasks.CountAsync(x => x.Status == GovernanceTaskStatus.Overdue, cancellationToken),
            governanceTaskCount == 0 ? 0 : Math.Round(completedGovernanceTasks * 100m / governanceTaskCount, 1)));
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
            RowCount = await CountRowsAsync(definition.Key, request.FiltersJson, cancellationToken),
            Status = SystemReportRunStatus.Generated,
            GeneratedAt = DateTime.UtcNow.AddHours(3)
        };
        dbcontext.SystemReportRuns.Add(run);
        definition.LastGeneratedAt = run.GeneratedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRun(run));
    }

    public async Task<Result<SystemReportExportResponse>> ExportReportAsync(GenerateSystemReportRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultReportsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(request.ReportKey))
            return Result.Failure<SystemReportExportResponse>(ReportsStatisticsErrors.InvalidRequest);

        var key = request.ReportKey.Trim();
        var definition = await dbcontext.SystemReportDefinitions.FirstOrDefaultAsync(x => x.Key == key && x.IsActive, cancellationToken);
        if (definition is null)
            return Result.Failure<SystemReportExportResponse>(ReportsStatisticsErrors.DefinitionNotFound);
        var exportFormat = NormalizeExportFormat(request.Format);
        if (exportFormat is null)
            return Result.Failure<SystemReportExportResponse>(ReportsStatisticsErrors.InvalidRequest);

        var filters = ReportFilters.FromJson(request.FiltersJson);
        var rows = await BuildReportRowsAsync(definition.Key, definition.NameAr, filters, cancellationToken);
        var rowCount = await CountRowsAsync(definition.Key, request.FiltersJson, cancellationToken);
        var generatedAt = DateTime.UtcNow.AddHours(3);
        var run = new SystemReportRun
        {
            SystemReportDefinition = definition,
            SystemReportDefinitionId = definition.Id,
            ReportKey = definition.Key,
            ReportName = definition.NameAr,
            Format = exportFormat,
            FiltersJson = request.FiltersJson?.Trim(),
            RequestedBy = request.RequestedBy?.Trim(),
            RowCount = rowCount,
            Status = SystemReportRunStatus.Generated,
            GeneratedAt = generatedAt
        };
        dbcontext.SystemReportRuns.Add(run);
        definition.LastGeneratedAt = run.GeneratedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);

        var (extension, contentType, content) = exportFormat switch
        {
            "Csv" => ("csv", "text/csv; charset=utf-8", ToCsv(rows)),
            "Tsv" => ("tsv", "text/tab-separated-values; charset=utf-8", ToDelimited(rows, "\t")),
            "Json" => ("json", "application/json; charset=utf-8", JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true })),
            "Xlsx" => ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Empty),
            "Pdf" => ("pdf", "application/pdf", string.Empty),
            _ => throw new InvalidOperationException("Unsupported export format.")
        };

        var pdf = exportFormat == "Pdf" ? GeneratePdf(definition.NameAr, rows, generatedAt) : null;
        if (pdf is not null) { extension = "pdf"; contentType = "application/pdf"; content = string.Empty; }
        var xlsx = exportFormat == "Xlsx" ? GenerateXlsx(definition.NameAr, rows) : null;

        var binaryContent = pdf ?? xlsx;
        run.ArchiveFileName = $"{definition.Key}-{generatedAt:yyyyMMddHHmmss}.{extension}";
        run.ArchiveContentType = contentType;
        run.ArchivedContent = binaryContent ?? Encoding.UTF8.GetBytes(content);
        run.ArchivedAt = generatedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(new SystemReportExportResponse(
            MapRun(run),
            run.ArchiveFileName,
            contentType,
            content,
            binaryContent));
    }

    public async Task<Result<SystemReportExportResponse>> GetArchivedExportAsync(int runId, CancellationToken cancellationToken = default)
    {
        var run = await dbcontext.SystemReportRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == runId, cancellationToken);
        if (run is null || run.ArchivedContent is null || string.IsNullOrWhiteSpace(run.ArchiveFileName) || string.IsNullOrWhiteSpace(run.ArchiveContentType))
            return Result.Failure<SystemReportExportResponse>(ReportsStatisticsErrors.DefinitionNotFound);

        var textContent = run.ArchiveContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
            run.ArchiveContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            ? Encoding.UTF8.GetString(run.ArchivedContent)
            : string.Empty;
        return Result.Success(new SystemReportExportResponse(MapRun(run), run.ArchiveFileName, run.ArchiveContentType, textContent, run.ArchivedContent));
    }

    public async Task<Result<IEnumerable<SystemReportRunResponse>>> GetRunsAsync(string? reportKey = null, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SystemReportRuns.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(reportKey)) query = query.Where(x => x.ReportKey == reportKey.Trim());
        if (!includeArchived) query = query.Where(x => x.Status != SystemReportRunStatus.Archived);
        return Result.Success<IEnumerable<SystemReportRunResponse>>(await query.OrderByDescending(x => x.GeneratedAt).Select(x => MapRun(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<ArchiveSystemReportRunsResponse>> ArchiveRunsAsync(ArchiveSystemReportRunsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Before == default)
            return Result.Failure<ArchiveSystemReportRunsResponse>(ReportsStatisticsErrors.InvalidRequest);

        var query = dbcontext.SystemReportRuns.AsQueryable()
            .Where(x =>
                x.GeneratedAt < request.Before &&
                (x.Status == SystemReportRunStatus.Generated || x.Status == SystemReportRunStatus.Failed));
        if (!string.IsNullOrWhiteSpace(request.ReportKey))
        {
            var key = request.ReportKey.Trim();
            query = query.Where(x => x.ReportKey == key);
        }

        var runs = await query.OrderBy(x => x.GeneratedAt).ToListAsync(cancellationToken);
        foreach (var run in runs)
        {
            run.Status = SystemReportRunStatus.Archived;
            run.RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? run.RequestedBy : request.RequestedBy.Trim();
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new ArchiveSystemReportRunsResponse(runs.Count, runs.Select(MapRun).ToList()));
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

    private async Task<int> CountRowsAsync(string key, string? filtersJson, CancellationToken cancellationToken)
    {
        var filters = ReportFilters.FromJson(filtersJson);
        return key switch
        {
            "report_users" or "report_users_income" or "statistics_users" or "statistics_areas" => await CountBeneficiariesAsync(filters, cancellationToken),
            "report_relatives" or "statistics_relatives" => await dbcontext.BeneficiaryDependents.CountAsync(cancellationToken),
            "report_personnel" => await dbcontext.EmployeeProfiles.CountAsync(cancellationToken),
            "report_requests" or "statistics_requests" => await CountAidRequestsAsync(filters, cancellationToken),
            "report_tasks" or "statistics_tasks" => await dbcontext.ManagementTasks.CountAsync(cancellationToken),
            "report_kafeel" => await dbcontext.Sponsors.CountAsync(cancellationToken),
            "report_kafeel_matrix" or "statistics_orphans" => await dbcontext.SponsorshipRecords.CountAsync(cancellationToken),
            "report_supporters" => await dbcontext.FinancialSupporters.CountAsync(cancellationToken),
            "report_board" => await dbcontext.MemberProfiles.CountAsync(cancellationToken),
            "report_projects" or "report_programs" or "statistics_projects" => await CountProjectsAsync(filters, cancellationToken),
            "report_media_events" => await dbcontext.MediaEvents.CountAsync(cancellationToken),
            "report_media_partners" => await dbcontext.MediaPartners.CountAsync(cancellationToken),
            "report_media_visits" => await dbcontext.MediaVisits.CountAsync(cancellationToken),
            "report_marketing" => await dbcontext.FundraisingOpportunities.CountAsync(cancellationToken) + await dbcontext.DigitalMarketingCampaigns.CountAsync(cancellationToken),
            "report_mail" => await dbcontext.InternalMailMessages.CountAsync(cancellationToken),
            "report_website_users" => await dbcontext.WebsiteUserAccounts.CountAsync(cancellationToken),
            "report_orders" => await dbcontext.BeneficiaryPaymentOrders.CountAsync(cancellationToken),
            "report_followup" => await dbcontext.FollowUpCases.CountAsync(cancellationToken),
            "report_qual" => await dbcontext.ProgramQualificationCases.CountAsync(cancellationToken),
            "report_expenses" => await CountExpensesAsync(filters, cancellationToken),
            "report_income" => await CountReceiptsAsync(filters, cancellationToken),
            "report_analytics" => await dbcontext.WebsiteContactRequests.CountAsync(cancellationToken),
            "projects_finance_reports" => await dbcontext.ProgramProjectFinanceEntries.CountAsync(cancellationToken),
            "report_board_governance" => await dbcontext.Boards.CountAsync(cancellationToken),
            "report_meeting_workflow" => await dbcontext.BoardMeetings.CountAsync(cancellationToken),
            "report_governance_tasks" => await dbcontext.GovernanceTasks.CountAsync(cancellationToken),
            "statistics_finance" => await CountReceiptsAsync(filters, cancellationToken) + await CountExpensesAsync(filters, cancellationToken),
            _ => 0
        };
    }

    private async Task<int> CountBeneficiariesAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        return await BeneficiaryQuery(filters).CountAsync(cancellationToken);
    }

    private async Task<int> CountAidRequestsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        return await AidRequestQuery(filters).CountAsync(cancellationToken);
    }

    private async Task<int> CountProjectsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        return await ProjectQuery(filters).CountAsync(cancellationToken);
    }

    private async Task<int> CountReceiptsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        return await ReceiptQuery(filters).CountAsync(cancellationToken);
    }

    private async Task<int> CountExpensesAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        return await ExpenseQuery(filters).CountAsync(cancellationToken);
    }

    private IQueryable<BeneficiaryProfile> BeneficiaryQuery(ReportFilters filters)
    {
        var query = dbcontext.BeneficiaryProfiles.AsQueryable();
        if (filters.TryEnum<BeneficiaryStatus>("status", out var status))
            query = query.Where(x => x.Status == status);
        if (filters.TryString("city", out var city))
            query = query.Where(x => x.City != null && x.City.Contains(city));
        if (filters.TryString("category", out var category))
            query = query.Where(x => x.Category != null && x.Category.Contains(category));
        return query;
    }

    private IQueryable<BeneficiaryAidRequest> AidRequestQuery(ReportFilters filters)
    {
        var query = dbcontext.BeneficiaryAidRequests.AsQueryable();
        if (filters.TryEnum<AidRequestStatus>("status", out var status))
            query = query.Where(x => x.Status == status);
        if (filters.TryBool("isExternal", out var isExternal))
            query = query.Where(x => x.IsExternal == isExternal);
        return query;
    }

    private IQueryable<ProgramProject> ProjectQuery(ReportFilters filters)
    {
        var query = dbcontext.ProgramProjects.AsQueryable();
        if (filters.TryEnum<ProgramProjectStatus>("status", out var status))
            query = query.Where(x => x.Status == status);
        if (filters.TryString("projectType", out var projectType))
            query = query.Where(x => x.ProjectType.Contains(projectType));
        return query;
    }

    private IQueryable<ReceiptVoucher> ReceiptQuery(ReportFilters filters)
    {
        var query = dbcontext.ReceiptVouchers.AsQueryable();
        if (filters.TryEnum<AccountingRecordStatus>("status", out var status))
            query = query.Where(x => x.Status == status);
        if (filters.TryEnum<ReceiptVoucherKind>("kind", out var kind))
            query = query.Where(x => x.Kind == kind);
        return query;
    }

    private IQueryable<ExpenseVoucher> ExpenseQuery(ReportFilters filters)
    {
        var query = dbcontext.ExpenseVouchers.AsQueryable();
        if (filters.TryEnum<AccountingRecordStatus>("status", out var status))
            query = query.Where(x => x.Status == status);
        if (filters.TryEnum<ExpenseVoucherKind>("kind", out var kind))
            query = query.Where(x => x.Kind == kind);
        return query;
    }

    private async Task<List<Dictionary<string, string>>> BuildReportRowsAsync(string key, string reportName, ReportFilters filters, CancellationToken cancellationToken)
    {
        return key switch
        {
            "report_users" or "report_users_income" or "statistics_users" or "statistics_areas" => await BuildBeneficiaryRowsAsync(filters, cancellationToken),
            "report_relatives" or "statistics_relatives" => await BuildSimpleRowsAsync(dbcontext.BeneficiaryDependents.AsNoTracking(), filters, cancellationToken,
                "Id", "BeneficiaryProfileId", "FullName", "NationalId", "Relationship", "BirthDate", "Category", "Grade", "IsActive", "CreatedAt"),
            "report_personnel" => await BuildSimpleRowsAsync(dbcontext.EmployeeProfiles.AsNoTracking(), filters, cancellationToken,
                "Id", "EmployeeNumber", "FullName", "NationalId", "Email", "Mobile", "DepartmentId", "JobTitleId", "AccountType", "HireDate", "Status", "BasicSalary", "Allowances", "CreatedAt"),
            "report_requests" or "statistics_requests" => await BuildAidRequestRowsAsync(filters, cancellationToken),
            "report_tasks" or "statistics_tasks" => await BuildSimpleRowsAsync(dbcontext.ManagementTasks.AsNoTracking(), filters, cancellationToken,
                "Id", "Title", "AssigneeUserId", "DueAt", "Priority", "Status", "ProgressPercentage", "RelatedEntityType", "RelatedEntityId", "CompletedAt", "CreatedAt"),
            "report_kafeel" => await BuildSimpleRowsAsync(dbcontext.Sponsors.AsNoTracking(), filters, cancellationToken,
                "Id", "FullName", "Mobile", "Email", "Status", "Notes", "CreatedAt"),
            "report_kafeel_matrix" or "statistics_orphans" => await BuildSimpleRowsAsync(dbcontext.SponsorshipRecords.AsNoTracking(), filters, cancellationToken,
                "Id", "SponsorId", "BeneficiaryProfileId", "SponsorshipRequirementId", "StartsAt", "EndsAt", "Amount", "Status", "Notes", "CreatedAt"),
            "report_supporters" => await BuildSimpleRowsAsync(dbcontext.FinancialSupporters.AsNoTracking(), filters, cancellationToken,
                "Id", "Name", "SupporterType", "Category", "Mobile", "Email", "NationalIdOrRegistrationNo", "PreferredContactChannel", "Status", "CreatedAt"),
            "report_board" => await BuildSimpleRowsAsync(dbcontext.MemberProfiles.AsNoTracking(), filters, cancellationToken,
                "Id", "MemberNumber", "FullName", "NationalId", "Email", "Mobile", "City", "MembershipTypeId", "JoinedAt", "Status", "FeesPaid", "IsSupporter", "CumulativePercentage", "CreatedAt"),
            "report_projects" or "report_programs" or "statistics_projects" => await BuildProjectRowsAsync(filters, cancellationToken),
            "report_media_events" => await BuildSimpleRowsAsync(dbcontext.MediaEvents.AsNoTracking(), filters, cancellationToken,
                "Id", "Title", "EventDate", "Location", "Description", "Status", "CreatedAt"),
            "report_media_partners" => await BuildSimpleRowsAsync(dbcontext.MediaPartners.AsNoTracking(), filters, cancellationToken,
                "Id", "Name", "ContactPerson", "Mobile", "Email", "Status", "Notes", "CreatedAt"),
            "report_media_visits" => await BuildSimpleRowsAsync(dbcontext.MediaVisits.AsNoTracking(), filters, cancellationToken,
                "Id", "VisitorName", "Organization", "VisitDate", "Purpose", "Status", "CreatedAt"),
            "report_marketing" => await BuildMarketingRowsAsync(filters, cancellationToken),
            "report_mail" => await BuildSimpleRowsAsync(dbcontext.InternalMailMessages.AsNoTracking(), filters, cancellationToken,
                "Id", "SenderUserId", "Subject", "Status", "SentAt", "CreatedAt"),
            "report_website_users" => await BuildSimpleRowsAsync(dbcontext.WebsiteUserAccounts.AsNoTracking(), filters, cancellationToken,
                "Id", "FullName", "Username", "Email", "RoleName", "Status", "LastLoginAt", "CreatedAt"),
            "report_orders" => await BuildSimpleRowsAsync(dbcontext.BeneficiaryPaymentOrders.AsNoTracking(), filters, cancellationToken,
                "Id", "OrderNumber", "OrderType", "BeneficiaryAidRequestId", "EntitySupportRequestId", "BeneficiaryProfileId", "Amount", "ItemDescription", "Status", "DueDate", "ClosedAt", "CreatedAt"),
            "report_followup" => await BuildSimpleRowsAsync(dbcontext.FollowUpCases.AsNoTracking(), filters, cancellationToken,
                "Id", "CaseNumber", "SubjectType", "SubjectName", "ReferenceNumber", "RequestedBy", "RequestDate", "DueDate", "Priority", "Status", "RejectionNote", "CompletionSummary", "CompletedAt", "CreatedAt"),
            "report_qual" => await BuildSimpleRowsAsync(dbcontext.ProgramQualificationCases.AsNoTracking(), filters, cancellationToken,
                "Id", "ProgramProjectId", "BeneficiaryName", "NeedSummary", "ManagementOpinion", "Status", "ApprovedAmount", "InstallmentCount", "CreatedAt"),
            "report_expenses" => await BuildExpenseRowsAsync(filters, cancellationToken),
            "report_income" => await BuildReceiptRowsAsync(filters, cancellationToken),
            "report_analytics" => await BuildSimpleRowsAsync(dbcontext.WebsiteContactRequests.AsNoTracking(), filters, cancellationToken,
                "Id", "FullName", "Email", "Mobile", "Subject", "Status", "CreatedAt"),
            "projects_finance_reports" => await BuildSimpleRowsAsync(dbcontext.ProgramProjectFinanceEntries.AsNoTracking(), filters, cancellationToken,
                "Id", "ProgramProjectId", "EntryType", "EntryDate", "Amount", "SourceOrPayee", "ReferenceNumber", "Notes", "CreatedAt"),
            "statistics_finance" => await BuildFinanceRowsAsync(filters, cancellationToken),
            "report_board_governance" => await BuildBoardGovernanceRowsAsync(cancellationToken),
            "report_meeting_workflow" => await BuildMeetingWorkflowRowsAsync(cancellationToken),
            "report_governance_tasks" => await BuildGovernanceTaskRowsAsync(cancellationToken),
            _ => [new Dictionary<string, string> { ["ReportKey"] = key, ["ReportName"] = reportName, ["Message"] = "No report dataset is registered for this definition." }]
        };
    }

    private async Task<List<Dictionary<string, string>>> BuildMarketingRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var opportunities = await BuildSimpleRowsAsync(dbcontext.FundraisingOpportunities.AsNoTracking(), filters, cancellationToken,
            "Id", "Title", "OpportunityType", "ReferenceNumber", "TargetAmount", "CurrentAmount", "StartDate", "EndDate", "Status", "ExternalUrl", "CreatedAt");
        foreach (var row in opportunities) row["Source"] = "FundraisingOpportunity";

        var campaigns = await BuildSimpleRowsAsync(dbcontext.DigitalMarketingCampaigns.AsNoTracking(), filters, cancellationToken,
            "Id", "Title", "Channel", "Budget", "TargetAudience", "LandingPageUrl", "StartDate", "EndDate", "Status", "LeadsCount", "DonationsCount", "DonationsAmount", "CreatedAt");
        foreach (var row in campaigns) row["Source"] = "DigitalMarketingCampaign";

        opportunities.AddRange(campaigns);
        return opportunities;
    }

    private static async Task<List<Dictionary<string, string>>> BuildSimpleRowsAsync<T>(
        IQueryable<T> query,
        ReportFilters filters,
        CancellationToken cancellationToken,
        params string[] propertyNames)
        where T : class
    {
        var items = await query.Take(10_000).ToListAsync(cancellationToken);
        var properties = propertyNames
            .Select(name => typeof(T).GetProperty(name))
            .Where(property => property is not null)
            .ToList();
        var rows = items.Select(item => properties.ToDictionary(
            property => property!.Name,
            property => FormatReportValue(property!.GetValue(item)),
            StringComparer.Ordinal)).ToList();
        return ApplyCommonRowFilters(rows, filters);
    }

    private static List<Dictionary<string, string>> ApplyCommonRowFilters(List<Dictionary<string, string>> rows, ReportFilters filters)
    {
        IEnumerable<Dictionary<string, string>> filtered = rows;
        if (filters.TryString("status", out var status))
            filtered = filtered.Where(row => row.TryGetValue("Status", out var value) && value.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (filters.TryString("search", out var search))
            filtered = filtered.Where(row => row.Values.Any(value => value.Contains(search, StringComparison.OrdinalIgnoreCase)));
        return filtered.ToList();
    }

    private static string FormatReportValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        bool flag => flag ? "Yes" : "No",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
        _ => value.ToString() ?? string.Empty
    };

    private async Task<List<Dictionary<string, string>>> BuildBeneficiaryRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await BeneficiaryQuery(filters).AsNoTracking().OrderBy(x => x.BeneficiaryNumber).ToListAsync(cancellationToken);
        return rows.Select(x => new Dictionary<string, string>
        {
            ["BeneficiaryNumber"] = x.BeneficiaryNumber,
            ["FullName"] = x.FullName,
            ["Mobile"] = x.Mobile ?? string.Empty,
            ["City"] = x.City ?? string.Empty,
            ["Category"] = x.Category ?? string.Empty,
            ["Status"] = x.Status.ToString(),
            ["MonthlyIncome"] = x.MonthlyIncome.ToString("0.##"),
            ["FamilyMembersCount"] = x.FamilyMembersCount.ToString()
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildAidRequestRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await AidRequestQuery(filters).AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return rows.Select(x => new Dictionary<string, string>
        {
            ["RequestNumber"] = x.RequestNumber,
            ["AidType"] = x.AidType,
            ["Amount"] = x.Amount.ToString("0.##"),
            ["Description"] = x.Description,
            ["Status"] = x.Status.ToString(),
            ["IsExternal"] = x.IsExternal.ToString(),
            ["DecisionNotes"] = x.DecisionNotes ?? string.Empty
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildProjectRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await ProjectQuery(filters).AsNoTracking().OrderBy(x => x.ProjectCode).ToListAsync(cancellationToken);
        return rows.Select(x => new Dictionary<string, string>
        {
            ["ProjectCode"] = x.ProjectCode,
            ["Name"] = x.Name,
            ["ProjectType"] = x.ProjectType,
            ["ManagerName"] = x.ManagerName ?? string.Empty,
            ["Status"] = x.Status.ToString(),
            ["Budget"] = x.Budget.ToString("0.##"),
            ["TargetBeneficiaries"] = x.TargetBeneficiaries.ToString("0.##"),
            ["StartsAt"] = FormatDate(x.StartsAt),
            ["EndsAt"] = FormatDate(x.EndsAt)
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildReceiptRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await ReceiptQuery(filters).AsNoTracking().OrderByDescending(x => x.ReceiptDate).ToListAsync(cancellationToken);
        return rows.Select(x => new Dictionary<string, string>
        {
            ["RecordType"] = "Income",
            ["Number"] = x.ReceiptNumber,
            ["Kind"] = x.Kind.ToString(),
            ["Date"] = FormatDate(x.ReceiptDate),
            ["Amount"] = x.Amount.ToString("0.##"),
            ["PartyName"] = x.PayerName,
            ["Status"] = x.Status.ToString(),
            ["ReferenceNumber"] = x.ReferenceNumber ?? string.Empty,
            ["Notes"] = x.Notes ?? string.Empty
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildExpenseRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await ExpenseQuery(filters).AsNoTracking().OrderByDescending(x => x.ExpenseDate).ToListAsync(cancellationToken);
        return rows.Select(x => new Dictionary<string, string>
        {
            ["RecordType"] = "Expense",
            ["Number"] = x.ExpenseNumber,
            ["Kind"] = x.Kind.ToString(),
            ["Date"] = FormatDate(x.ExpenseDate),
            ["Amount"] = x.Amount.ToString("0.##"),
            ["PartyName"] = x.PayeeName,
            ["Status"] = x.Status.ToString(),
            ["Notes"] = x.Notes ?? string.Empty
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildFinanceRowsAsync(ReportFilters filters, CancellationToken cancellationToken)
    {
        var rows = await BuildReceiptRowsAsync(filters, cancellationToken);
        rows.AddRange(await BuildExpenseRowsAsync(filters, cancellationToken));
        return rows;
    }

    private async Task<List<Dictionary<string, string>>> BuildBoardGovernanceRowsAsync(CancellationToken cancellationToken)
    {
        var boards = await dbcontext.Boards.AsNoTracking().Include(x => x.Cycles).Include(x => x.Memberships).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return boards.Select(x =>
        {
            var cycle = x.Cycles.OrderByDescending(c => c.CycleNumber).FirstOrDefault();
            return new Dictionary<string, string>
            {
                ["Board"] = x.Name, ["Code"] = x.Code, ["Status"] = x.Status.ToString(), ["Members"] = x.Memberships.Count(m => m.IsActive).ToString(),
                ["Cycle"] = cycle?.CycleNumber.ToString() ?? string.Empty, ["CycleStartsAt"] = FormatDate(cycle?.StartsAt), ["CycleEndsAt"] = FormatDate(cycle?.EndsAt)
            };
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildMeetingWorkflowRowsAsync(CancellationToken cancellationToken)
    {
        var meetings = await dbcontext.BoardMeetings.AsNoTracking().Include(x => x.BoardCycle).ThenInclude(x => x!.Board).Include(x => x.AgendaItems).OrderByDescending(x => x.ScheduledAt).ToListAsync(cancellationToken);
        return meetings.Select(x => new Dictionary<string, string>
        {
            ["Board"] = x.BoardCycle?.Board?.Name ?? string.Empty, ["Meeting"] = x.Title, ["ScheduledAt"] = FormatDate(x.ScheduledAt), ["Type"] = x.Type.ToString(), ["Status"] = x.Status.ToString(),
            ["VotingEnabled"] = x.HasVoting.ToString(), ["AgendaItems"] = x.AgendaItems.Count.ToString(), ["ApprovedDecisions"] = x.AgendaItems.Count(a => a.Status == AgendaItemStatus.DecisionApproved).ToString()
        }).ToList();
    }

    private async Task<List<Dictionary<string, string>>> BuildGovernanceTaskRowsAsync(CancellationToken cancellationToken)
    {
        var tasks = await dbcontext.GovernanceTasks.AsNoTracking().Include(x => x.GovernanceCycle).OrderBy(x => x.DueDate).ToListAsync(cancellationToken);
        return tasks.Select(x => new Dictionary<string, string>
        {
            ["Cycle"] = x.GovernanceCycle?.Title ?? string.Empty, ["Task"] = x.Title, ["Owner"] = x.OwnerName ?? string.Empty, ["DueDate"] = FormatDate(x.DueDate),
            ["Status"] = x.Status.ToString(), ["ProgressPercent"] = x.ProgressPercent.ToString(), ["Notes"] = x.Notes ?? string.Empty
        }).ToList();
    }

    private static string ToCsv(IReadOnlyCollection<Dictionary<string, string>> rows)
    {
        if (rows.Count == 0)
            return "Message\r\nNo rows matched the report filters.\r\n";

        var headers = rows.SelectMany(x => x.Keys).Distinct(StringComparer.Ordinal).ToList();
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(",", headers.Select(header => EscapeCsv(row.TryGetValue(header, out var value) ? value : string.Empty))));
        }

        return builder.ToString();
    }

    private static string ToDelimited(IReadOnlyCollection<Dictionary<string, string>> rows, string delimiter)
    {
        if (rows.Count == 0)
            return "Message\r\nNo rows matched the report filters.\r\n";

        var headers = rows.SelectMany(x => x.Keys).Distinct(StringComparer.Ordinal).ToList();
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(delimiter, headers));
        foreach (var row in rows)
            builder.AppendLine(string.Join(delimiter, headers.Select(header => (row.TryGetValue(header, out var value) ? value : string.Empty).Replace("\t", " ").Replace("\r", " ").Replace("\n", " "))));
        return builder.ToString();
    }

    private static string? NormalizeExportFormat(string? format) => format?.Trim().ToLowerInvariant() switch
    {
        "csv" or null or "" => "Csv",
        "tsv" => "Tsv",
        "json" => "Json",
        "pdf" => "Pdf",
        "xlsx" => "Xlsx",
        _ => null
    };

    private static string EscapeCsv(string? value)
    {
        value ??= string.Empty;
        return value.Contains('"') || value.Contains(',') || value.Contains('\r') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    private static byte[] GeneratePdf(string reportName, IReadOnlyCollection<Dictionary<string, string>> rows, DateTime generatedAt) =>
        Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontSize(10));
            page.Header().AlignRight().Text($"{reportName}\nتقرير صادر من نظام الإدارة الإلكتروني — {generatedAt:yyyy-MM-dd HH:mm}").SemiBold().FontSize(14).FontColor(Colors.Blue.Darken2);
            page.Content().Column(column =>
            {
                if (rows.Count == 0) { column.Item().AlignRight().Text("لا توجد بيانات مطابقة للفلاتر المحددة."); return; }
                var headers = rows.SelectMany(x => x.Keys).Distinct(StringComparer.Ordinal).ToList();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { foreach (var _ in headers) columns.RelativeColumn(); });
                    table.Header(header => { foreach (var item in headers) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item).SemiBold(); });
                    foreach (var row in rows)
                        foreach (var header in headers)
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(row.TryGetValue(header, out var value) ? value : string.Empty);
                });
            });
            page.Footer().AlignCenter().Text("وثيقة قابلة للطباعة — لا تعد بديلاً عن الاعتماد النظامي عند الحاجة.");
        })).GeneratePdf();

    private static byte[] GenerateXlsx(string reportName, IReadOnlyCollection<Dictionary<string, string>> rows)
    {
        var headers = rows.SelectMany(x => x.Keys).Distinct(StringComparer.Ordinal).ToList();
        if (headers.Count == 0) headers.Add("Message");
        var sheetRows = rows.Count == 0
            ? [new Dictionary<string, string> { ["Message"] = "لا توجد بيانات مطابقة للفلاتر المحددة." }]
            : rows.ToList();

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteZipEntry(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
            WriteZipEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            WriteZipEntry(archive, "xl/workbook.xml", $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="{EscapeXml(reportName)[..Math.Min(EscapeXml(reportName).Length, 31)]}" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            WriteZipEntry(archive, "xl/worksheets/sheet1.xml", GenerateWorksheetXml(headers, sheetRows));
        }

        return stream.ToArray();
    }

    private static string GenerateWorksheetXml(IReadOnlyList<string> headers, IReadOnlyList<Dictionary<string, string>> rows)
    {
        var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetViews><sheetView workbookViewId=\"0\" rightToLeft=\"1\"/></sheetViews><sheetData>");
        AppendWorksheetRow(builder, 1, headers.Select(x => x).ToList());
        for (var index = 0; index < rows.Count; index++)
            AppendWorksheetRow(builder, index + 2, headers.Select(header => rows[index].TryGetValue(header, out var value) ? value : string.Empty).ToList());
        return builder.Append("</sheetData></worksheet>").ToString();
    }

    private static void AppendWorksheetRow(StringBuilder builder, int rowNumber, IReadOnlyList<string> cells)
    {
        builder.Append($"<row r=\"{rowNumber}\">");
        for (var index = 0; index < cells.Count; index++)
            builder.Append($"<c r=\"{ExcelColumn(index)}{rowNumber}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{EscapeXml(cells[index])}</t></is></c>");
        builder.Append("</row>");
    }

    private static string ExcelColumn(int index)
    {
        var value = index + 1;
        var column = string.Empty;
        while (value > 0) { value--; column = (char)('A' + value % 26) + column; value /= 26; }
        return column;
    }

    private static string EscapeXml(string value) => SecurityElement.Escape(value) ?? string.Empty;
    private static void WriteZipEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content.Trim());
    }

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");
    private static string FormatDate(DateTime? value) => value?.ToString("yyyy-MM-dd") ?? string.Empty;

    private static SystemReportDefinitionResponse MapDefinition(SystemReportDefinition x) =>
        new(x.Id, x.Key, x.NameAr, x.Kind.ToString(), x.SourceDomain, x.IsActive, x.LastGeneratedAt);

    private static SystemReportRunResponse MapRun(SystemReportRun x) =>
        new(x.Id, x.SystemReportDefinitionId, x.ReportKey, x.ReportName, x.Format, x.FiltersJson, x.RowCount, x.Status.ToString(), x.RequestedBy, x.ArchivedContent is { Length: > 0 }, x.GeneratedAt);

    private sealed record ReportSeed(string Key, string NameAr, SystemReportKind Kind, string SourceDomain);

    private sealed class ReportFilters
    {
        private readonly Dictionary<string, string> values;

        private ReportFilters(Dictionary<string, string> values) => this.values = values;

        public static ReportFilters FromJson(string? filtersJson)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(filtersJson))
                return new ReportFilters(values);

            try
            {
                using var document = JsonDocument.Parse(filtersJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return new ReportFilters(values);

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    values[property.Name] = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => property.Value.GetRawText(),
                        _ => string.Empty
                    };
                }
            }
            catch (JsonException)
            {
                return new ReportFilters(values);
            }

            return new ReportFilters(values);
        }

        public bool TryString(string key, out string value)
        {
            if (values.TryGetValue(key, out var stored) && !string.IsNullOrWhiteSpace(stored))
            {
                value = stored.Trim();
                return true;
            }

            value = string.Empty;
            return false;
        }

        public bool TryBool(string key, out bool value)
        {
            value = false;
            return values.TryGetValue(key, out var stored) && bool.TryParse(stored, out value);
        }

        public bool TryEnum<TEnum>(string key, out TEnum value)
            where TEnum : struct, Enum
        {
            value = default;
            return values.TryGetValue(key, out var stored) && Enum.TryParse(stored, true, out value);
        }
    }
}
