using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ProgramsProjects;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.ProgramsProjects;

public class ProgramsProjectsService(ApplicationDbcontext dbcontext) : IProgramsProjectsService
{
    public async Task<Result<ProgramsProjectsDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var projectsCount = await dbcontext.ProgramProjects.CountAsync(cancellationToken);
        var activeProjectsCount = await dbcontext.ProgramProjects.CountAsync(x => x.Status == ProgramProjectStatus.Active, cancellationToken);
        var completedProjectsCount = await dbcontext.ProgramProjects.CountAsync(x => x.Status == ProgramProjectStatus.Completed, cancellationToken);
        var suppliersCount = await dbcontext.ProgramSuppliers.CountAsync(x => x.Status == ProgramSupplierStatus.Active, cancellationToken);
        var pendingIdeasCount = await dbcontext.ProgramIdeas.CountAsync(x => x.Status == ProgramIdeaStatus.Pending, cancellationToken);
        var pendingApprovalsCount = await dbcontext.ProgramApprovals.CountAsync(x => x.Status == ProgramApprovalStatus.Pending, cancellationToken);
        var totalBudget = await dbcontext.ProgramProjects.SumAsync(x => x.Budget, cancellationToken);
        var totalIncome = await dbcontext.ProgramProjectFinanceEntries
            .Where(x => x.EntryType == ProgramProjectFinanceEntryType.Income)
            .SumAsync(x => x.Amount, cancellationToken);
        var totalExpenses = await dbcontext.ProgramProjectFinanceEntries
            .Where(x => x.EntryType == ProgramProjectFinanceEntryType.Expense || x.EntryType == ProgramProjectFinanceEntryType.Custody)
            .SumAsync(x => x.Amount, cancellationToken);

        return Result.Success(new ProgramsProjectsDashboardResponse(
            projectsCount,
            activeProjectsCount,
            completedProjectsCount,
            suppliersCount,
            pendingIdeasCount,
            pendingApprovalsCount,
            totalBudget,
            totalIncome,
            totalExpenses));
    }

    public async Task<Result<IEnumerable<ProgramProjectResponse>>> SearchProjectsAsync(ProgramProjectSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = ProjectQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.ProjectCode.Contains(search) ||
                (x.ManagerName != null && x.ManagerName.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.ProjectType))
        {
            var projectType = request.ProjectType.Trim();
            query = query.Where(x => x.ProjectType.Contains(projectType));
        }

        var projects = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Name)
            .Select(x => MapProject(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProgramProjectResponse>>(projects);
    }

    public async Task<Result<ProgramProjectResponse>> GetProjectAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await ProjectQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return project is null
            ? Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound)
            : Result.Success(MapProject(project));
    }

    public async Task<Result<ProgramProjectResponse>> SaveProjectAsync(int? id, SaveProgramProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ProjectType) || request.Budget < 0 || request.TargetBeneficiaries < 0)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        if (request.StartsAt.HasValue && request.EndsAt.HasValue && request.EndsAt.Value.Date < request.StartsAt.Value.Date)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        var projectCode = string.IsNullOrWhiteSpace(request.ProjectCode)
            ? await GenerateProjectCodeAsync(cancellationToken)
            : request.ProjectCode.Trim();

        var duplicateCode = await dbcontext.ProgramProjects.AnyAsync(x => x.ProjectCode == projectCode && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicateCode)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.DuplicateProjectCode);

        ProgramProject project;
        var isNew = !id.HasValue;
        string? fromStatus = null;
        if (id.HasValue)
        {
            project = await ProjectQuery(false).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (project is null)
                return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound);
            fromStatus = project.Status.ToString();
        }
        else
        {
            project = new ProgramProject();
            dbcontext.ProgramProjects.Add(project);
        }

        project.ProjectCode = projectCode;
        project.Name = request.Name.Trim();
        project.ProjectType = request.ProjectType.Trim();
        project.Description = request.Description?.Trim();
        project.ManagerName = request.ManagerName?.Trim();
        project.StartsAt = request.StartsAt?.Date;
        project.EndsAt = request.EndsAt?.Date;
        project.Status = request.Status;
        project.Budget = request.Budget;
        project.TargetBeneficiaries = request.TargetBeneficiaries;
        project.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueProjectActivity(
            project.Id,
            isNew ? ProgramProjectActivityType.Created : ProgramProjectActivityType.Updated,
            isNew ? "إنشاء المشروع/البرنامج" : "تحديث بيانات المشروع/البرنامج",
            project.Notes,
            fromStatus,
            project.Status.ToString(),
            null,
            project.ProjectCode);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadProjectCollectionsAsync(project, cancellationToken);
        return Result.Success(MapProject(project));
    }

    public async Task<Result<ProgramProjectResponse>> UpdateProjectStatusAsync(int id, UpdateProgramProjectStatusRequest request, CancellationToken cancellationToken = default)
    {
        var project = await ProjectQuery(false).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound);

        var fromStatus = project.Status.ToString();
        project.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            project.Notes = request.Notes.Trim();

        QueueProjectActivity(project.Id, ProgramProjectActivityType.StatusChanged, "تغيير حالة المشروع/البرنامج", request.Notes?.Trim(), fromStatus, project.Status.ToString());
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapProject(project));
    }

    public async Task<Result<ProgramProjectResponse>> PublishProjectAsync(int id, PublishProgramProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await ProjectQuery(false).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound);

        project.IsPublished = request.IsPublished;
        project.PublishedAt = request.IsPublished
            ? request.PublishedAt ?? DateTime.UtcNow.AddHours(3)
            : null;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            project.Notes = request.Notes.Trim();

        QueueProjectActivity(
            project.Id,
            ProgramProjectActivityType.Published,
            request.IsPublished ? "نشر البرنامج" : "إلغاء نشر البرنامج",
            request.Notes?.Trim(),
            null,
            request.IsPublished ? "Published" : "Unpublished",
            null,
            project.ProjectCode);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapProject(project));
    }

    public async Task<Result<ProgramProjectResponse>> SaveRegistrationFormAsync(int id, SaveProgramRegistrationFormRequest request, CancellationToken cancellationToken = default)
    {
        var project = await ProjectQuery(false).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (string.IsNullOrWhiteSpace(request.RegistrationFormJson))
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        project.SpecialProgramCategory = request.SpecialProgramCategory?.Trim();
        project.RegistrationFormJson = request.RegistrationFormJson.Trim();

        QueueProjectActivity(project.Id, ProgramProjectActivityType.RegistrationFormSaved, "حفظ نموذج تسجيل البرنامج", project.SpecialProgramCategory, null, null, null, project.ProjectCode);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapProject(project));
    }

    public async Task<Result<IEnumerable<ProgramProjectTaskResponse>>> GetTasksAsync(int? projectId, ProgramProjectTaskStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectTasks.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var tasks = await query.OrderBy(x => x.DueDate ?? DateTime.MaxValue).Select(x => MapTask(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectTaskResponse>>(tasks);
    }

    public async Task<Result<ProgramProjectTaskResponse>> SaveTaskAsync(int? id, SaveProgramProjectTaskRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectTaskResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || request.ProgressPercent < 0 || request.ProgressPercent > 100)
            return Result.Failure<ProgramProjectTaskResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramProjectTask task;
        if (id.HasValue)
        {
            task = await dbcontext.ProgramProjectTasks.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (task is null)
                return Result.Failure<ProgramProjectTaskResponse>(ProgramsProjectsErrors.TaskNotFound);
        }
        else
        {
            task = new ProgramProjectTask { ProgramProject = project };
            dbcontext.ProgramProjectTasks.Add(task);
        }

        task.ProgramProjectId = request.ProgramProjectId;
        task.Title = request.Title.Trim();
        task.OwnerName = request.OwnerName?.Trim();
        task.DueDate = request.DueDate?.Date;
        task.Status = request.Status;
        task.ProgressPercent = request.ProgressPercent;
        task.Notes = request.Notes?.Trim();

        QueueProjectActivity(task.ProgramProjectId, ProgramProjectActivityType.TaskSaved, $"حفظ مهمة: {task.Title}", task.Notes, null, task.Status.ToString(), null, task.OwnerName);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapTask(task));
    }

    public async Task<Result<IEnumerable<ProgramProjectMilestoneResponse>>> GetMilestonesAsync(int? projectId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectMilestones.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);

        var milestones = await query.OrderBy(x => x.StartsAt).Select(x => MapMilestone(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectMilestoneResponse>>(milestones);
    }

    public async Task<Result<ProgramProjectMilestoneResponse>> SaveMilestoneAsync(int? id, SaveProgramProjectMilestoneRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectMilestoneResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || request.EndsAt.Date < request.StartsAt.Date || request.ProgressPercent < 0 || request.ProgressPercent > 100)
            return Result.Failure<ProgramProjectMilestoneResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramProjectMilestone milestone;
        if (id.HasValue)
        {
            milestone = await dbcontext.ProgramProjectMilestones.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (milestone is null)
                return Result.Failure<ProgramProjectMilestoneResponse>(ProgramsProjectsErrors.MilestoneNotFound);
        }
        else
        {
            milestone = new ProgramProjectMilestone { ProgramProject = project };
            dbcontext.ProgramProjectMilestones.Add(milestone);
        }

        milestone.ProgramProjectId = request.ProgramProjectId;
        milestone.Title = request.Title.Trim();
        milestone.StartsAt = request.StartsAt.Date;
        milestone.EndsAt = request.EndsAt.Date;
        milestone.ProgressPercent = request.ProgressPercent;
        milestone.Notes = request.Notes?.Trim();

        QueueProjectActivity(milestone.ProgramProjectId, ProgramProjectActivityType.MilestoneSaved, $"حفظ مرحلة: {milestone.Title}", milestone.Notes, null, null, null, $"{milestone.StartsAt:yyyy-MM-dd} - {milestone.EndsAt:yyyy-MM-dd}");
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapMilestone(milestone));
    }

    public async Task<Result<IEnumerable<ProgramProjectContractResponse>>> GetContractsAsync(int? projectId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectContracts.AsNoTracking().Include(x => x.ProgramProject).Include(x => x.ProgramSupplier).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);

        var contracts = await query.OrderByDescending(x => x.SignedAt).Select(x => MapContract(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectContractResponse>>(contracts);
    }

    public async Task<Result<ProgramProjectContractResponse>> SaveContractAsync(int? id, SaveProgramProjectContractRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectContractResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (request.ProgramSupplierId.HasValue && !await dbcontext.ProgramSuppliers.AnyAsync(x => x.Id == request.ProgramSupplierId.Value, cancellationToken))
            return Result.Failure<ProgramProjectContractResponse>(ProgramsProjectsErrors.SupplierNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || request.Amount < 0)
            return Result.Failure<ProgramProjectContractResponse>(ProgramsProjectsErrors.InvalidRequest);

        var contractNumber = string.IsNullOrWhiteSpace(request.ContractNumber)
            ? await GenerateContractNumberAsync(cancellationToken)
            : request.ContractNumber.Trim();
        var duplicateContract = await dbcontext.ProgramProjectContracts.AnyAsync(x => x.ContractNumber == contractNumber && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicateContract)
            return Result.Failure<ProgramProjectContractResponse>(ProgramsProjectsErrors.DuplicateContractNumber);

        ProgramProjectContract contract;
        if (id.HasValue)
        {
            contract = await dbcontext.ProgramProjectContracts.Include(x => x.ProgramProject).Include(x => x.ProgramSupplier).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (contract is null)
                return Result.Failure<ProgramProjectContractResponse>(ProgramsProjectsErrors.ContractNotFound);
        }
        else
        {
            contract = new ProgramProjectContract { ProgramProject = project };
            dbcontext.ProgramProjectContracts.Add(contract);
        }

        contract.ProgramProjectId = request.ProgramProjectId;
        contract.ProgramSupplierId = request.ProgramSupplierId;
        contract.ContractNumber = contractNumber;
        contract.Title = request.Title.Trim();
        contract.Amount = request.Amount;
        contract.SignedAt = request.SignedAt.Date;
        contract.EndsAt = request.EndsAt?.Date;
        contract.Notes = request.Notes?.Trim();

        QueueProjectActivity(contract.ProgramProjectId, ProgramProjectActivityType.ContractSaved, $"حفظ عقد: {contract.Title}", contract.Notes, null, null, contract.Amount, contract.ContractNumber);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(contract).Reference(x => x.ProgramSupplier).LoadAsync(cancellationToken);
        return Result.Success(MapContract(contract));
    }

    public async Task<Result<IEnumerable<ProgramProjectFinanceEntryResponse>>> GetFinanceEntriesAsync(int? projectId, ProgramProjectFinanceEntryType? entryType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectFinanceEntries.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (entryType.HasValue)
            query = query.Where(x => x.EntryType == entryType.Value);

        var entries = await query.OrderByDescending(x => x.EntryDate).Select(x => MapFinanceEntry(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectFinanceEntryResponse>>(entries);
    }

    public async Task<Result<ProgramProjectFinanceEntryResponse>> AddFinanceEntryAsync(AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectFinanceEntryResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.SourceOrPayee))
            return Result.Failure<ProgramProjectFinanceEntryResponse>(ProgramsProjectsErrors.InvalidRequest);

        var entry = new ProgramProjectFinanceEntry
        {
            ProgramProjectId = request.ProgramProjectId,
            ProgramProject = project,
            EntryType = request.EntryType,
            EntryDate = request.EntryDate ?? DateTime.UtcNow.AddHours(3),
            Amount = request.Amount,
            SourceOrPayee = request.SourceOrPayee.Trim(),
            ReferenceNumber = request.ReferenceNumber?.Trim(),
            Notes = request.Notes?.Trim()
        };

        dbcontext.ProgramProjectFinanceEntries.Add(entry);
        QueueProjectActivity(entry.ProgramProjectId, ProgramProjectActivityType.FinanceEntryAdded, $"حركة مالية: {entry.EntryType}", entry.Notes, null, null, entry.Amount, entry.ReferenceNumber ?? entry.SourceOrPayee);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapFinanceEntry(entry));
    }

    public async Task<Result<IEnumerable<ProgramProjectAssignmentResponse>>> GetAssignmentsAsync(int? projectId, ProgramProjectAssignmentType? assignmentType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectAssignments.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (assignmentType.HasValue)
            query = query.Where(x => x.AssignmentType == assignmentType.Value);

        var assignments = await query.OrderByDescending(x => x.AssignedAt).Select(x => MapAssignment(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectAssignmentResponse>>(assignments);
    }

    public async Task<Result<ProgramProjectAssignmentResponse>> AddAssignmentAsync(AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectAssignmentResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return Result.Failure<ProgramProjectAssignmentResponse>(ProgramsProjectsErrors.InvalidRequest);

        var assignment = new ProgramProjectAssignment
        {
            ProgramProjectId = request.ProgramProjectId,
            ProgramProject = project,
            AssignmentType = request.AssignmentType,
            DisplayName = request.DisplayName.Trim(),
            ExternalReference = request.ExternalReference?.Trim(),
            AssignedAt = request.AssignedAt ?? DateTime.UtcNow.AddHours(3),
            Notes = request.Notes?.Trim()
        };

        dbcontext.ProgramProjectAssignments.Add(assignment);
        QueueProjectActivity(assignment.ProgramProjectId, ProgramProjectActivityType.AssignmentAdded, $"إلحاق {assignment.AssignmentType}: {assignment.DisplayName}", assignment.Notes, null, null, null, assignment.ExternalReference);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAssignment(assignment));
    }

    public async Task<Result<IEnumerable<ProgramProjectReportResponse>>> GetReportsAsync(int? projectId, string? reportType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramProjectReports.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(reportType))
        {
            var value = reportType.Trim();
            query = query.Where(x => x.ReportType.Contains(value));
        }

        var reports = await query.OrderByDescending(x => x.ReportDate).Select(x => MapReport(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramProjectReportResponse>>(reports);
    }

    public async Task<Result<ProgramProjectReportResponse>> AddReportAsync(AddProgramProjectReportRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectReportResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.ReportType) || string.IsNullOrWhiteSpace(request.Summary))
            return Result.Failure<ProgramProjectReportResponse>(ProgramsProjectsErrors.InvalidRequest);

        var report = new ProgramProjectReport
        {
            ProgramProjectId = request.ProgramProjectId,
            ProgramProject = project,
            ReportType = request.ReportType.Trim(),
            ReportDate = request.ReportDate ?? DateTime.UtcNow.AddHours(3),
            Summary = request.Summary.Trim(),
            FilePath = request.FilePath?.Trim()
        };

        dbcontext.ProgramProjectReports.Add(report);
        QueueProjectActivity(report.ProgramProjectId, ProgramProjectActivityType.ReportAdded, $"تقرير: {report.ReportType}", report.Summary, null, null, null, report.FilePath);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReport(report));
    }

    public async Task<Result<IEnumerable<ProgramProjectActivityResponse>>> GetProjectActivitiesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await dbcontext.ProgramProjects.AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
            return Result.Failure<IEnumerable<ProgramProjectActivityResponse>>(ProgramsProjectsErrors.ProjectNotFound);

        var activities = await dbcontext.ProgramProjectActivities
            .AsNoTracking()
            .Include(x => x.ProgramProject)
            .Where(x => x.ProgramProjectId == projectId)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Select(x => MapActivity(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProgramProjectActivityResponse>>(activities);
    }

    public async Task<Result<IEnumerable<ProgramSupplierResponse>>> GetSuppliersAsync(string? search, ProgramSupplierStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSuppliers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(value) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(value)) ||
                (x.Mobile != null && x.Mobile.Contains(value)) ||
                (x.Email != null && x.Email.Contains(value)));
        }
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var suppliers = await query.OrderBy(x => x.Name).Select(x => MapSupplier(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramSupplierResponse>>(suppliers);
    }

    public async Task<Result<ProgramSupplierResponse>> SaveSupplierAsync(int? id, SaveProgramSupplierRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<ProgramSupplierResponse>(ProgramsProjectsErrors.InvalidRequest);

        var name = request.Name.Trim();
        var duplicate = await dbcontext.ProgramSuppliers.AnyAsync(x => x.Name == name && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicate)
            return Result.Failure<ProgramSupplierResponse>(ProgramsProjectsErrors.DuplicateSupplierName);

        ProgramSupplier supplier;
        if (id.HasValue)
        {
            supplier = await dbcontext.ProgramSuppliers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (supplier is null)
                return Result.Failure<ProgramSupplierResponse>(ProgramsProjectsErrors.SupplierNotFound);
        }
        else
        {
            supplier = new ProgramSupplier();
            dbcontext.ProgramSuppliers.Add(supplier);
        }

        supplier.Name = name;
        supplier.ContactPerson = request.ContactPerson?.Trim();
        supplier.Mobile = request.Mobile?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.City = request.City?.Trim();
        supplier.Status = request.Status;
        supplier.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSupplier(supplier));
    }

    public async Task<Result<IEnumerable<ProgramIdeaResponse>>> GetIdeasAsync(ProgramIdeaStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramIdeas.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var ideas = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapIdea(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramIdeaResponse>>(ideas);
    }

    public async Task<Result<ProgramIdeaResponse>> SaveIdeaAsync(int? id, SaveProgramIdeaRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || request.EstimatedBudget < 0)
            return Result.Failure<ProgramIdeaResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramIdea idea;
        if (id.HasValue)
        {
            idea = await dbcontext.ProgramIdeas.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (idea is null)
                return Result.Failure<ProgramIdeaResponse>(ProgramsProjectsErrors.IdeaNotFound);
        }
        else
        {
            idea = new ProgramIdea();
            dbcontext.ProgramIdeas.Add(idea);
        }

        idea.Title = request.Title.Trim();
        idea.OwnerName = request.OwnerName?.Trim();
        idea.Description = request.Description.Trim();
        idea.MarketingNotes = request.MarketingNotes?.Trim();
        idea.EstimatedBudget = request.EstimatedBudget;
        idea.Status = request.Status;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapIdea(idea));
    }

    public async Task<Result<ProgramIdeaResponse>> UpdateIdeaStatusAsync(int id, UpdateProgramIdeaStatusRequest request, CancellationToken cancellationToken = default)
    {
        var idea = await dbcontext.ProgramIdeas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (idea is null)
            return Result.Failure<ProgramIdeaResponse>(ProgramsProjectsErrors.IdeaNotFound);

        idea.Status = request.Status;
        idea.DecisionNotes = request.DecisionNotes?.Trim();
        idea.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapIdea(idea));
    }

    public async Task<Result<IEnumerable<ProgramApprovalResponse>>> GetApprovalsAsync(ProgramApprovalStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramApprovals.AsNoTracking().Include(x => x.ProgramIdea).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var approvals = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapApproval(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramApprovalResponse>>(approvals);
    }

    public async Task<Result<ProgramApprovalResponse>> SaveApprovalAsync(int? id, SaveProgramApprovalRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ApprovalType) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<ProgramApprovalResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramIdea? idea = null;
        if (request.ProgramIdeaId.HasValue)
        {
            idea = await dbcontext.ProgramIdeas.FirstOrDefaultAsync(x => x.Id == request.ProgramIdeaId.Value, cancellationToken);
            if (idea is null)
                return Result.Failure<ProgramApprovalResponse>(ProgramsProjectsErrors.IdeaNotFound);
        }

        ProgramApproval approval;
        if (id.HasValue)
        {
            approval = await dbcontext.ProgramApprovals.Include(x => x.ProgramIdea).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (approval is null)
                return Result.Failure<ProgramApprovalResponse>(ProgramsProjectsErrors.ApprovalNotFound);
        }
        else
        {
            approval = new ProgramApproval();
            dbcontext.ProgramApprovals.Add(approval);
        }

        approval.ProgramIdeaId = request.ProgramIdeaId;
        approval.ProgramIdea = idea;
        approval.ApprovalType = request.ApprovalType.Trim();
        approval.Title = request.Title.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapApproval(approval));
    }

    public async Task<Result<ProgramApprovalResponse>> DecideApprovalAsync(int id, DecideProgramApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var approval = await dbcontext.ProgramApprovals.Include(x => x.ProgramIdea).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (approval is null)
            return Result.Failure<ProgramApprovalResponse>(ProgramsProjectsErrors.ApprovalNotFound);

        approval.Status = request.Status;
        approval.DecisionNotes = request.DecisionNotes?.Trim();
        approval.DecidedAt = DateTime.UtcNow.AddHours(3);

        if (approval.ProgramIdea is not null)
        {
            approval.ProgramIdea.Status = request.Status switch
            {
                ProgramApprovalStatus.Approved => ProgramIdeaStatus.Approved,
                ProgramApprovalStatus.Rejected => ProgramIdeaStatus.Rejected,
                ProgramApprovalStatus.Completed => ProgramIdeaStatus.Completed,
                ProgramApprovalStatus.Cancelled => ProgramIdeaStatus.Cancelled,
                _ => approval.ProgramIdea.Status
            };
            approval.ProgramIdea.DecisionNotes = request.DecisionNotes?.Trim();
            approval.ProgramIdea.DecidedAt = approval.DecidedAt;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapApproval(approval));
    }

    public async Task<Result<IEnumerable<ProgramRegistrationResponse>>> GetRegistrationsAsync(int? projectId, ProgramRegistrationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramRegistrations.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var registrations = await query.OrderByDescending(x => x.RegisteredAt).Select(x => MapRegistration(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramRegistrationResponse>>(registrations);
    }

    public async Task<Result<ProgramRegistrationResponse>> SaveRegistrationAsync(int? id, SaveProgramRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramRegistrationResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (string.IsNullOrWhiteSpace(request.ParticipantName))
            return Result.Failure<ProgramRegistrationResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramRegistration registration;
        if (id.HasValue)
        {
            registration = await dbcontext.ProgramRegistrations.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (registration is null)
                return Result.Failure<ProgramRegistrationResponse>(ProgramsProjectsErrors.RegistrationNotFound);
        }
        else
        {
            registration = new ProgramRegistration { ProgramProject = project };
            dbcontext.ProgramRegistrations.Add(registration);
        }

        registration.ProgramProjectId = request.ProgramProjectId;
        registration.ParticipantName = request.ParticipantName.Trim();
        registration.Mobile = request.Mobile?.Trim();
        registration.Email = request.Email?.Trim();
        registration.ExternalReference = request.ExternalReference?.Trim();
        registration.RegisteredAt = request.RegisteredAt ?? DateTime.UtcNow.AddHours(3);
        registration.Notes = request.Notes?.Trim();

        QueueProjectActivity(registration.ProgramProjectId, ProgramProjectActivityType.RegistrationSaved, $"تسجيل مشارك: {registration.ParticipantName}", registration.Notes, null, registration.Status.ToString(), null, registration.ExternalReference);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRegistration(registration));
    }

    public async Task<Result<ProgramRegistrationResponse>> DecideRegistrationAsync(int id, DecideProgramRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var registration = await dbcontext.ProgramRegistrations.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (registration is null)
            return Result.Failure<ProgramRegistrationResponse>(ProgramsProjectsErrors.RegistrationNotFound);

        var fromStatus = registration.Status.ToString();
        registration.Status = request.Status;
        registration.DecisionNotes = request.DecisionNotes?.Trim();
        registration.DecidedAt = DateTime.UtcNow.AddHours(3);
        QueueProjectActivity(registration.ProgramProjectId, ProgramProjectActivityType.RegistrationDecided, $"قرار تسجيل: {registration.ParticipantName}", registration.DecisionNotes, fromStatus, registration.Status.ToString(), null, registration.ExternalReference);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRegistration(registration));
    }

    public async Task<Result<IEnumerable<ProgramSessionResponse>>> GetSessionsAsync(int? projectId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSessions.AsNoTracking().Include(x => x.ProgramProject).Include(x => x.AttendanceRecords).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);

        var sessions = await query.OrderBy(x => x.StartsAt).Select(x => MapSession(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramSessionResponse>>(sessions);
    }

    public async Task<Result<ProgramSessionResponse>> SaveSessionAsync(int? id, SaveProgramSessionRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramSessionResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (string.IsNullOrWhiteSpace(request.Title) || request.EndsAt < request.StartsAt)
            return Result.Failure<ProgramSessionResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramSession session;
        if (id.HasValue)
        {
            session = await dbcontext.ProgramSessions.Include(x => x.ProgramProject).Include(x => x.AttendanceRecords).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (session is null)
                return Result.Failure<ProgramSessionResponse>(ProgramsProjectsErrors.SessionNotFound);
        }
        else
        {
            session = new ProgramSession { ProgramProject = project };
            dbcontext.ProgramSessions.Add(session);
        }

        session.ProgramProjectId = request.ProgramProjectId;
        session.Title = request.Title.Trim();
        session.StartsAt = request.StartsAt;
        session.EndsAt = request.EndsAt;
        session.Location = request.Location?.Trim();
        session.Notes = request.Notes?.Trim();

        QueueProjectActivity(session.ProgramProjectId, ProgramProjectActivityType.SessionSaved, $"حفظ موعد: {session.Title}", session.Notes, null, null, null, session.Location);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSession(session));
    }

    public async Task<Result<IEnumerable<ProgramSessionAttendanceResponse>>> GetAttendanceAsync(int? sessionId, ProgramAttendanceStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSessionAttendanceRecords
            .AsNoTracking()
            .Include(x => x.ProgramSession)
            .ThenInclude(x => x!.ProgramProject)
            .AsQueryable();

        if (sessionId.HasValue)
            query = query.Where(x => x.ProgramSessionId == sessionId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var attendance = await query.OrderBy(x => x.ParticipantName).Select(x => MapAttendance(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramSessionAttendanceResponse>>(attendance);
    }

    public async Task<Result<ProgramSessionAttendanceResponse>> SaveAttendanceAsync(int? id, SaveProgramSessionAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var session = await dbcontext.ProgramSessions.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == request.ProgramSessionId, cancellationToken);
        if (session is null)
            return Result.Failure<ProgramSessionAttendanceResponse>(ProgramsProjectsErrors.SessionNotFound);
        if (string.IsNullOrWhiteSpace(request.ParticipantName))
            return Result.Failure<ProgramSessionAttendanceResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramSessionAttendance attendance;
        if (id.HasValue)
        {
            attendance = await dbcontext.ProgramSessionAttendanceRecords.Include(x => x.ProgramSession).ThenInclude(x => x!.ProgramProject).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (attendance is null)
                return Result.Failure<ProgramSessionAttendanceResponse>(ProgramsProjectsErrors.SessionNotFound);
        }
        else
        {
            attendance = new ProgramSessionAttendance { ProgramSession = session };
            dbcontext.ProgramSessionAttendanceRecords.Add(attendance);
        }

        attendance.ProgramSessionId = request.ProgramSessionId;
        attendance.ParticipantName = request.ParticipantName.Trim();
        attendance.ExternalReference = request.ExternalReference?.Trim();
        attendance.Status = request.Status;
        attendance.Notes = request.Notes?.Trim();

        QueueProjectActivity(session.ProgramProjectId, ProgramProjectActivityType.AttendanceSaved, $"حضور مشارك: {attendance.ParticipantName}", attendance.Notes, null, attendance.Status.ToString(), null, session.Title);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendance(attendance));
    }

    public async Task<Result<IEnumerable<ProgramSurveyResponse>>> GetSurveysAsync(int? projectId, ProgramSurveyStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSurveys.AsNoTracking().Include(x => x.ProgramProject).Include(x => x.Submissions).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var surveys = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapSurvey(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramSurveyResponse>>(surveys);
    }

    public async Task<Result<ProgramSurveyResponse>> SaveSurveyAsync(int? id, SaveProgramSurveyRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramSurveyResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.QuestionsJson))
            return Result.Failure<ProgramSurveyResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramSurvey survey;
        if (id.HasValue)
        {
            survey = await dbcontext.ProgramSurveys.Include(x => x.ProgramProject).Include(x => x.Submissions).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (survey is null)
                return Result.Failure<ProgramSurveyResponse>(ProgramsProjectsErrors.SurveyNotFound);
        }
        else
        {
            survey = new ProgramSurvey { ProgramProject = project };
            dbcontext.ProgramSurveys.Add(survey);
        }

        survey.ProgramProjectId = request.ProgramProjectId;
        survey.Title = request.Title.Trim();
        survey.QuestionsJson = request.QuestionsJson.Trim();
        survey.Status = request.Status;
        survey.Notes = request.Notes?.Trim();

        QueueProjectActivity(survey.ProgramProjectId, ProgramProjectActivityType.SurveySaved, $"حفظ استبيان: {survey.Title}", survey.Notes, null, survey.Status.ToString());
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSurvey(survey));
    }

    public async Task<Result<IEnumerable<ProgramSurveySubmissionResponse>>> GetSurveySubmissionsAsync(int? surveyId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSurveySubmissions.AsNoTracking().Include(x => x.ProgramSurvey).AsQueryable();
        if (surveyId.HasValue)
            query = query.Where(x => x.ProgramSurveyId == surveyId.Value);

        var submissions = await query.OrderByDescending(x => x.SubmittedAt).Select(x => MapSurveySubmission(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramSurveySubmissionResponse>>(submissions);
    }

    public async Task<Result<ProgramSurveySubmissionResponse>> AddSurveySubmissionAsync(AddProgramSurveySubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var survey = await dbcontext.ProgramSurveys.FirstOrDefaultAsync(x => x.Id == request.ProgramSurveyId, cancellationToken);
        if (survey is null)
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.SurveyNotFound);
        if (string.IsNullOrWhiteSpace(request.RespondentName) || string.IsNullOrWhiteSpace(request.AnswersJson))
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.InvalidRequest);

        var submission = new ProgramSurveySubmission
        {
            ProgramSurveyId = request.ProgramSurveyId,
            ProgramSurvey = survey,
            RespondentName = request.RespondentName.Trim(),
            AnswersJson = request.AnswersJson.Trim(),
            SubmittedAt = request.SubmittedAt ?? DateTime.UtcNow.AddHours(3)
        };

        dbcontext.ProgramSurveySubmissions.Add(submission);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSurveySubmission(submission));
    }

    public async Task<Result<IEnumerable<ProgramCertificateTemplateResponse>>> GetCertificateTemplatesAsync(int? projectId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramCertificateTemplates.AsNoTracking().Include(x => x.ProgramProject).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var templates = await query.OrderBy(x => x.Name).Select(x => MapCertificateTemplate(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramCertificateTemplateResponse>>(templates);
    }

    public async Task<Result<ProgramCertificateTemplateResponse>> SaveCertificateTemplateAsync(int? id, SaveProgramCertificateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.BodyTemplate))
            return Result.Failure<ProgramCertificateTemplateResponse>(ProgramsProjectsErrors.InvalidRequest);
        if (request.ProgramProjectId.HasValue && !await dbcontext.ProgramProjects.AnyAsync(x => x.Id == request.ProgramProjectId.Value, cancellationToken))
            return Result.Failure<ProgramCertificateTemplateResponse>(ProgramsProjectsErrors.ProjectNotFound);

        ProgramCertificateTemplate template;
        if (id.HasValue)
        {
            template = await dbcontext.ProgramCertificateTemplates.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (template is null)
                return Result.Failure<ProgramCertificateTemplateResponse>(ProgramsProjectsErrors.CertificateTemplateNotFound);
        }
        else
        {
            template = new ProgramCertificateTemplate();
            dbcontext.ProgramCertificateTemplates.Add(template);
        }

        template.ProgramProjectId = request.ProgramProjectId;
        template.Name = request.Name.Trim();
        template.BodyTemplate = request.BodyTemplate.Trim();
        template.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(template).Reference(x => x.ProgramProject).LoadAsync(cancellationToken);
        return Result.Success(MapCertificateTemplate(template));
    }

    public async Task<Result<IEnumerable<ProgramCertificateIssueResponse>>> GetCertificateIssuesAsync(int? projectId, ProgramCertificateIssueStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramCertificateIssues
            .AsNoTracking()
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramCertificateTemplate)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var issues = await query.OrderByDescending(x => x.IssuedAt).Select(x => MapCertificateIssue(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramCertificateIssueResponse>>(issues);
    }

    public async Task<Result<ProgramCertificateIssueResponse>> IssueCertificateAsync(IssueProgramCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (request.ProgramCertificateTemplateId.HasValue && !await dbcontext.ProgramCertificateTemplates.AnyAsync(x => x.Id == request.ProgramCertificateTemplateId.Value, cancellationToken))
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.CertificateTemplateNotFound);
        if (string.IsNullOrWhiteSpace(request.RecipientName))
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.InvalidRequest);

        var certificateNumber = string.IsNullOrWhiteSpace(request.CertificateNumber)
            ? await GenerateCertificateNumberAsync(cancellationToken)
            : request.CertificateNumber.Trim();
        var duplicate = await dbcontext.ProgramCertificateIssues.AnyAsync(x => x.CertificateNumber == certificateNumber, cancellationToken);
        if (duplicate)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.DuplicateCertificateNumber);

        var issue = new ProgramCertificateIssue
        {
            ProgramProjectId = request.ProgramProjectId,
            ProgramProject = project,
            ProgramCertificateTemplateId = request.ProgramCertificateTemplateId,
            CertificateNumber = certificateNumber,
            RecipientName = request.RecipientName.Trim(),
            IssuedAt = request.IssuedAt ?? DateTime.UtcNow.AddHours(3),
            Notes = request.Notes?.Trim()
        };

        dbcontext.ProgramCertificateIssues.Add(issue);
        QueueProjectActivity(issue.ProgramProjectId, ProgramProjectActivityType.CertificateIssued, $"إصدار شهادة: {issue.RecipientName}", issue.Notes, null, issue.Status.ToString(), null, certificateNumber);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(issue).Reference(x => x.ProgramCertificateTemplate).LoadAsync(cancellationToken);
        return Result.Success(MapCertificateIssue(issue));
    }

    public async Task<Result<IEnumerable<ProgramQualificationCaseResponse>>> GetQualificationCasesAsync(ProgramQualificationCaseStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramQualificationCases.AsNoTracking().Include(x => x.ProgramProject).Include(x => x.Installments).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var cases = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapQualificationCase(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramQualificationCaseResponse>>(cases);
    }

    public async Task<Result<ProgramQualificationCaseResponse>> SaveQualificationCaseAsync(int? id, SaveProgramQualificationCaseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.BeneficiaryName) || string.IsNullOrWhiteSpace(request.NeedSummary) || request.ApprovedAmount < 0 || request.InstallmentCount < 0)
            return Result.Failure<ProgramQualificationCaseResponse>(ProgramsProjectsErrors.InvalidRequest);
        if (request.ProgramProjectId.HasValue && !await dbcontext.ProgramProjects.AnyAsync(x => x.Id == request.ProgramProjectId.Value, cancellationToken))
            return Result.Failure<ProgramQualificationCaseResponse>(ProgramsProjectsErrors.ProjectNotFound);

        ProgramQualificationCase qualificationCase;
        if (id.HasValue)
        {
            qualificationCase = await dbcontext.ProgramQualificationCases.Include(x => x.ProgramProject).Include(x => x.Installments).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (qualificationCase is null)
                return Result.Failure<ProgramQualificationCaseResponse>(ProgramsProjectsErrors.QualificationCaseNotFound);
        }
        else
        {
            qualificationCase = new ProgramQualificationCase();
            dbcontext.ProgramQualificationCases.Add(qualificationCase);
        }

        qualificationCase.ProgramProjectId = request.ProgramProjectId;
        qualificationCase.BeneficiaryName = request.BeneficiaryName.Trim();
        qualificationCase.NeedSummary = request.NeedSummary.Trim();
        qualificationCase.ManagementOpinion = request.ManagementOpinion?.Trim();
        qualificationCase.Status = request.Status;
        qualificationCase.ApprovedAmount = request.ApprovedAmount;
        qualificationCase.InstallmentCount = request.InstallmentCount;
        qualificationCase.Notes = request.Notes?.Trim();

        if (qualificationCase.ProgramProjectId.HasValue)
            QueueProjectActivity(qualificationCase.ProgramProjectId.Value, ProgramProjectActivityType.QualificationStatusChanged, $"حفظ تأهيل: {qualificationCase.BeneficiaryName}", qualificationCase.Notes ?? qualificationCase.ManagementOpinion, null, qualificationCase.Status.ToString(), qualificationCase.ApprovedAmount);

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(qualificationCase).Reference(x => x.ProgramProject).LoadAsync(cancellationToken);
        return Result.Success(MapQualificationCase(qualificationCase));
    }

    public async Task<Result<ProgramQualificationCaseResponse>> UpdateQualificationCaseStatusAsync(int id, UpdateProgramQualificationCaseStatusRequest request, CancellationToken cancellationToken = default)
    {
        var qualificationCase = await dbcontext.ProgramQualificationCases.Include(x => x.ProgramProject).Include(x => x.Installments).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (qualificationCase is null)
            return Result.Failure<ProgramQualificationCaseResponse>(ProgramsProjectsErrors.QualificationCaseNotFound);

        var fromStatus = qualificationCase.Status.ToString();
        qualificationCase.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ManagementOpinion))
            qualificationCase.ManagementOpinion = request.ManagementOpinion.Trim();
        if (!string.IsNullOrWhiteSpace(request.Notes))
            qualificationCase.Notes = request.Notes.Trim();

        if (qualificationCase.ProgramProjectId.HasValue)
            QueueProjectActivity(qualificationCase.ProgramProjectId.Value, ProgramProjectActivityType.QualificationStatusChanged, $"تحديث تأهيل: {qualificationCase.BeneficiaryName}", qualificationCase.Notes ?? qualificationCase.ManagementOpinion, fromStatus, qualificationCase.Status.ToString(), qualificationCase.ApprovedAmount);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapQualificationCase(qualificationCase));
    }

    public async Task<Result<IEnumerable<ProgramQualificationInstallmentResponse>>> GetQualificationInstallmentsAsync(int? caseId, ProgramQualificationInstallmentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramQualificationInstallments.AsNoTracking().Include(x => x.ProgramQualificationCase).AsQueryable();
        if (caseId.HasValue)
            query = query.Where(x => x.ProgramQualificationCaseId == caseId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var installments = await query.OrderBy(x => x.DueDate).Select(x => MapQualificationInstallment(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramQualificationInstallmentResponse>>(installments);
    }

    public async Task<Result<ProgramQualificationInstallmentResponse>> SaveQualificationInstallmentAsync(int? id, SaveProgramQualificationInstallmentRequest request, CancellationToken cancellationToken = default)
    {
        var qualificationCase = await dbcontext.ProgramQualificationCases.FirstOrDefaultAsync(x => x.Id == request.ProgramQualificationCaseId, cancellationToken);
        if (qualificationCase is null)
            return Result.Failure<ProgramQualificationInstallmentResponse>(ProgramsProjectsErrors.QualificationCaseNotFound);
        if (request.Amount <= 0)
            return Result.Failure<ProgramQualificationInstallmentResponse>(ProgramsProjectsErrors.InvalidRequest);

        ProgramQualificationInstallment installment;
        if (id.HasValue)
        {
            installment = await dbcontext.ProgramQualificationInstallments.Include(x => x.ProgramQualificationCase).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (installment is null)
                return Result.Failure<ProgramQualificationInstallmentResponse>(ProgramsProjectsErrors.QualificationInstallmentNotFound);
        }
        else
        {
            installment = new ProgramQualificationInstallment { ProgramQualificationCase = qualificationCase };
            dbcontext.ProgramQualificationInstallments.Add(installment);
        }

        installment.ProgramQualificationCaseId = request.ProgramQualificationCaseId;
        installment.DueDate = request.DueDate.Date;
        installment.Amount = request.Amount;
        installment.Notes = request.Notes?.Trim();
        installment.Status = installment.PaidAmount >= request.Amount ? ProgramQualificationInstallmentStatus.Paid : ProgramQualificationInstallmentStatus.Pending;

        if (qualificationCase.ProgramProjectId.HasValue)
            QueueProjectActivity(qualificationCase.ProgramProjectId.Value, ProgramProjectActivityType.FinanceEntryAdded, $"قسط تأهيل: {qualificationCase.BeneficiaryName}", installment.Notes, null, installment.Status.ToString(), installment.Amount, $"{installment.DueDate:yyyy-MM-dd}");

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapQualificationInstallment(installment));
    }

    public async Task<Result<ProgramQualificationInstallmentResponse>> RecordQualificationInstallmentPaymentAsync(int id, RecordQualificationInstallmentPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var installment = await dbcontext.ProgramQualificationInstallments
            .Include(x => x.ProgramQualificationCase)
            .ThenInclude(x => x!.Installments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (installment is null)
            return Result.Failure<ProgramQualificationInstallmentResponse>(ProgramsProjectsErrors.QualificationInstallmentNotFound);
        if (request.PaidAmount < 0)
            return Result.Failure<ProgramQualificationInstallmentResponse>(ProgramsProjectsErrors.InvalidRequest);

        installment.PaidAmount = request.PaidAmount;
        installment.PaidAt = request.PaidAt ?? DateTime.UtcNow.AddHours(3);
        installment.Notes = request.Notes?.Trim();
        installment.Status = request.PaidAmount >= installment.Amount
            ? ProgramQualificationInstallmentStatus.Paid
            : installment.DueDate.Date < DateTime.UtcNow.AddHours(3).Date
                ? ProgramQualificationInstallmentStatus.Late
                : ProgramQualificationInstallmentStatus.Pending;

        if (installment.ProgramQualificationCase is not null)
        {
            if (installment.ProgramQualificationCase.Installments.All(x => x.Id == installment.Id || x.Status == ProgramQualificationInstallmentStatus.Paid) && installment.Status == ProgramQualificationInstallmentStatus.Paid)
                installment.ProgramQualificationCase.Status = ProgramQualificationCaseStatus.Paid;
            else if (installment.ProgramQualificationCase.Installments.Any(x => x.Id == installment.Id ? installment.Status == ProgramQualificationInstallmentStatus.Late : x.Status == ProgramQualificationInstallmentStatus.Late))
                installment.ProgramQualificationCase.Status = ProgramQualificationCaseStatus.Late;

            if (installment.ProgramQualificationCase.ProgramProjectId.HasValue)
                QueueProjectActivity(installment.ProgramQualificationCase.ProgramProjectId.Value, ProgramProjectActivityType.FinanceEntryAdded, $"سداد قسط تأهيل: {installment.ProgramQualificationCase.BeneficiaryName}", installment.Notes, null, installment.Status.ToString(), installment.PaidAmount, $"{installment.PaidAt:yyyy-MM-dd}");
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapQualificationInstallment(installment));
    }

    private IQueryable<ProgramProject> ProjectQuery(bool asNoTracking = true)
    {
        var query = dbcontext.ProgramProjects
            .Include(x => x.Tasks)
            .Include(x => x.FinanceEntries)
            .AsQueryable();

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private async Task LoadProjectCollectionsAsync(ProgramProject project, CancellationToken cancellationToken)
    {
        await dbcontext.Entry(project).Collection(x => x.Tasks).LoadAsync(cancellationToken);
        await dbcontext.Entry(project).Collection(x => x.FinanceEntries).LoadAsync(cancellationToken);
    }

    private void QueueProjectActivity(
        int projectId,
        ProgramProjectActivityType type,
        string title,
        string? note = null,
        string? fromStatus = null,
        string? toStatus = null,
        decimal? amount = null,
        string? reference = null)
    {
        dbcontext.ProgramProjectActivities.Add(new ProgramProjectActivity
        {
            ProgramProjectId = projectId,
            Type = type,
            Title = title.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Amount = amount,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            OccurredAt = DateTime.UtcNow.AddHours(3)
        });
    }

    private async Task<string> GenerateProjectCodeAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.ProgramProjects.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"PRJ-{year}-{count:0000}";
    }

    private async Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.ProgramProjectContracts.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"CTR-{year}-{count:0000}";
    }

    private async Task<string> GenerateCertificateNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.ProgramCertificateIssues.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"CERT-{year}-{count:0000}";
    }

    private static ProgramProjectResponse MapProject(ProgramProject project)
    {
        var income = project.FinanceEntries.Where(x => x.EntryType == ProgramProjectFinanceEntryType.Income).Sum(x => x.Amount);
        var expenses = project.FinanceEntries
            .Where(x => x.EntryType is ProgramProjectFinanceEntryType.Expense or ProgramProjectFinanceEntryType.Custody)
            .Sum(x => x.Amount);

        return new(
            project.Id,
            project.ProjectCode,
            project.Name,
            project.ProjectType,
            project.Description,
            project.ManagerName,
            project.StartsAt,
            project.EndsAt,
            project.Status.ToString(),
            project.Budget,
            project.TargetBeneficiaries,
            income,
            expenses,
            income - expenses,
            project.Tasks.Count,
            project.Tasks.Count(x => x.Status is ProgramProjectTaskStatus.Completed or ProgramProjectTaskStatus.Finished),
            project.IsPublished,
            project.PublishedAt,
            project.RegistrationFormJson,
            project.SpecialProgramCategory,
            project.Notes,
            project.CreatedAt);
    }

    private static ProgramProjectTaskResponse MapTask(ProgramProjectTask task) =>
        new(task.Id, task.ProgramProjectId, task.ProgramProject?.Name ?? string.Empty, task.Title, task.OwnerName, task.DueDate, task.Status.ToString(), task.ProgressPercent, task.Notes);

    private static ProgramProjectMilestoneResponse MapMilestone(ProgramProjectMilestone milestone) =>
        new(milestone.Id, milestone.ProgramProjectId, milestone.ProgramProject?.Name ?? string.Empty, milestone.Title, milestone.StartsAt, milestone.EndsAt, milestone.ProgressPercent, milestone.Notes);

    private static ProgramProjectContractResponse MapContract(ProgramProjectContract contract) =>
        new(contract.Id, contract.ProgramProjectId, contract.ProgramProject?.Name ?? string.Empty, contract.ProgramSupplierId, contract.ProgramSupplier?.Name, contract.ContractNumber, contract.Title, contract.Amount, contract.SignedAt, contract.EndsAt, contract.Notes);

    private static ProgramProjectFinanceEntryResponse MapFinanceEntry(ProgramProjectFinanceEntry entry) =>
        new(entry.Id, entry.ProgramProjectId, entry.ProgramProject?.Name ?? string.Empty, entry.EntryType.ToString(), entry.EntryDate, entry.Amount, entry.SourceOrPayee, entry.ReferenceNumber, entry.Notes);

    private static ProgramProjectAssignmentResponse MapAssignment(ProgramProjectAssignment assignment) =>
        new(assignment.Id, assignment.ProgramProjectId, assignment.ProgramProject?.Name ?? string.Empty, assignment.AssignmentType.ToString(), assignment.DisplayName, assignment.ExternalReference, assignment.AssignedAt, assignment.Notes);

    private static ProgramProjectReportResponse MapReport(ProgramProjectReport report) =>
        new(report.Id, report.ProgramProjectId, report.ProgramProject?.Name ?? string.Empty, report.ReportType, report.ReportDate, report.Summary, report.FilePath);

    private static ProgramProjectActivityResponse MapActivity(ProgramProjectActivity activity) =>
        new(
            activity.Id,
            activity.ProgramProjectId,
            activity.ProgramProject?.Name ?? string.Empty,
            activity.Type.ToString(),
            activity.Title,
            activity.Note,
            activity.FromStatus,
            activity.ToStatus,
            activity.Amount,
            activity.Reference,
            activity.OccurredAt);

    private static ProgramSupplierResponse MapSupplier(ProgramSupplier supplier) =>
        new(supplier.Id, supplier.Name, supplier.ContactPerson, supplier.Mobile, supplier.Email, supplier.City, supplier.Status.ToString(), supplier.Notes);

    private static ProgramIdeaResponse MapIdea(ProgramIdea idea) =>
        new(idea.Id, idea.Title, idea.OwnerName, idea.Description, idea.MarketingNotes, idea.EstimatedBudget, idea.Status.ToString(), idea.DecisionNotes, idea.DecidedAt, idea.CreatedAt);

    private static ProgramApprovalResponse MapApproval(ProgramApproval approval) =>
        new(approval.Id, approval.ProgramIdeaId, approval.ProgramIdea?.Title, approval.ApprovalType, approval.Title, approval.Status.ToString(), approval.DecisionNotes, approval.DecidedAt);

    private static ProgramRegistrationResponse MapRegistration(ProgramRegistration registration) =>
        new(registration.Id, registration.ProgramProjectId, registration.ProgramProject?.Name ?? string.Empty, registration.ParticipantName, registration.Mobile, registration.Email, registration.ExternalReference, registration.Status.ToString(), registration.RegisteredAt, registration.DecisionNotes, registration.DecidedAt, registration.Notes);

    private static ProgramSessionResponse MapSession(ProgramSession session) =>
        new(
            session.Id,
            session.ProgramProjectId,
            session.ProgramProject?.Name ?? string.Empty,
            session.Title,
            session.StartsAt,
            session.EndsAt,
            session.Location,
            session.Notes,
            session.AttendanceRecords.Count(x => x.Status == ProgramAttendanceStatus.Present),
            session.AttendanceRecords.Count(x => x.Status == ProgramAttendanceStatus.Absent),
            session.AttendanceRecords.Count(x => x.Status == ProgramAttendanceStatus.Excused));

    private static ProgramSessionAttendanceResponse MapAttendance(ProgramSessionAttendance attendance) =>
        new(attendance.Id, attendance.ProgramSessionId, attendance.ProgramSession?.Title ?? string.Empty, attendance.ProgramSession?.ProgramProject?.Name ?? string.Empty, attendance.ParticipantName, attendance.ExternalReference, attendance.Status.ToString(), attendance.Notes);

    private static ProgramSurveyResponse MapSurvey(ProgramSurvey survey) =>
        new(survey.Id, survey.ProgramProjectId, survey.ProgramProject?.Name ?? string.Empty, survey.Title, survey.QuestionsJson, survey.Status.ToString(), survey.Notes, survey.Submissions.Count);

    private static ProgramSurveySubmissionResponse MapSurveySubmission(ProgramSurveySubmission submission) =>
        new(submission.Id, submission.ProgramSurveyId, submission.ProgramSurvey?.Title ?? string.Empty, submission.RespondentName, submission.AnswersJson, submission.SubmittedAt);

    private static ProgramCertificateTemplateResponse MapCertificateTemplate(ProgramCertificateTemplate template) =>
        new(template.Id, template.ProgramProjectId, template.ProgramProject?.Name, template.Name, template.BodyTemplate, template.IsActive);

    private static ProgramCertificateIssueResponse MapCertificateIssue(ProgramCertificateIssue issue) =>
        new(issue.Id, issue.ProgramProjectId, issue.ProgramProject?.Name ?? string.Empty, issue.ProgramCertificateTemplateId, issue.ProgramCertificateTemplate?.Name, issue.CertificateNumber, issue.RecipientName, issue.IssuedAt, issue.Status.ToString(), issue.Notes);

    private static ProgramQualificationCaseResponse MapQualificationCase(ProgramQualificationCase qualificationCase)
    {
        var paidAmount = qualificationCase.Installments.Sum(x => x.PaidAmount);
        return new(
            qualificationCase.Id,
            qualificationCase.ProgramProjectId,
            qualificationCase.ProgramProject?.Name,
            qualificationCase.BeneficiaryName,
            qualificationCase.NeedSummary,
            qualificationCase.ManagementOpinion,
            qualificationCase.Status.ToString(),
            qualificationCase.ApprovedAmount,
            qualificationCase.InstallmentCount,
            paidAmount,
            qualificationCase.ApprovedAmount - paidAmount,
            qualificationCase.Notes);
    }

    private static ProgramQualificationInstallmentResponse MapQualificationInstallment(ProgramQualificationInstallment installment) =>
        new(installment.Id, installment.ProgramQualificationCaseId, installment.ProgramQualificationCase?.BeneficiaryName ?? string.Empty, installment.DueDate, installment.Amount, installment.PaidAmount, installment.PaidAt, installment.Status.ToString(), installment.Notes);
}
