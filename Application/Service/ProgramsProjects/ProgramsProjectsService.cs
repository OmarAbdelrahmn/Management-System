using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ProgramsProjects;
using Application.Service.TaskManagement;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Service.ProgramsProjects;

public class ProgramsProjectsService(ApplicationDbcontext dbcontext, ITaskManagementService? approvalWorkflow = null) : IProgramsProjectsService
{
    public async Task<Result<ProgramsProjectsDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.AddHours(3).Date;
        var nextMonth = today.AddDays(30);
        var recentActivitySince = today.AddDays(-30);

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
        var overdueTasksCount = await dbcontext.ProgramProjectTasks.CountAsync(
            x => x.DueDate.HasValue &&
                x.DueDate.Value.Date < today &&
                x.Status != ProgramProjectTaskStatus.Completed &&
                x.Status != ProgramProjectTaskStatus.Finished,
            cancellationToken);
        var pendingRegistrationsCount = await dbcontext.ProgramRegistrations.CountAsync(x => x.Status == ProgramRegistrationStatus.Pending, cancellationToken);
        var attendedRegistrationsCount = await dbcontext.ProgramRegistrations.CountAsync(x => x.Status == ProgramRegistrationStatus.Attended, cancellationToken);
        var upcomingSessionsCount = await dbcontext.ProgramSessions.CountAsync(x => x.StartsAt.Date >= today && x.StartsAt.Date <= nextMonth, cancellationToken);
        var activeSurveysCount = await dbcontext.ProgramSurveys.CountAsync(x => x.Status == ProgramSurveyStatus.Active, cancellationToken);
        var issuedCertificatesCount = await dbcontext.ProgramCertificateIssues.CountAsync(x => x.Status == ProgramCertificateIssueStatus.Issued, cancellationToken);
        var pendingSupplierProposalsCount = await dbcontext.ProgramSupplierProposals.CountAsync(x => x.Status == ProgramSupplierProposalStatus.Submitted, cancellationToken);
        var openQualificationCasesCount = await dbcontext.ProgramQualificationCases.CountAsync(
            x => x.Status != ProgramQualificationCaseStatus.Completed &&
                x.Status != ProgramQualificationCaseStatus.Cancelled,
            cancellationToken);
        var recentActivityCount = await dbcontext.ProgramProjectActivities.CountAsync(x => x.OccurredAt.Date >= recentActivitySince, cancellationToken);

        return Result.Success(new ProgramsProjectsDashboardResponse(
            projectsCount,
            activeProjectsCount,
            completedProjectsCount,
            suppliersCount,
            pendingIdeasCount,
            pendingApprovalsCount,
            totalBudget,
            totalIncome,
            totalExpenses,
            overdueTasksCount,
            pendingRegistrationsCount,
            attendedRegistrationsCount,
            upcomingSessionsCount,
            activeSurveysCount,
            issuedCertificatesCount,
            pendingSupplierProposalsCount,
            openQualificationCasesCount,
            recentActivityCount));
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

        var notes = request.Notes?.Trim();
        var transitionError = ValidateProjectStatusTransition(project, request.Status, notes);
        if (transitionError is not null)
            return Result.Failure<ProgramProjectResponse>(transitionError);

        var fromStatus = project.Status.ToString();
        project.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(notes))
            project.Notes = notes;

        QueueProjectActivity(project.Id, ProgramProjectActivityType.StatusChanged, "تغيير حالة المشروع/البرنامج", notes, fromStatus, project.Status.ToString());
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
        if (request.Status is ProgramProjectTaskStatus.Completed or ProgramProjectTaskStatus.Finished && request.ProgressPercent != 100)
            return Result.Failure<ProgramProjectTaskResponse>(ProgramsProjectsErrors.TaskCompletionProgressRequired);

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

    public async Task<Result<ProgramContractPaymentResponse>> RecordContractPaymentAsync(int contractId, RecordProgramContractPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            return Result.Failure<ProgramContractPaymentResponse>(ProgramsProjectsErrors.InvalidRequest);

        var contract = await dbcontext.ProgramProjectContracts
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramSupplier)
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken);

        if (contract is null)
            return Result.Failure<ProgramContractPaymentResponse>(ProgramsProjectsErrors.ContractNotFound);

        var paidAmount = await dbcontext.ProgramProjectFinanceEntries
            .Where(x =>
                x.ProgramProjectId == contract.ProgramProjectId &&
                x.ReferenceNumber == contract.ContractNumber &&
                (x.EntryType == ProgramProjectFinanceEntryType.Expense || x.EntryType == ProgramProjectFinanceEntryType.Custody))
            .SumAsync(x => x.Amount, cancellationToken);

        var remainingAmount = contract.Amount - paidAmount;
        if (remainingAmount <= 0)
            return Result.Failure<ProgramContractPaymentResponse>(ProgramsProjectsErrors.ContractAlreadyPaid);

        if (request.Amount > remainingAmount)
            return Result.Failure<ProgramContractPaymentResponse>(ProgramsProjectsErrors.ContractPaymentExceedsBalance);

        var paymentReference = request.PaymentReference?.Trim();
        var notes = request.Notes?.Trim();
        if (!string.IsNullOrWhiteSpace(paymentReference))
            notes = string.IsNullOrWhiteSpace(notes)
                ? $"مرجع السداد: {paymentReference}"
                : $"{notes} | مرجع السداد: {paymentReference}";

        var entry = new ProgramProjectFinanceEntry
        {
            ProgramProjectId = contract.ProgramProjectId,
            ProgramProject = contract.ProgramProject,
            EntryType = ProgramProjectFinanceEntryType.Expense,
            EntryDate = request.PaidAt ?? DateTime.UtcNow.AddHours(3),
            Amount = request.Amount,
            SourceOrPayee = contract.ProgramSupplier?.Name ?? contract.Title,
            ReferenceNumber = contract.ContractNumber,
            Notes = notes ?? $"سداد عقد {contract.ContractNumber}"
        };

        dbcontext.ProgramProjectFinanceEntries.Add(entry);
        QueueProjectActivity(
            entry.ProgramProjectId,
            ProgramProjectActivityType.FinanceEntryAdded,
            $"سداد عقد: {contract.Title}",
            entry.Notes,
            null,
            null,
            entry.Amount,
            contract.ContractNumber);

        await dbcontext.SaveChangesAsync(cancellationToken);

        var newPaidAmount = paidAmount + entry.Amount;
        return Result.Success(new ProgramContractPaymentResponse(
            MapContract(contract),
            MapFinanceEntry(entry),
            newPaidAmount,
            contract.Amount - newPaidAmount));
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

    public async Task<Result<ProgramProjectFinanceEntryResponse>> UpdateFinanceEntryAsync(int id, AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await dbcontext.ProgramProjectFinanceEntries
            .Include(x => x.ProgramProject)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entry is null)
            return Result.Failure<ProgramProjectFinanceEntryResponse>(ProgramsProjectsErrors.FinanceEntryNotFound);

        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectFinanceEntryResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.SourceOrPayee))
            return Result.Failure<ProgramProjectFinanceEntryResponse>(ProgramsProjectsErrors.InvalidRequest);

        var oldType = entry.EntryType.ToString();
        var oldAmount = entry.Amount;
        entry.ProgramProjectId = request.ProgramProjectId;
        entry.ProgramProject = project;
        entry.EntryType = request.EntryType;
        entry.EntryDate = request.EntryDate ?? DateTime.UtcNow.AddHours(3);
        entry.Amount = request.Amount;
        entry.SourceOrPayee = request.SourceOrPayee.Trim();
        entry.ReferenceNumber = request.ReferenceNumber?.Trim();
        entry.Notes = request.Notes?.Trim();

        QueueProjectActivity(
            entry.ProgramProjectId,
            ProgramProjectActivityType.FinanceEntryAdded,
            $"تعديل حركة مالية: {entry.EntryType}",
            entry.Notes,
            oldType,
            entry.EntryType.ToString(),
            entry.Amount - oldAmount,
            entry.ReferenceNumber ?? entry.SourceOrPayee);

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

    public async Task<Result<ProgramProjectAssignmentResponse>> UpdateAssignmentAsync(int id, AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await dbcontext.ProgramProjectAssignments
            .Include(x => x.ProgramProject)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (assignment is null)
            return Result.Failure<ProgramProjectAssignmentResponse>(ProgramsProjectsErrors.AssignmentNotFound);

        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectAssignmentResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return Result.Failure<ProgramProjectAssignmentResponse>(ProgramsProjectsErrors.InvalidRequest);

        var oldType = assignment.AssignmentType.ToString();
        assignment.ProgramProjectId = request.ProgramProjectId;
        assignment.ProgramProject = project;
        assignment.AssignmentType = request.AssignmentType;
        assignment.DisplayName = request.DisplayName.Trim();
        assignment.ExternalReference = request.ExternalReference?.Trim();
        assignment.AssignedAt = request.AssignedAt ?? DateTime.UtcNow.AddHours(3);
        assignment.Notes = request.Notes?.Trim();

        QueueProjectActivity(
            assignment.ProgramProjectId,
            ProgramProjectActivityType.AssignmentAdded,
            $"تعديل إلحاق {assignment.AssignmentType}: {assignment.DisplayName}",
            assignment.Notes,
            oldType,
            assignment.AssignmentType.ToString(),
            null,
            assignment.ExternalReference);

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

    public async Task<Result<ProgramProjectReportResponse>> UpdateReportAsync(int id, AddProgramProjectReportRequest request, CancellationToken cancellationToken = default)
    {
        var report = await dbcontext.ProgramProjectReports
            .Include(x => x.ProgramProject)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (report is null)
            return Result.Failure<ProgramProjectReportResponse>(ProgramsProjectsErrors.ReportNotFound);

        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectReportResponse>(ProgramsProjectsErrors.ProjectNotFound);

        if (string.IsNullOrWhiteSpace(request.ReportType) || string.IsNullOrWhiteSpace(request.Summary))
            return Result.Failure<ProgramProjectReportResponse>(ProgramsProjectsErrors.InvalidRequest);

        var oldReportType = report.ReportType;
        report.ProgramProjectId = request.ProgramProjectId;
        report.ProgramProject = project;
        report.ReportType = request.ReportType.Trim();
        report.ReportDate = request.ReportDate ?? DateTime.UtcNow.AddHours(3);
        report.Summary = request.Summary.Trim();
        report.FilePath = request.FilePath?.Trim();

        QueueProjectActivity(
            report.ProgramProjectId,
            ProgramProjectActivityType.ReportAdded,
            $"تعديل تقرير: {report.ReportType}",
            report.Summary,
            oldReportType,
            report.ReportType,
            null,
            report.FilePath);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReport(report));
    }

    public async Task<Result<ProgramProjectResponse>> CloseProjectAsync(int id, CloseProgramProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await ProjectQuery(false).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectNotFound);
        if (project.Status is ProgramProjectStatus.Completed or ProgramProjectStatus.Cancelled or ProgramProjectStatus.Deleted)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.ProjectAlreadyClosed);
        if (string.IsNullOrWhiteSpace(request.Summary))
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        var closedAt = request.ClosedAt ?? DateTime.UtcNow.AddHours(3);
        if (project.StartsAt.HasValue && closedAt.Date < project.StartsAt.Value.Date)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        var fromStatus = project.Status.ToString();
        project.Status = ProgramProjectStatus.Completed;
        project.EndsAt = closedAt.Date;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            project.Notes = request.Notes.Trim();

        var report = new ProgramProjectReport
        {
            ProgramProjectId = project.Id,
            ProgramProject = project,
            ReportType = "Final",
            ReportDate = closedAt,
            Summary = request.Summary.Trim(),
            FilePath = request.FilePath?.Trim()
        };

        dbcontext.ProgramProjectReports.Add(report);
        QueueProjectActivity(project.Id, ProgramProjectActivityType.ReportAdded, "تقرير ختامي", report.Summary, null, null, null, report.FilePath);
        QueueProjectActivity(project.Id, ProgramProjectActivityType.StatusChanged, "إغلاق المشروع/البرنامج", project.Notes ?? report.Summary, fromStatus, project.Status.ToString(), null, project.ProjectCode);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadProjectCollectionsAsync(project, cancellationToken);
        return Result.Success(MapProject(project));
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

    public async Task<Result<IEnumerable<ProgramSupplierProposalResponse>>> GetSupplierProposalsAsync(int? projectId, int? supplierId, ProgramSupplierProposalStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramSupplierProposals
            .AsNoTracking()
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramSupplier)
            .Include(x => x.ConvertedContract)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(x => x.ProgramProjectId == projectId.Value);

        if (supplierId.HasValue)
            query = query.Where(x => x.ProgramSupplierId == supplierId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var proposals = await query
            .OrderByDescending(x => x.SubmittedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => MapSupplierProposal(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProgramSupplierProposalResponse>>(proposals);
    }

    public async Task<Result<ProgramSupplierProposalResponse>> SaveSupplierProposalAsync(int? id, SaveProgramSupplierProposalRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Amount < 0)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.InvalidRequest);

        var project = await dbcontext.ProgramProjects.FirstOrDefaultAsync(x => x.Id == request.ProgramProjectId, cancellationToken);
        if (project is null)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.ProjectNotFound);

        var supplier = await dbcontext.ProgramSuppliers.FirstOrDefaultAsync(x => x.Id == request.ProgramSupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.SupplierNotFound);

        var proposalNumber = string.IsNullOrWhiteSpace(request.ProposalNumber)
            ? await GenerateProposalNumberAsync(cancellationToken)
            : request.ProposalNumber.Trim();

        var duplicateNumber = await dbcontext.ProgramSupplierProposals.AnyAsync(x => x.ProposalNumber == proposalNumber && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicateNumber)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.DuplicateProposalNumber);

        var isNew = !id.HasValue;
        ProgramSupplierProposal proposal;
        string? fromStatus = null;
        if (id.HasValue)
        {
            proposal = await dbcontext.ProgramSupplierProposals
                .Include(x => x.ProgramProject)
                .Include(x => x.ProgramSupplier)
                .Include(x => x.ConvertedContract)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (proposal is null)
                return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.SupplierProposalNotFound);

            fromStatus = proposal.Status.ToString();
        }
        else
        {
            proposal = new ProgramSupplierProposal();
            dbcontext.ProgramSupplierProposals.Add(proposal);
        }

        proposal.ProgramProjectId = request.ProgramProjectId;
        proposal.ProgramSupplierId = request.ProgramSupplierId;
        proposal.ProgramProject = project;
        proposal.ProgramSupplier = supplier;
        proposal.ProposalNumber = proposalNumber;
        proposal.Title = request.Title.Trim();
        proposal.Scope = request.Scope?.Trim();
        proposal.Amount = request.Amount;
        proposal.SubmittedAt = request.SubmittedAt ?? DateTime.UtcNow.AddHours(3);
        proposal.ValidUntil = request.ValidUntil?.Date;
        proposal.Status = request.Status;
        proposal.Notes = request.Notes?.Trim();

        QueueProjectActivity(
            proposal.ProgramProjectId,
            ProgramProjectActivityType.SupplierProposalSaved,
            isNew ? $"عرض مورد: {proposal.Title}" : $"تحديث عرض مورد: {proposal.Title}",
            proposal.Notes ?? proposal.Scope,
            fromStatus,
            proposal.Status.ToString(),
            proposal.Amount,
            proposal.ProposalNumber);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSupplierProposal(proposal));
    }

    public async Task<Result<ProgramSupplierProposalResponse>> DecideSupplierProposalAsync(int id, DecideProgramSupplierProposalRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Status == ProgramSupplierProposalStatus.Converted)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.InvalidRequest);

        var proposal = await dbcontext.ProgramSupplierProposals
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramSupplier)
            .Include(x => x.ConvertedContract)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (proposal is null)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.SupplierProposalNotFound);

        var decisionNotes = request.DecisionNotes?.Trim();
        var decisionError = ValidateSupplierProposalDecision(proposal, request.Status, decisionNotes);
        if (decisionError is not null)
            return Result.Failure<ProgramSupplierProposalResponse>(decisionError);

        var fromStatus = proposal.Status.ToString();
        proposal.Status = request.Status;
        proposal.DecisionNotes = decisionNotes ?? proposal.DecisionNotes;
        proposal.DecidedAt = DateTime.UtcNow.AddHours(3);

        QueueProjectActivity(
            proposal.ProgramProjectId,
            ProgramProjectActivityType.SupplierProposalDecided,
            $"قرار عرض مورد: {proposal.Title}",
            proposal.DecisionNotes,
            fromStatus,
            proposal.Status.ToString(),
            proposal.Amount,
            proposal.ProposalNumber);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSupplierProposal(proposal));
    }

    public async Task<Result<ProgramSupplierProposalResponse>> ConvertSupplierProposalToContractAsync(int id, ConvertProgramSupplierProposalRequest request, CancellationToken cancellationToken = default)
    {
        var proposal = await dbcontext.ProgramSupplierProposals
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramSupplier)
            .Include(x => x.ConvertedContract)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (proposal is null)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.SupplierProposalNotFound);

        if (proposal.Status != ProgramSupplierProposalStatus.Approved || proposal.ConvertedContractId.HasValue)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.InvalidRequest);

        var contractNumber = string.IsNullOrWhiteSpace(request.ContractNumber)
            ? await GenerateContractNumberAsync(cancellationToken)
            : request.ContractNumber.Trim();

        var duplicateContract = await dbcontext.ProgramProjectContracts.AnyAsync(x => x.ContractNumber == contractNumber, cancellationToken);
        if (duplicateContract)
            return Result.Failure<ProgramSupplierProposalResponse>(ProgramsProjectsErrors.DuplicateContractNumber);

        var contract = new ProgramProjectContract
        {
            ProgramProjectId = proposal.ProgramProjectId,
            ProgramProject = proposal.ProgramProject,
            ProgramSupplierId = proposal.ProgramSupplierId,
            ProgramSupplier = proposal.ProgramSupplier,
            ContractNumber = contractNumber,
            Title = proposal.Title,
            Amount = proposal.Amount,
            SignedAt = (request.SignedAt ?? DateTime.UtcNow.AddHours(3)).Date,
            EndsAt = request.EndsAt?.Date,
            Notes = request.Notes?.Trim() ?? proposal.Notes
        };

        dbcontext.ProgramProjectContracts.Add(contract);
        var fromStatus = proposal.Status.ToString();
        proposal.Status = ProgramSupplierProposalStatus.Converted;
        proposal.DecisionNotes = request.Notes?.Trim() ?? proposal.DecisionNotes;
        proposal.DecidedAt = DateTime.UtcNow.AddHours(3);
        proposal.ConvertedContract = contract;

        QueueProjectActivity(
            proposal.ProgramProjectId,
            ProgramProjectActivityType.SupplierProposalConverted,
            $"تحويل عرض إلى عقد: {proposal.Title}",
            proposal.DecisionNotes,
            fromStatus,
            proposal.Status.ToString(),
            proposal.Amount,
            proposal.ProposalNumber);
        QueueProjectActivity(proposal.ProgramProjectId, ProgramProjectActivityType.ContractSaved, $"عقد من عرض مورد: {contract.Title}", contract.Notes, null, null, contract.Amount, contract.ContractNumber);

        await dbcontext.SaveChangesAsync(cancellationToken);
        proposal.ConvertedContractId = contract.Id;
        proposal.ConvertedContract = contract;
        return Result.Success(MapSupplierProposal(proposal));
    }

    private static Error? ValidateSupplierProposalDecision(ProgramSupplierProposal proposal, ProgramSupplierProposalStatus status, string? notes)
    {
        if (proposal.Status is ProgramSupplierProposalStatus.Converted or ProgramSupplierProposalStatus.Cancelled or ProgramSupplierProposalStatus.Rejected ||
            proposal.ConvertedContractId.HasValue)
            return proposal.Status == status && status != ProgramSupplierProposalStatus.Converted
                ? null
                : ProgramsProjectsErrors.SupplierProposalAlreadyClosed;

        if (proposal.Status == status)
            return null;

        if (status is ProgramSupplierProposalStatus.Rejected or ProgramSupplierProposalStatus.Cancelled && string.IsNullOrWhiteSpace(notes))
            return ProgramsProjectsErrors.SupplierProposalDecisionNotesRequired;

        if (status == ProgramSupplierProposalStatus.Approved)
        {
            if (proposal.Status is not (ProgramSupplierProposalStatus.Submitted or ProgramSupplierProposalStatus.UnderReview))
                return ProgramsProjectsErrors.InvalidSupplierProposalStatusTransition;

            if (proposal.ValidUntil.HasValue && proposal.ValidUntil.Value.Date < DateTime.UtcNow.AddHours(3).Date)
                return ProgramsProjectsErrors.SupplierProposalExpired;

            return null;
        }

        return status switch
        {
            ProgramSupplierProposalStatus.Submitted when proposal.Status == ProgramSupplierProposalStatus.Draft => null,
            ProgramSupplierProposalStatus.UnderReview when proposal.Status is ProgramSupplierProposalStatus.Draft or ProgramSupplierProposalStatus.Submitted => null,
            ProgramSupplierProposalStatus.Rejected when proposal.Status is ProgramSupplierProposalStatus.Draft or ProgramSupplierProposalStatus.Submitted or ProgramSupplierProposalStatus.UnderReview => null,
            ProgramSupplierProposalStatus.Cancelled when proposal.Status is ProgramSupplierProposalStatus.Draft or ProgramSupplierProposalStatus.Submitted or ProgramSupplierProposalStatus.UnderReview or ProgramSupplierProposalStatus.Approved => null,
            _ => ProgramsProjectsErrors.InvalidSupplierProposalStatusTransition
        };
    }

    public async Task<Result<IEnumerable<ProgramIdeaResponse>>> GetIdeasAsync(ProgramIdeaStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ProgramIdeas.AsNoTracking().Include(x => x.ConvertedProject).AsQueryable();
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

    public async Task<Result<ProgramProjectResponse>> ConvertIdeaToProjectAsync(int id, ConvertProgramIdeaToProjectRequest request, CancellationToken cancellationToken = default)
    {
        var idea = await dbcontext.ProgramIdeas
            .Include(x => x.ConvertedProject)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (idea is null)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.IdeaNotFound);
        if (idea.ConvertedProjectId.HasValue)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.IdeaAlreadyConverted);
        if (idea.Status != ProgramIdeaStatus.Approved)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.IdeaNotApproved);
        if (string.IsNullOrWhiteSpace(request.ProjectType) || request.TargetBeneficiaries < 0)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);
        if (request.StartsAt.HasValue && request.EndsAt.HasValue && request.EndsAt.Value.Date < request.StartsAt.Value.Date)
            return Result.Failure<ProgramProjectResponse>(ProgramsProjectsErrors.InvalidRequest);

        var project = new ProgramProject
        {
            ProjectCode = await GenerateProjectCodeAsync(cancellationToken),
            Name = idea.Title.Trim(),
            ProjectType = request.ProjectType.Trim(),
            Description = idea.Description.Trim(),
            ManagerName = request.ManagerName?.Trim() ?? idea.OwnerName?.Trim(),
            StartsAt = request.StartsAt?.Date,
            EndsAt = request.EndsAt?.Date,
            Status = request.Status,
            Budget = idea.EstimatedBudget,
            TargetBeneficiaries = request.TargetBeneficiaries,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? idea.MarketingNotes?.Trim() : request.Notes.Trim()
        };

        dbcontext.ProgramProjects.Add(project);
        idea.Status = ProgramIdeaStatus.Completed;
        idea.DecisionNotes = string.IsNullOrWhiteSpace(request.Notes)
            ? $"تم تحويل المقترح إلى {request.ProjectType.Trim()}."
            : request.Notes.Trim();
        idea.DecidedAt = DateTime.UtcNow.AddHours(3);
        idea.ConvertedProject = project;

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueProjectActivity(
            project.Id,
            ProgramProjectActivityType.Created,
            "تحويل مقترح برنامج إلى مشروع/برنامج",
            idea.DecisionNotes,
            ProgramIdeaStatus.Approved.ToString(),
            idea.Status.ToString(),
            idea.EstimatedBudget,
            $"IDEA-{idea.Id}");
        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadProjectCollectionsAsync(project, cancellationToken);
        return Result.Success(MapProject(project));
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
        if (!id.HasValue && approvalWorkflow is not null)
            await approvalWorkflow.EnsureApprovalRequestForEntityAsync(
                nameof(ProgramApproval), approval.Id, approval.Title, cancellationToken: cancellationToken);
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

        var decisionNotes = request.DecisionNotes?.Trim();
        var decisionError = ValidateRegistrationDecision(registration, request.Status, decisionNotes);
        if (decisionError is not null)
            return Result.Failure<ProgramRegistrationResponse>(decisionError);

        if (request.Status == ProgramRegistrationStatus.Approved
            && !await HasRegistrationCapacityAsync(registration.ProgramProjectId, registration.Id, cancellationToken))
            return Result.Failure<ProgramRegistrationResponse>(ProgramsProjectsErrors.ProgramCapacityReached);

        var fromStatus = registration.Status.ToString();
        registration.Status = request.Status;
        registration.DecisionNotes = decisionNotes;
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

        await MarkMatchingRegistrationAttendedAsync(session.ProgramProjectId, attendance, cancellationToken);
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
        if (!IsValidJson(request.QuestionsJson, JsonValueKind.Array))
            return Result.Failure<ProgramSurveyResponse>(ProgramsProjectsErrors.InvalidSurveyJson);

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
        var survey = await dbcontext.ProgramSurveys.Include(x => x.ProgramProject).FirstOrDefaultAsync(x => x.Id == request.ProgramSurveyId, cancellationToken);
        if (survey is null)
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.SurveyNotFound);
        if (string.IsNullOrWhiteSpace(request.RespondentName) || string.IsNullOrWhiteSpace(request.AnswersJson))
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.InvalidRequest);
        if (survey.Status != ProgramSurveyStatus.Active)
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.SurveyNotActive);
        if (!IsValidJson(request.AnswersJson, JsonValueKind.Object))
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.InvalidSurveyJson);

        var respondentName = request.RespondentName.Trim();
        var duplicate = await dbcontext.ProgramSurveySubmissions.AnyAsync(
            x => x.ProgramSurveyId == request.ProgramSurveyId && x.RespondentName == respondentName,
            cancellationToken);
        if (duplicate)
            return Result.Failure<ProgramSurveySubmissionResponse>(ProgramsProjectsErrors.DuplicateSurveySubmission);

        var submission = new ProgramSurveySubmission
        {
            ProgramSurveyId = request.ProgramSurveyId,
            ProgramSurvey = survey,
            RespondentName = respondentName,
            AnswersJson = request.AnswersJson.Trim(),
            SubmittedAt = request.SubmittedAt ?? DateTime.UtcNow.AddHours(3)
        };

        dbcontext.ProgramSurveySubmissions.Add(submission);
        QueueProjectActivity(survey.ProgramProjectId, ProgramProjectActivityType.SurveySaved, $"إجابة استبيان: {survey.Title}", submission.RespondentName, null, survey.Status.ToString(), null, survey.Title);
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
        if (string.IsNullOrWhiteSpace(request.RecipientName))
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.InvalidRequest);

        return await CreateCertificateIssueAsync(
            project,
            request.ProgramCertificateTemplateId,
            request.CertificateNumber,
            request.RecipientName,
            request.IssuedAt,
            request.Notes,
            cancellationToken);
    }

    public async Task<Result<ProgramCertificateIssueResponse>> IssueCertificateFromRegistrationAsync(int registrationId, IssueProgramCertificateFromRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var registration = await dbcontext.ProgramRegistrations
            .Include(x => x.ProgramProject)
            .FirstOrDefaultAsync(x => x.Id == registrationId, cancellationToken);
        if (registration is null || registration.ProgramProject is null)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.RegistrationNotFound);
        if (registration.Status != ProgramRegistrationStatus.Attended)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.CertificateRequiresAttendedRegistration);

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? $"إصدار من تسجيل البرنامج رقم {registration.Id}"
            : request.Notes.Trim();

        return await CreateCertificateIssueAsync(
            registration.ProgramProject,
            request.ProgramCertificateTemplateId,
            request.CertificateNumber,
            registration.ParticipantName,
            request.IssuedAt,
            notes,
            cancellationToken);
    }

    public async Task<Result<ProgramCertificateIssueResponse>> CancelCertificateIssueAsync(int id, CancelProgramCertificateIssueRequest request, CancellationToken cancellationToken = default)
    {
        var issue = await dbcontext.ProgramCertificateIssues
            .Include(x => x.ProgramProject)
            .Include(x => x.ProgramCertificateTemplate)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (issue is null)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.CertificateIssueNotFound);
        if (issue.Status == ProgramCertificateIssueStatus.Cancelled)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.CertificateIssueAlreadyCancelled);

        var oldStatus = issue.Status.ToString();
        var notes = request.Notes?.Trim();
        issue.Status = ProgramCertificateIssueStatus.Cancelled;
        issue.Notes = string.IsNullOrWhiteSpace(notes)
            ? issue.Notes
            : string.IsNullOrWhiteSpace(issue.Notes)
                ? notes
                : $"{issue.Notes.Trim()} | {notes}";

        QueueProjectActivity(
            issue.ProgramProjectId,
            ProgramProjectActivityType.CertificateCancelled,
            $"إلغاء شهادة: {issue.RecipientName}",
            issue.Notes,
            oldStatus,
            issue.Status.ToString(),
            null,
            issue.CertificateNumber);

        await dbcontext.SaveChangesAsync(cancellationToken);
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

    public async Task<Result<IEnumerable<ProgramQualificationInstallmentResponse>>> GenerateQualificationInstallmentsAsync(int caseId, GenerateQualificationInstallmentsRequest request, CancellationToken cancellationToken = default)
    {
        var qualificationCase = await dbcontext.ProgramQualificationCases
            .Include(x => x.Installments)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken);
        if (qualificationCase is null)
            return Result.Failure<IEnumerable<ProgramQualificationInstallmentResponse>>(ProgramsProjectsErrors.QualificationCaseNotFound);
        if (qualificationCase.Status != ProgramQualificationCaseStatus.Approved)
            return Result.Failure<IEnumerable<ProgramQualificationInstallmentResponse>>(ProgramsProjectsErrors.QualificationCaseNotApproved);
        if (qualificationCase.Installments.Count != 0)
            return Result.Failure<IEnumerable<ProgramQualificationInstallmentResponse>>(ProgramsProjectsErrors.QualificationInstallmentsAlreadyGenerated);
        if (qualificationCase.ApprovedAmount <= 0 || qualificationCase.InstallmentCount <= 0 || request.MonthsBetweenInstallments <= 0)
            return Result.Failure<IEnumerable<ProgramQualificationInstallmentResponse>>(ProgramsProjectsErrors.InvalidRequest);

        var baseAmount = Math.Round(qualificationCase.ApprovedAmount / qualificationCase.InstallmentCount, 2);
        var generated = new List<ProgramQualificationInstallment>();
        decimal scheduledTotal = 0;

        for (var index = 0; index < qualificationCase.InstallmentCount; index++)
        {
            var amount = index == qualificationCase.InstallmentCount - 1
                ? qualificationCase.ApprovedAmount - scheduledTotal
                : baseAmount;
            scheduledTotal += amount;

            generated.Add(new ProgramQualificationInstallment
            {
                ProgramQualificationCaseId = qualificationCase.Id,
                ProgramQualificationCase = qualificationCase,
                DueDate = request.FirstDueDate.Date.AddMonths(index * request.MonthsBetweenInstallments),
                Amount = amount,
                Status = ProgramQualificationInstallmentStatus.Pending,
                Notes = request.Notes?.Trim()
            });
        }

        dbcontext.ProgramQualificationInstallments.AddRange(generated);
        var fromStatus = qualificationCase.Status.ToString();
        qualificationCase.Status = ProgramQualificationCaseStatus.Active;

        if (qualificationCase.ProgramProjectId.HasValue)
        {
            QueueProjectActivity(
                qualificationCase.ProgramProjectId.Value,
                ProgramProjectActivityType.FinanceEntryAdded,
                $"جدولة أقساط تأهيل: {qualificationCase.BeneficiaryName}",
                request.Notes,
                fromStatus,
                qualificationCase.Status.ToString(),
                qualificationCase.ApprovedAmount,
                $"{request.FirstDueDate:yyyy-MM-dd}");
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success<IEnumerable<ProgramQualificationInstallmentResponse>>(generated.Select(MapQualificationInstallment).ToList());
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

    private static Error? ValidateProjectStatusTransition(ProgramProject project, ProgramProjectStatus status, string? notes)
    {
        if (project.Status == status)
            return null;

        if (project.Status is ProgramProjectStatus.Completed or ProgramProjectStatus.Cancelled or ProgramProjectStatus.Deleted)
            return ProgramsProjectsErrors.ProjectAlreadyClosed;

        if (status == ProgramProjectStatus.Completed)
            return ProgramsProjectsErrors.ProjectFinalReportRequired;

        if (status is ProgramProjectStatus.Cancelled or ProgramProjectStatus.Deleted && string.IsNullOrWhiteSpace(notes))
            return ProgramsProjectsErrors.ProjectStatusNotesRequired;

        return status switch
        {
            ProgramProjectStatus.Planning when project.Status is ProgramProjectStatus.Draft or ProgramProjectStatus.OnHold => null,
            ProgramProjectStatus.Active when project.Status is ProgramProjectStatus.Draft or ProgramProjectStatus.Planning or ProgramProjectStatus.OnHold => null,
            ProgramProjectStatus.OnHold when project.Status is ProgramProjectStatus.Planning or ProgramProjectStatus.Active => null,
            ProgramProjectStatus.Cancelled or ProgramProjectStatus.Deleted => null,
            _ => ProgramsProjectsErrors.InvalidProjectStatusTransition
        };
    }

    private static Error? ValidateRegistrationDecision(ProgramRegistration registration, ProgramRegistrationStatus status, string? notes)
    {
        if (registration.Status != ProgramRegistrationStatus.Pending)
            return ProgramsProjectsErrors.RegistrationAlreadyDecided;

        if (status is ProgramRegistrationStatus.Pending or ProgramRegistrationStatus.Attended)
            return ProgramsProjectsErrors.InvalidRegistrationStatusTransition;

        if (status is ProgramRegistrationStatus.Rejected or ProgramRegistrationStatus.Cancelled && string.IsNullOrWhiteSpace(notes))
            return ProgramsProjectsErrors.RegistrationDecisionNotesRequired;

        return status == ProgramRegistrationStatus.Approved ||
               status == ProgramRegistrationStatus.Rejected ||
               status == ProgramRegistrationStatus.Cancelled
            ? null
            : ProgramsProjectsErrors.InvalidRegistrationStatusTransition;
    }

    private static bool IsValidJson(string json, JsonValueKind expectedKind)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == expectedKind;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task MarkMatchingRegistrationAttendedAsync(int projectId, ProgramSessionAttendance attendance, CancellationToken cancellationToken)
    {
        if (attendance.Status != ProgramAttendanceStatus.Present)
            return;

        var participantName = attendance.ParticipantName.Trim();
        var externalReference = attendance.ExternalReference?.Trim();
        ProgramRegistration? registration = null;

        if (!string.IsNullOrWhiteSpace(externalReference))
        {
            registration = await dbcontext.ProgramRegistrations.FirstOrDefaultAsync(
                x => x.ProgramProjectId == projectId && x.ExternalReference == externalReference,
                cancellationToken);
        }

        registration ??= await dbcontext.ProgramRegistrations.FirstOrDefaultAsync(
            x => x.ProgramProjectId == projectId && x.ParticipantName == participantName,
            cancellationToken);

        if (registration is null || registration.Status != ProgramRegistrationStatus.Approved)
            return;

        var fromStatus = registration.Status.ToString();
        registration.Status = ProgramRegistrationStatus.Attended;
        registration.DecisionNotes = string.IsNullOrWhiteSpace(attendance.Notes)
            ? "تحديث تلقائي من سجل حضور البرنامج."
            : attendance.Notes.Trim();
        registration.DecidedAt = DateTime.UtcNow.AddHours(3);

        QueueProjectActivity(
            registration.ProgramProjectId,
            ProgramProjectActivityType.RegistrationDecided,
            $"تحديث حضور تسجيل: {registration.ParticipantName}",
            registration.DecisionNotes,
            fromStatus,
            registration.Status.ToString(),
            null,
            registration.ExternalReference);
    }

    private async Task<bool> HasRegistrationCapacityAsync(int projectId, int registrationId, CancellationToken cancellationToken)
    {
        var target = await dbcontext.ProgramProjects
            .Where(x => x.Id == projectId)
            .Select(x => x.TargetBeneficiaries)
            .FirstOrDefaultAsync(cancellationToken);
        if (target <= 0)
            return true;

        var acceptedCount = await dbcontext.ProgramRegistrations.CountAsync(
            x => x.ProgramProjectId == projectId
                && x.Id != registrationId
                && (x.Status == ProgramRegistrationStatus.Approved || x.Status == ProgramRegistrationStatus.Attended),
            cancellationToken);

        return acceptedCount < target;
    }

    private async Task<Result<ProgramCertificateIssueResponse>> CreateCertificateIssueAsync(
        ProgramProject project,
        int? templateId,
        string? requestedCertificateNumber,
        string recipientName,
        DateTime? issuedAt,
        string? notes,
        CancellationToken cancellationToken)
    {
        if (templateId.HasValue && !await dbcontext.ProgramCertificateTemplates.AnyAsync(x => x.Id == templateId.Value, cancellationToken))
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.CertificateTemplateNotFound);

        var certificateNumber = string.IsNullOrWhiteSpace(requestedCertificateNumber)
            ? await GenerateCertificateNumberAsync(cancellationToken)
            : requestedCertificateNumber.Trim();
        var duplicate = await dbcontext.ProgramCertificateIssues.AnyAsync(x => x.CertificateNumber == certificateNumber, cancellationToken);
        if (duplicate)
            return Result.Failure<ProgramCertificateIssueResponse>(ProgramsProjectsErrors.DuplicateCertificateNumber);

        var issue = new ProgramCertificateIssue
        {
            ProgramProjectId = project.Id,
            ProgramProject = project,
            ProgramCertificateTemplateId = templateId,
            CertificateNumber = certificateNumber,
            RecipientName = recipientName.Trim(),
            IssuedAt = issuedAt ?? DateTime.UtcNow.AddHours(3),
            Notes = notes?.Trim()
        };

        dbcontext.ProgramCertificateIssues.Add(issue);
        QueueProjectActivity(issue.ProgramProjectId, ProgramProjectActivityType.CertificateIssued, $"إصدار شهادة: {issue.RecipientName}", issue.Notes, null, issue.Status.ToString(), null, certificateNumber);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(issue).Reference(x => x.ProgramCertificateTemplate).LoadAsync(cancellationToken);
        return Result.Success(MapCertificateIssue(issue));
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

    private async Task<string> GenerateProposalNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.ProgramSupplierProposals.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"PROP-{year}-{count:0000}";
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

    private static ProgramSupplierProposalResponse MapSupplierProposal(ProgramSupplierProposal proposal) =>
        new(
            proposal.Id,
            proposal.ProgramProjectId,
            proposal.ProgramProject?.Name ?? string.Empty,
            proposal.ProgramSupplierId,
            proposal.ProgramSupplier?.Name ?? string.Empty,
            proposal.ProposalNumber,
            proposal.Title,
            proposal.Scope,
            proposal.Amount,
            proposal.SubmittedAt,
            proposal.ValidUntil,
            proposal.Status.ToString(),
            proposal.DecisionNotes,
            proposal.DecidedAt,
            proposal.ConvertedContractId,
            proposal.ConvertedContract?.ContractNumber,
            proposal.Notes);

    private static ProgramIdeaResponse MapIdea(ProgramIdea idea) =>
        new(idea.Id, idea.Title, idea.OwnerName, idea.Description, idea.MarketingNotes, idea.EstimatedBudget, idea.Status.ToString(), idea.DecisionNotes, idea.DecidedAt, idea.ConvertedProjectId, idea.ConvertedProject?.ProjectCode, idea.CreatedAt);

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
