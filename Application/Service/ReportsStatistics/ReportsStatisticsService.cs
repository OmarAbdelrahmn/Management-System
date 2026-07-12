using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ReportsStatistics;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
            await dbcontext.SystemReportRuns.CountAsync(x => x.Status != SystemReportRunStatus.Archived, cancellationToken),
            await dbcontext.SystemReportRuns.Where(x => x.Status != SystemReportRunStatus.Archived).OrderByDescending(x => x.GeneratedAt).Select(x => (DateTime?)x.GeneratedAt).FirstOrDefaultAsync(cancellationToken)));
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
            Format = string.IsNullOrWhiteSpace(request.Format) ? "Csv" : request.Format.Trim(),
            FiltersJson = request.FiltersJson?.Trim(),
            RequestedBy = request.RequestedBy?.Trim(),
            RowCount = rowCount,
            Status = SystemReportRunStatus.Generated,
            GeneratedAt = generatedAt
        };
        dbcontext.SystemReportRuns.Add(run);
        definition.LastGeneratedAt = run.GeneratedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(new SystemReportExportResponse(
            MapRun(run),
            $"{definition.Key}-{generatedAt:yyyyMMddHHmmss}.csv",
            "text/csv; charset=utf-8",
            ToCsv(rows)));
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
            "report_requests" or "statistics_requests" => await BuildAidRequestRowsAsync(filters, cancellationToken),
            "report_projects" or "report_programs" or "statistics_projects" => await BuildProjectRowsAsync(filters, cancellationToken),
            "report_expenses" => await BuildExpenseRowsAsync(filters, cancellationToken),
            "report_income" => await BuildReceiptRowsAsync(filters, cancellationToken),
            "statistics_finance" => await BuildFinanceRowsAsync(filters, cancellationToken),
            _ => [new Dictionary<string, string> { ["ReportKey"] = key, ["ReportName"] = reportName, ["Message"] = "Detailed row export is planned for this report." }]
        };
    }

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

    private static string EscapeCsv(string? value)
    {
        value ??= string.Empty;
        return value.Contains('"') || value.Contains(',') || value.Contains('\r') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");
    private static string FormatDate(DateTime? value) => value?.ToString("yyyy-MM-dd") ?? string.Empty;

    private static SystemReportDefinitionResponse MapDefinition(SystemReportDefinition x) =>
        new(x.Id, x.Key, x.NameAr, x.Kind.ToString(), x.SourceDomain, x.IsActive, x.LastGeneratedAt);

    private static SystemReportRunResponse MapRun(SystemReportRun x) =>
        new(x.Id, x.SystemReportDefinitionId, x.ReportKey, x.ReportName, x.Format, x.FiltersJson, x.RowCount, x.Status.ToString(), x.RequestedBy, x.GeneratedAt);

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
