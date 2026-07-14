using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.HumanResources;
using Application.Service.TaskManagement;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.HumanResources;

public class HumanResourceService(ApplicationDbcontext dbcontext, ITaskManagementService? approvalWorkflow = null) : IHumanResourceService
{
    public async Task<Result<HumanResourceDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.AddHours(3).Date;
        var nextMonth = today.AddDays(30);
        var currentPayrollMonth = FirstDayOfMonth(today);
        var recentActivitySince = today.AddDays(-30);

        var employeesCount = await dbcontext.EmployeeProfiles.CountAsync(cancellationToken);
        var activeEmployeesCount = await dbcontext.EmployeeProfiles.CountAsync(x => x.Status == EmployeeStatus.Active, cancellationToken);
        var pendingLeaveRequestsCount = await dbcontext.EmployeeLeaveRequests.CountAsync(x => x.Status == LeaveRequestStatus.Pending, cancellationToken);
        var todayAttendanceCount = await dbcontext.EmployeeAttendanceRecords.CountAsync(x => x.WorkDate.Date == today, cancellationToken);
        var expiringDocumentsCount = await dbcontext.EmployeeDocuments.CountAsync(x => x.ExpiresAt.HasValue && x.ExpiresAt.Value.Date <= nextMonth, cancellationToken);
        var monthlyPayrollTotal = await dbcontext.EmployeeProfiles
            .Where(x => x.Status == EmployeeStatus.Active || x.Status == EmployeeStatus.OnLeave)
            .SumAsync(x => x.BasicSalary + x.Allowances, cancellationToken);
        var pendingAttendanceExcusesCount = await dbcontext.EmployeeAttendanceExcuses.CountAsync(x => x.Status == HumanResourceRequestStatus.Pending, cancellationToken);
        var pendingAdministrativeRequestsCount = await dbcontext.EmployeeAdministrativeRequests.CountAsync(x => x.Status == HumanResourceRequestStatus.Pending, cancellationToken);
        var pendingEvaluationsCount = await dbcontext.EmployeeEvaluations.CountAsync(x => x.Status == EmployeeEvaluationStatus.PendingApproval, cancellationToken);
        var openSafetyInspectionsCount = await dbcontext.HrSafetyInspections.CountAsync(x => x.Status == SafetyRecordStatus.Open || x.Status == SafetyRecordStatus.InProgress, cancellationToken);
        var openRecruitmentRequestsCount = await dbcontext.RecruitmentRequests.CountAsync(x => x.Status != RecruitmentRequestStatus.Completed && x.Status != RecruitmentRequestStatus.Cancelled, cancellationToken);
        var draftPayrollRecordsCount = await dbcontext.EmployeePayrollRecords.CountAsync(x => x.PayrollMonth == currentPayrollMonth && x.Status == PayrollRecordStatus.Draft, cancellationToken);
        var reviewedPayrollRecordsCount = await dbcontext.EmployeePayrollRecords.CountAsync(x => x.PayrollMonth == currentPayrollMonth && x.Status == PayrollRecordStatus.Reviewed, cancellationToken);
        var approvedPayrollRecordsCount = await dbcontext.EmployeePayrollRecords.CountAsync(x => x.PayrollMonth == currentPayrollMonth && x.Status == PayrollRecordStatus.Approved, cancellationToken);
        var approvedPayrollTotal = await dbcontext.EmployeePayrollRecords
            .Where(x => x.PayrollMonth == currentPayrollMonth && (x.Status == PayrollRecordStatus.Approved || x.Status == PayrollRecordStatus.Paid))
            .SumAsync(x => x.NetSalary, cancellationToken);
        var recentActivityCount = await dbcontext.HumanResourceActivities.CountAsync(x => x.OccurredAt.Date >= recentActivitySince, cancellationToken);

        return Result.Success(new HumanResourceDashboardResponse(
            employeesCount,
            activeEmployeesCount,
            pendingLeaveRequestsCount,
            todayAttendanceCount,
            expiringDocumentsCount,
            monthlyPayrollTotal,
            pendingAttendanceExcusesCount,
            pendingAdministrativeRequestsCount,
            pendingEvaluationsCount,
            openSafetyInspectionsCount,
            openRecruitmentRequestsCount,
            draftPayrollRecordsCount,
            reviewedPayrollRecordsCount,
            approvedPayrollRecordsCount,
            approvedPayrollTotal,
            recentActivityCount));
    }

    public async Task<Result<IEnumerable<EmployeeDepartmentResponse>>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var departments = await dbcontext.EmployeeDepartments
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.NameAr)
            .Select(x => MapDepartment(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeDepartmentResponse>>(departments);
    }

    public async Task<Result<EmployeeDepartmentResponse>> SaveDepartmentAsync(int? id, UpsertLookupRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<EmployeeDepartmentResponse>(HumanResourceErrors.InvalidRequest);

        if (id.HasValue)
        {
            var department = await dbcontext.EmployeeDepartments.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (department is null)
                return Result.Failure<EmployeeDepartmentResponse>(HumanResourceErrors.DepartmentNotFound);

            department.NameAr = request.NameAr.Trim();
            department.NameEn = request.NameEn?.Trim();
            department.IsActive = request.IsActive;
            await dbcontext.SaveChangesAsync(cancellationToken);
            return Result.Success(MapDepartment(department));
        }

        var created = new EmployeeDepartment
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn?.Trim(),
            IsActive = request.IsActive
        };

        dbcontext.EmployeeDepartments.Add(created);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDepartment(created));
    }

    public async Task<Result<IEnumerable<JobTitleResponse>>> GetJobTitlesAsync(CancellationToken cancellationToken = default)
    {
        var titles = await dbcontext.JobTitles
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.NameAr)
            .Select(x => MapJobTitle(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<JobTitleResponse>>(titles);
    }

    public async Task<Result<JobTitleResponse>> SaveJobTitleAsync(int? id, UpsertLookupRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<JobTitleResponse>(HumanResourceErrors.InvalidRequest);

        if (id.HasValue)
        {
            var title = await dbcontext.JobTitles.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (title is null)
                return Result.Failure<JobTitleResponse>(HumanResourceErrors.JobTitleNotFound);

            title.NameAr = request.NameAr.Trim();
            title.NameEn = request.NameEn?.Trim();
            title.IsActive = request.IsActive;
            await dbcontext.SaveChangesAsync(cancellationToken);
            return Result.Success(MapJobTitle(title));
        }

        var created = new JobTitle
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn?.Trim(),
            IsActive = request.IsActive
        };

        dbcontext.JobTitles.Add(created);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapJobTitle(created));
    }

    public async Task<Result<IEnumerable<EmployeeResponse>>> SearchEmployeesAsync(EmployeeSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeProfiles
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.EmployeeNumber.Contains(search) ||
                (x.NationalId != null && x.NationalId.Contains(search)) ||
                (x.Email != null && x.Email.Contains(search)) ||
                (x.Mobile != null && x.Mobile.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

        if (request.JobTitleId.HasValue)
            query = query.Where(x => x.JobTitleId == request.JobTitleId.Value);

        if (request.AccountType.HasValue)
            query = query.Where(x => x.AccountType == request.AccountType.Value);

        var employees = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.FullName)
            .Select(x => MapEmployee(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeResponse>>(employees);
    }

    public async Task<Result<EmployeeResponse>> GetEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return employee is null
            ? Result.Failure<EmployeeResponse>(HumanResourceErrors.EmployeeNotFound)
            : Result.Success(MapEmployee(employee));
    }

    public async Task<Result<EmployeeResponse>> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.BasicSalary < 0 || request.Allowances < 0)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.InvalidRequest);

        var lookupResult = await ValidateLookupsAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (lookupResult is not null)
            return Result.Failure<EmployeeResponse>(lookupResult);

        var employeeNumber = string.IsNullOrWhiteSpace(request.EmployeeNumber)
            ? await GenerateEmployeeNumberAsync(cancellationToken)
            : request.EmployeeNumber.Trim();

        if (await dbcontext.EmployeeProfiles.AnyAsync(x => x.EmployeeNumber == employeeNumber, cancellationToken))
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.DuplicateEmployeeNumber);

        var employee = new EmployeeProfile
        {
            EmployeeNumber = employeeNumber,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Email = request.Email?.Trim(),
            Mobile = request.Mobile?.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            AccountType = request.AccountType,
            HireDate = request.HireDate ?? DateTime.UtcNow.AddHours(3),
            BasicSalary = request.BasicSalary,
            Allowances = request.Allowances,
            Notes = request.Notes?.Trim()
        };

        dbcontext.EmployeeProfiles.Add(employee);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            employee.Id,
            employee.Id,
            HumanResourceActivityAction.Created,
            employee.FullName,
            null,
            employee.Status.ToString(),
            "تم إنشاء ملف الموظف.");
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.Department).LoadAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.JobTitle).LoadAsync(cancellationToken);
        return Result.Success(MapEmployee(employee));
    }

    public async Task<Result<EmployeeResponse>> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (employee is null)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName) || request.BasicSalary < 0 || request.Allowances < 0 || request.Status == EmployeeStatus.Terminated)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.InvalidRequest);

        var lookupResult = await ValidateLookupsAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (lookupResult is not null)
            return Result.Failure<EmployeeResponse>(lookupResult);

        var oldStatus = employee.Status.ToString();
        employee.FullName = request.FullName.Trim();
        employee.NationalId = request.NationalId?.Trim();
        employee.Email = request.Email?.Trim();
        employee.Mobile = request.Mobile?.Trim();
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitleId = request.JobTitleId;
        employee.AccountType = request.AccountType;
        employee.HireDate = request.HireDate;
        employee.Status = request.Status;
        employee.BasicSalary = request.BasicSalary;
        employee.Allowances = request.Allowances;
        employee.Notes = request.Notes?.Trim();

        if (employee.Status != EmployeeStatus.Terminated)
        {
            employee.TerminatedAt = null;
            employee.TerminationReason = null;
        }

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            employee.Id,
            employee.Id,
            oldStatus == employee.Status.ToString() ? HumanResourceActivityAction.Updated : HumanResourceActivityAction.StatusChanged,
            employee.FullName,
            oldStatus,
            employee.Status.ToString(),
            "تم تحديث ملف الموظف.");
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.Department).LoadAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.JobTitle).LoadAsync(cancellationToken);
        return Result.Success(MapEmployee(employee));
    }

    public async Task<Result> TerminateEmployeeAsync(int id, TerminateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (employee is null)
            return Result.Failure(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(HumanResourceErrors.InvalidRequest);

        var oldStatus = employee.Status.ToString();
        employee.Status = EmployeeStatus.Terminated;
        employee.TerminatedAt = DateTime.UtcNow.AddHours(3);
        employee.TerminationReason = request.Reason.Trim();
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            employee.Id,
            employee.Id,
            HumanResourceActivityAction.StatusChanged,
            employee.FullName,
            oldStatus,
            employee.Status.ToString(),
            employee.TerminationReason);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (employee is null)
            return Result.Failure(HumanResourceErrors.EmployeeNotFound);

        var oldStatus = employee.Status.ToString();
        employee.Status = EmployeeStatus.Active;
        employee.TerminatedAt = null;
        employee.TerminationReason = null;
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            employee.Id,
            employee.Id,
            HumanResourceActivityAction.StatusChanged,
            employee.FullName,
            oldStatus,
            employee.Status.ToString(),
            "تمت إعادة الموظف للخدمة.");
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<EmployeeAttendanceResponse>> RecordAttendanceAsync(RecordEmployeeAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeAttendanceResponse>(HumanResourceErrors.EmployeeNotFound);

        var workDate = request.WorkDate.Date;
        if (await dbcontext.EmployeeAttendanceRecords.AnyAsync(x => x.EmployeeProfileId == request.EmployeeProfileId && x.WorkDate.Date == workDate, cancellationToken))
            return Result.Failure<EmployeeAttendanceResponse>(HumanResourceErrors.DuplicateAttendance);

        var status = await ResolveAttendanceStatusAsync(request, cancellationToken);
        var attendance = new EmployeeAttendance
        {
            EmployeeProfileId = request.EmployeeProfileId,
            WorkDate = workDate,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Status = status,
            Notes = request.Notes?.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeAttendanceRecords.Add(attendance);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AttendanceRecord,
            attendance.Id,
            attendance.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            $"{attendance.WorkDate:yyyy-MM-dd} - {employee.FullName}",
            null,
            attendance.Status.ToString(),
            attendance.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendance(attendance));
    }

    public async Task<Result<EmployeeAttendanceResponse>> UpdateAttendanceAsync(int id, RecordEmployeeAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var attendance = await dbcontext.EmployeeAttendanceRecords
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (attendance is null)
            return Result.Failure<EmployeeAttendanceResponse>(HumanResourceErrors.AttendanceRecordNotFound);

        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeAttendanceResponse>(HumanResourceErrors.EmployeeNotFound);

        var workDate = request.WorkDate.Date;
        if (await dbcontext.EmployeeAttendanceRecords.AnyAsync(x => x.Id != id && x.EmployeeProfileId == request.EmployeeProfileId && x.WorkDate.Date == workDate, cancellationToken))
            return Result.Failure<EmployeeAttendanceResponse>(HumanResourceErrors.DuplicateAttendance);

        var oldStatus = attendance.Status.ToString();
        var resolvedStatus = await ResolveAttendanceStatusAsync(request, cancellationToken);
        attendance.EmployeeProfileId = request.EmployeeProfileId;
        attendance.EmployeeProfile = employee;
        attendance.WorkDate = workDate;
        attendance.CheckIn = request.CheckIn;
        attendance.CheckOut = request.CheckOut;
        attendance.Status = resolvedStatus;
        attendance.Notes = request.Notes?.Trim();

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AttendanceRecord,
            attendance.Id,
            attendance.EmployeeProfileId,
            oldStatus == attendance.Status.ToString() ? HumanResourceActivityAction.Updated : HumanResourceActivityAction.StatusChanged,
            $"{attendance.WorkDate:yyyy-MM-dd} - {employee.FullName}",
            oldStatus,
            attendance.Status.ToString(),
            attendance.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendance(attendance));
    }

    public async Task<Result<IEnumerable<EmployeeAttendanceResponse>>> GetAttendanceAsync(int? employeeId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeAttendanceRecords
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (from.HasValue)
            query = query.Where(x => x.WorkDate.Date >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(x => x.WorkDate.Date <= to.Value.Date);

        var records = await query
            .OrderByDescending(x => x.WorkDate)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .Select(x => MapAttendance(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeAttendanceResponse>>(records);
    }

    public async Task<Result<EmployeeLeaveRequestResponse>> CreateLeaveRequestAsync(CreateEmployeeLeaveRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeLeaveRequestResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.LeaveType) || request.EndsAt.Date < request.StartsAt.Date)
            return Result.Failure<EmployeeLeaveRequestResponse>(HumanResourceErrors.InvalidRequest);

        var leave = new EmployeeLeaveRequest
        {
            EmployeeProfileId = request.EmployeeProfileId,
            LeaveType = request.LeaveType.Trim(),
            StartsAt = request.StartsAt.Date,
            EndsAt = request.EndsAt.Date,
            Reason = request.Reason?.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeLeaveRequests.Add(leave);
        await dbcontext.SaveChangesAsync(cancellationToken);
        if (approvalWorkflow is not null)
            await approvalWorkflow.EnsureApprovalRequestForEntityAsync(
                nameof(EmployeeLeaveRequest), leave.Id, $"طلب إجازة - {employee.FullName}", cancellationToken: cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.LeaveRequest,
            leave.Id,
            leave.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            leave.LeaveType,
            null,
            leave.Status.ToString(),
            leave.Reason);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLeave(leave));
    }

    public async Task<Result<EmployeeLeaveRequestResponse>> DecideLeaveRequestAsync(int id, DecideEmployeeLeaveRequest request, CancellationToken cancellationToken = default)
    {
        var leave = await dbcontext.EmployeeLeaveRequests
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (leave is null)
            return Result.Failure<EmployeeLeaveRequestResponse>(HumanResourceErrors.LeaveRequestNotFound);
        if (leave.Status != LeaveRequestStatus.Pending)
            return Result.Failure<EmployeeLeaveRequestResponse>(HumanResourceErrors.LeaveAlreadyDecided);

        if (!request.Approved && string.IsNullOrWhiteSpace(request.Notes))
            return Result.Failure<EmployeeLeaveRequestResponse>(HumanResourceErrors.InvalidRequest);

        if (request.Approved)
        {
            var balanceValidationError = await ValidateApprovedLeaveBalanceAsync(leave, cancellationToken);
            if (balanceValidationError is not null)
                return Result.Failure<EmployeeLeaveRequestResponse>(balanceValidationError);
        }

        var oldStatus = leave.Status.ToString();
        leave.Status = request.Approved ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
        leave.DecisionNotes = request.Notes?.Trim();
        leave.DecidedAt = DateTime.UtcNow.AddHours(3);

        if (request.Approved && leave.EmployeeProfile is not null)
        {
            leave.EmployeeProfile.Status = EmployeeStatus.OnLeave;
            await ApplyApprovedLeaveBalanceAsync(leave, cancellationToken);
        }

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.LeaveRequest,
            leave.Id,
            leave.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            leave.LeaveType,
            oldStatus,
            leave.Status.ToString(),
            leave.DecisionNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLeave(leave));
    }

    public async Task<Result<IEnumerable<EmployeeLeaveRequestResponse>>> GetLeaveRequestsAsync(int? employeeId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeLeaveRequests
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        var leaves = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapLeave(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeLeaveRequestResponse>>(leaves);
    }

    public async Task<Result<EmployeeDocumentResponse>> AddDocumentAsync(AddEmployeeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeDocumentResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.DocumentType))
            return Result.Failure<EmployeeDocumentResponse>(HumanResourceErrors.InvalidRequest);

        var document = new EmployeeDocument
        {
            EmployeeProfileId = request.EmployeeProfileId,
            Title = request.Title.Trim(),
            DocumentType = request.DocumentType.Trim(),
            FilePath = request.FilePath?.Trim(),
            ExpiresAt = request.ExpiresAt,
            Notes = request.Notes?.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeDocuments.Add(document);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeDocument,
            document.Id,
            document.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            document.Title,
            null,
            document.DocumentType,
            document.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDocument(document));
    }

    public async Task<Result<EmployeeDocumentResponse>> UpdateDocumentAsync(int id, AddEmployeeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var document = await dbcontext.EmployeeDocuments
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
            return Result.Failure<EmployeeDocumentResponse>(HumanResourceErrors.EmployeeDocumentNotFound);

        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeDocumentResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.DocumentType))
            return Result.Failure<EmployeeDocumentResponse>(HumanResourceErrors.InvalidRequest);

        var oldDocumentType = document.DocumentType;
        document.EmployeeProfileId = request.EmployeeProfileId;
        document.EmployeeProfile = employee;
        document.Title = request.Title.Trim();
        document.DocumentType = request.DocumentType.Trim();
        document.FilePath = request.FilePath?.Trim();
        document.ExpiresAt = request.ExpiresAt;
        document.Notes = request.Notes?.Trim();

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeDocument,
            document.Id,
            document.EmployeeProfileId,
            HumanResourceActivityAction.Updated,
            document.Title,
            oldDocumentType,
            document.DocumentType,
            document.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDocument(document));
    }

    public async Task<Result<IEnumerable<EmployeeDocumentResponse>>> GetDocumentsAsync(int? employeeId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeDocuments
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        var documents = await query
            .OrderBy(x => x.ExpiresAt ?? DateTime.MaxValue)
            .ThenBy(x => x.Title)
            .Select(x => MapDocument(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeDocumentResponse>>(documents);
    }

    public async Task<Result<IEnumerable<EmployeeDisciplinaryRecordResponse>>> GetDisciplinaryRecordsAsync(int? employeeId, EmployeeDisciplinaryRecordType? type, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeDisciplinaryRecords
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        var records = await query
            .OrderByDescending(x => x.RecordDate)
            .Select(x => MapDisciplinaryRecord(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeDisciplinaryRecordResponse>>(records);
    }

    public async Task<Result<EmployeeDisciplinaryRecordResponse>> CreateDisciplinaryRecordAsync(CreateEmployeeDisciplinaryRecordRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeDisciplinaryRecordResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure<EmployeeDisciplinaryRecordResponse>(HumanResourceErrors.InvalidRequest);

        var record = new EmployeeDisciplinaryRecord
        {
            EmployeeProfileId = request.EmployeeProfileId,
            Type = request.Type,
            RecordDate = request.RecordDate ?? DateTime.UtcNow.AddHours(3),
            Title = request.Title.Trim(),
            Reason = request.Reason.Trim(),
            ActionTaken = request.ActionTaken?.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeDisciplinaryRecords.Add(record);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.DisciplinaryRecord,
            record.Id,
            record.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            record.Title,
            null,
            record.Status.ToString(),
            record.Reason);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDisciplinaryRecord(record));
    }

    public async Task<Result<EmployeeDisciplinaryRecordResponse>> DecideDisciplinaryRecordAsync(int id, DecideHumanResourceItemRequest request, CancellationToken cancellationToken = default)
    {
        var record = await dbcontext.EmployeeDisciplinaryRecords
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (record is null)
            return Result.Failure<EmployeeDisciplinaryRecordResponse>(HumanResourceErrors.DisciplinaryRecordNotFound);

        if (record.Status != HumanResourceRequestStatus.Pending)
            return Result.Failure<EmployeeDisciplinaryRecordResponse>(HumanResourceErrors.InvalidRequest);

        var oldStatus = record.Status.ToString();
        record.Status = request.Status;
        record.DecisionNotes = request.Notes?.Trim();
        record.DecidedAt = DateTime.UtcNow.AddHours(3);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.DisciplinaryRecord,
            record.Id,
            record.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            record.Title,
            oldStatus,
            record.Status.ToString(),
            record.DecisionNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDisciplinaryRecord(record));
    }

    public async Task<Result<IEnumerable<EmployeeLeaveBalanceResponse>>> GetLeaveBalancesAsync(int? employeeId, int? year, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeLeaveBalances
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (year.HasValue)
            query = query.Where(x => x.Year == year.Value);

        var balances = await query
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .ThenBy(x => x.LeaveType)
            .Select(x => MapLeaveBalance(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeLeaveBalanceResponse>>(balances);
    }

    public async Task<Result<EmployeeLeaveBalanceResponse>> SaveLeaveBalanceAsync(int? id, SaveEmployeeLeaveBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeLeaveBalanceResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.LeaveType) || request.Year < 2000 || request.EntitledDays < 0 || request.UsedDays < 0 || request.CarriedDays < 0)
            return Result.Failure<EmployeeLeaveBalanceResponse>(HumanResourceErrors.InvalidRequest);

        var leaveType = request.LeaveType.Trim();
        var duplicateExists = await dbcontext.EmployeeLeaveBalances.AnyAsync(x =>
            x.EmployeeProfileId == request.EmployeeProfileId &&
            x.Year == request.Year &&
            x.LeaveType == leaveType &&
            (!id.HasValue || x.Id != id.Value),
            cancellationToken);

        if (duplicateExists)
            return Result.Failure<EmployeeLeaveBalanceResponse>(HumanResourceErrors.DuplicateLeaveBalance);

        EmployeeLeaveBalance balance;
        if (id.HasValue)
        {
            balance = await dbcontext.EmployeeLeaveBalances
                .Include(x => x.EmployeeProfile)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (balance is null)
                return Result.Failure<EmployeeLeaveBalanceResponse>(HumanResourceErrors.LeaveBalanceNotFound);
        }
        else
        {
            balance = new EmployeeLeaveBalance { EmployeeProfile = employee };
            dbcontext.EmployeeLeaveBalances.Add(balance);
        }

        balance.EmployeeProfileId = request.EmployeeProfileId;
        balance.Year = request.Year;
        balance.LeaveType = leaveType;
        balance.EntitledDays = request.EntitledDays;
        balance.UsedDays = request.UsedDays;
        balance.CarriedDays = request.CarriedDays;
        balance.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLeaveBalance(balance));
    }

    public async Task<Result<IEnumerable<EmployeeEvaluationResponse>>> GetEvaluationsAsync(int? employeeId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeEvaluations
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        var evaluations = await query
            .OrderByDescending(x => x.PeriodEnd)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .Select(x => MapEvaluation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeEvaluationResponse>>(evaluations);
    }

    public async Task<Result<EmployeeEvaluationResponse>> SaveEvaluationAsync(int? id, SaveEmployeeEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.EmployeeNotFound);

        if (request.PeriodEnd.Date < request.PeriodStart.Date || request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore || string.IsNullOrWhiteSpace(request.Rating))
            return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.InvalidRequest);
        if (request.Status is EmployeeEvaluationStatus.Approved or EmployeeEvaluationStatus.Rejected)
            return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.InvalidRequest);

        var isNew = !id.HasValue;
        EmployeeEvaluation evaluation;
        string? oldStatus = null;
        if (id.HasValue)
        {
            evaluation = await dbcontext.EmployeeEvaluations
                .Include(x => x.EmployeeProfile)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (evaluation is null)
                return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.EvaluationNotFound);
            if (evaluation.Status is EmployeeEvaluationStatus.Approved or EmployeeEvaluationStatus.Rejected)
                return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.InvalidEvaluationStatusTransition);
            oldStatus = evaluation.Status.ToString();
        }
        else
        {
            evaluation = new EmployeeEvaluation { EmployeeProfile = employee };
            dbcontext.EmployeeEvaluations.Add(evaluation);
        }

        evaluation.EmployeeProfileId = request.EmployeeProfileId;
        evaluation.PeriodStart = request.PeriodStart.Date;
        evaluation.PeriodEnd = request.PeriodEnd.Date;
        evaluation.Score = request.Score;
        evaluation.MaxScore = request.MaxScore;
        evaluation.Rating = request.Rating.Trim();
        evaluation.EvaluatorName = request.EvaluatorName?.Trim();
        evaluation.Strengths = request.Strengths?.Trim();
        evaluation.ImprovementAreas = request.ImprovementAreas?.Trim();
        evaluation.Status = request.Status;
        evaluation.Notes = request.Notes?.Trim();

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            evaluation.EmployeeProfileId,
            evaluation.EmployeeProfileId,
            isNew ? HumanResourceActivityAction.Created : oldStatus == evaluation.Status.ToString() ? HumanResourceActivityAction.Updated : HumanResourceActivityAction.StatusChanged,
            $"تقييم {evaluation.Rating}",
            oldStatus,
            evaluation.Status.ToString(),
            evaluation.Notes ?? evaluation.ImprovementAreas);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEvaluation(evaluation));
    }

    public async Task<Result<EmployeeEvaluationResponse>> DecideEvaluationAsync(int id, DecideEmployeeEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var evaluation = await dbcontext.EmployeeEvaluations
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evaluation is null)
            return Result.Failure<EmployeeEvaluationResponse>(HumanResourceErrors.EvaluationNotFound);

        var decisionNotes = request.DecisionNotes?.Trim();
        var transitionError = ValidateEvaluationDecision(evaluation, request.Status, decisionNotes);
        if (transitionError is not null)
            return Result.Failure<EmployeeEvaluationResponse>(transitionError);

        var oldStatus = evaluation.Status.ToString();
        evaluation.Status = request.Status;
        evaluation.DecisionNotes = decisionNotes;
        evaluation.DecidedAt = DateTime.UtcNow.AddHours(3);

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            evaluation.EmployeeProfileId,
            evaluation.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            $"اعتماد تقييم {evaluation.Rating}",
            oldStatus,
            evaluation.Status.ToString(),
            evaluation.DecisionNotes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEvaluation(evaluation));
    }

    public async Task<Result<IEnumerable<EmployeeCardIssueResponse>>> GetCardIssuesAsync(int? employeeId, string? cardType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeCardIssues
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (!string.IsNullOrWhiteSpace(cardType))
        {
            var value = cardType.Trim();
            query = query.Where(x => x.CardType.Contains(value));
        }

        var cards = await query
            .OrderByDescending(x => x.IssuedAt)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .Select(x => MapCardIssue(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeCardIssueResponse>>(cards);
    }

    public async Task<Result<EmployeeCardIssueResponse>> IssueCardAsync(IssueEmployeeCardRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeCardIssueResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.CardType))
            return Result.Failure<EmployeeCardIssueResponse>(HumanResourceErrors.InvalidRequest);

        var cardNumber = string.IsNullOrWhiteSpace(request.CardNumber)
            ? await GenerateCardNumberAsync(request.CardType, cancellationToken)
            : request.CardNumber.Trim();

        if (await dbcontext.EmployeeCardIssues.AnyAsync(x => x.CardNumber == cardNumber, cancellationToken))
            return Result.Failure<EmployeeCardIssueResponse>(HumanResourceErrors.DuplicateCardNumber);

        var card = new EmployeeCardIssue
        {
            EmployeeProfileId = request.EmployeeProfileId,
            CardType = request.CardType.Trim(),
            CardNumber = cardNumber,
            IssuedAt = request.IssuedAt ?? DateTime.UtcNow.AddHours(3),
            ExpiresAt = request.ExpiresAt,
            Notes = request.Notes?.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeCardIssues.Add(card);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            card.EmployeeProfileId,
            card.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            $"إصدار بطاقة {card.CardType}",
            null,
            card.Status.ToString(),
            card.CardNumber);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCardIssue(card));
    }

    public async Task<Result<EmployeeCardIssueResponse>> DecideCardIssueAsync(int id, DecideEmployeeCardIssueRequest request, CancellationToken cancellationToken = default)
    {
        var card = await dbcontext.EmployeeCardIssues
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (card is null)
            return Result.Failure<EmployeeCardIssueResponse>(HumanResourceErrors.CardIssueNotFound);

        var notes = request.Notes?.Trim();
        var transitionError = ValidateCardIssueDecision(card, request.Status, notes);
        if (transitionError is not null)
            return Result.Failure<EmployeeCardIssueResponse>(transitionError);

        var oldStatus = card.Status.ToString();
        card.Status = request.Status;
        card.Notes = string.IsNullOrWhiteSpace(notes)
            ? card.Notes
            : string.IsNullOrWhiteSpace(card.Notes)
                ? notes
                : $"{card.Notes.Trim()} | {notes}";

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            card.EmployeeProfileId,
            card.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            $"تحديث بطاقة {card.CardType}",
            oldStatus,
            card.Status.ToString(),
            card.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCardIssue(card));
    }

    public async Task<Result<IEnumerable<EmployeeLetterRequestResponse>>> GetLetterRequestsAsync(int? employeeId, HumanResourceRequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeLetterRequests
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var letters = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapLetterRequest(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeLetterRequestResponse>>(letters);
    }

    public async Task<Result<EmployeeLetterRequestResponse>> SaveLetterRequestAsync(int? id, SaveEmployeeLetterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.LetterType) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<EmployeeLetterRequestResponse>(HumanResourceErrors.InvalidRequest);

        EmployeeProfile? employee = null;
        if (request.EmployeeProfileId.HasValue)
        {
            employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId.Value, cancellationToken);
            if (employee is null)
                return Result.Failure<EmployeeLetterRequestResponse>(HumanResourceErrors.EmployeeNotFound);
        }

        var isNew = !id.HasValue;
        EmployeeLetterRequest letter;
        if (id.HasValue)
        {
            letter = await dbcontext.EmployeeLetterRequests
                .Include(x => x.EmployeeProfile)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (letter is null)
                return Result.Failure<EmployeeLetterRequestResponse>(HumanResourceErrors.LetterRequestNotFound);
        }
        else
        {
            letter = new EmployeeLetterRequest();
            dbcontext.EmployeeLetterRequests.Add(letter);
        }

        letter.EmployeeProfileId = request.EmployeeProfileId;
        letter.EmployeeProfile = employee;
        letter.LetterType = request.LetterType.Trim();
        letter.Subject = request.Subject.Trim();
        letter.Purpose = request.Purpose?.Trim();
        letter.Body = request.Body.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.LetterRequest,
            letter.Id,
            letter.EmployeeProfileId,
            isNew ? HumanResourceActivityAction.Created : HumanResourceActivityAction.Updated,
            letter.Subject,
            null,
            letter.Status.ToString(),
            letter.Purpose);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLetterRequest(letter));
    }

    public async Task<Result<EmployeeLetterRequestResponse>> DecideLetterRequestAsync(int id, DecideHumanResourceItemRequest request, CancellationToken cancellationToken = default)
    {
        var letter = await dbcontext.EmployeeLetterRequests
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (letter is null)
            return Result.Failure<EmployeeLetterRequestResponse>(HumanResourceErrors.LetterRequestNotFound);

        if (letter.Status != HumanResourceRequestStatus.Pending)
            return Result.Failure<EmployeeLetterRequestResponse>(HumanResourceErrors.InvalidRequest);

        var oldStatus = letter.Status.ToString();
        letter.Status = request.Status;
        letter.DecisionNotes = request.Notes?.Trim();
        letter.IssuedAt = request.Status is HumanResourceRequestStatus.Approved or HumanResourceRequestStatus.Completed
            ? DateTime.UtcNow.AddHours(3)
            : letter.IssuedAt;

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.LetterRequest,
            letter.Id,
            letter.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            letter.Subject,
            oldStatus,
            letter.Status.ToString(),
            letter.DecisionNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLetterRequest(letter));
    }

    public async Task<Result<IEnumerable<EmployeePayrollRecordResponse>>> GetPayrollRecordsAsync(DateTime? payrollMonth, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeePayrollRecords
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (payrollMonth.HasValue)
        {
            var month = FirstDayOfMonth(payrollMonth.Value);
            query = query.Where(x => x.PayrollMonth == month);
        }

        var records = await query
            .OrderByDescending(x => x.PayrollMonth)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .Select(x => MapPayrollRecord(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeePayrollRecordResponse>>(records);
    }

    public async Task<Result<IEnumerable<EmployeePayrollRecordResponse>>> GeneratePayrollPreviewAsync(GeneratePayrollPreviewRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DefaultDeductions < 0)
            return Result.Failure<IEnumerable<EmployeePayrollRecordResponse>>(HumanResourceErrors.InvalidRequest);

        var month = FirstDayOfMonth(request.PayrollMonth);
        var employees = await dbcontext.EmployeeProfiles
            .Where(x => x.Status == EmployeeStatus.Active || x.Status == EmployeeStatus.OnLeave)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        var employeeIds = employees.Select(x => x.Id).ToList();
        var existingRecords = await dbcontext.EmployeePayrollRecords
            .Include(x => x.EmployeeProfile)
            .Where(x => x.PayrollMonth == month && employeeIds.Contains(x.EmployeeProfileId))
            .ToDictionaryAsync(x => x.EmployeeProfileId, cancellationToken);

        foreach (var employee in employees)
        {
            if (!existingRecords.TryGetValue(employee.Id, out var record))
            {
                record = new EmployeePayrollRecord
                {
                    EmployeeProfileId = employee.Id,
                    EmployeeProfile = employee,
                    PayrollMonth = month
                };
                dbcontext.EmployeePayrollRecords.Add(record);
                existingRecords[employee.Id] = record;
            }

            if (record.Status != PayrollRecordStatus.Draft)
                continue;

            record.BasicSalary = employee.BasicSalary;
            record.Allowances = employee.Allowances;
            record.Deductions = request.DefaultDeductions;
            record.NetSalary = employee.BasicSalary + employee.Allowances - request.DefaultDeductions;
            record.Notes = request.Notes?.Trim();
        }

        await dbcontext.SaveChangesAsync(cancellationToken);

        var generated = existingRecords.Values
            .OrderBy(x => x.EmployeeProfile?.FullName)
            .Select(MapPayrollRecord)
            .ToList();

        return Result.Success<IEnumerable<EmployeePayrollRecordResponse>>(generated);
    }

    public async Task<Result<EmployeePayrollRecordResponse>> DecidePayrollRecordAsync(int id, DecidePayrollRecordRequest request, CancellationToken cancellationToken = default)
    {
        var record = await dbcontext.EmployeePayrollRecords
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (record is null)
            return Result.Failure<EmployeePayrollRecordResponse>(HumanResourceErrors.PayrollRecordNotFound);

        if (record.Status == PayrollRecordStatus.Paid)
            return Result.Failure<EmployeePayrollRecordResponse>(HumanResourceErrors.PayrollAlreadyPaid);

        if (!IsValidPayrollTransition(record.Status, request.Status))
            return Result.Failure<EmployeePayrollRecordResponse>(HumanResourceErrors.InvalidPayrollStatusTransition);

        var oldStatus = record.Status.ToString();
        var now = DateTime.UtcNow.AddHours(3);
        record.Status = request.Status;
        record.DecisionNotes = request.Notes?.Trim();

        if (record.Status == PayrollRecordStatus.Reviewed)
            record.ReviewedAt = now;
        else if (record.Status == PayrollRecordStatus.Approved)
            record.ApprovedAt = now;
        else if (record.Status == PayrollRecordStatus.Paid)
            record.PaidAt = now;

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.PayrollRecord,
            record.Id,
            record.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            $"{record.PayrollMonth:yyyy-MM} - {record.EmployeeProfile?.FullName}",
            oldStatus,
            record.Status.ToString(),
            record.DecisionNotes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPayrollRecord(record));
    }

    public async Task<Result<IEnumerable<AttendancePolicyResponse>>> GetAttendancePoliciesAsync(CancellationToken cancellationToken = default)
    {
        var policies = await dbcontext.EmployeeAttendancePolicies
            .AsNoTracking()
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(x => MapAttendancePolicy(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<AttendancePolicyResponse>>(policies);
    }

    public async Task<Result<AttendancePolicyResponse>> SaveAttendancePolicyAsync(int? id, SaveAttendancePolicyRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.WorkEnd <= request.WorkStart || request.GraceMinutes < 0 || string.IsNullOrWhiteSpace(request.WorkDays))
            return Result.Failure<AttendancePolicyResponse>(HumanResourceErrors.InvalidRequest);

        AttendancePolicyResponse response;
        if (request.IsDefault)
        {
            var defaultPolicies = await dbcontext.EmployeeAttendancePolicies
                .Where(x => !id.HasValue || x.Id != id.Value)
                .ToListAsync(cancellationToken);

            foreach (var policy in defaultPolicies)
                policy.IsDefault = false;
        }

        EmployeeAttendancePolicy entity;
        if (id.HasValue)
        {
            entity = await dbcontext.EmployeeAttendancePolicies.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (entity is null)
                return Result.Failure<AttendancePolicyResponse>(HumanResourceErrors.AttendancePolicyNotFound);
        }
        else
        {
            entity = new EmployeeAttendancePolicy();
            dbcontext.EmployeeAttendancePolicies.Add(entity);
        }

        entity.Name = request.Name.Trim();
        entity.WorkStart = request.WorkStart;
        entity.WorkEnd = request.WorkEnd;
        entity.GraceMinutes = request.GraceMinutes;
        entity.WorkDays = request.WorkDays.Trim();
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        response = MapAttendancePolicy(entity);
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<AttendanceLocationResponse>>> GetAttendanceLocationsAsync(CancellationToken cancellationToken = default)
    {
        var locations = await dbcontext.EmployeeAttendanceLocations
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .Select(x => MapAttendanceLocation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<AttendanceLocationResponse>>(locations);
    }

    public async Task<Result<AttendanceLocationResponse>> SaveAttendanceLocationAsync(int? id, SaveAttendanceLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.RadiusMeters < 0)
            return Result.Failure<AttendanceLocationResponse>(HumanResourceErrors.InvalidRequest);

        EmployeeAttendanceLocation location;
        if (id.HasValue)
        {
            location = await dbcontext.EmployeeAttendanceLocations.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (location is null)
                return Result.Failure<AttendanceLocationResponse>(HumanResourceErrors.AttendanceLocationNotFound);
        }
        else
        {
            location = new EmployeeAttendanceLocation();
            dbcontext.EmployeeAttendanceLocations.Add(location);
        }

        location.Name = request.Name.Trim();
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.RadiusMeters = request.RadiusMeters;
        location.IsActive = request.IsActive;
        location.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendanceLocation(location));
    }

    public async Task<Result<IEnumerable<OfficialVacationResponse>>> GetOfficialVacationsAsync(int? year, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeOfficialVacations.AsNoTracking().AsQueryable();

        if (year.HasValue)
            query = query.Where(x => x.StartsAt.Year == year.Value || x.EndsAt.Year == year.Value);

        var vacations = await query
            .OrderBy(x => x.StartsAt)
            .Select(x => MapOfficialVacation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<OfficialVacationResponse>>(vacations);
    }

    public async Task<Result<OfficialVacationResponse>> SaveOfficialVacationAsync(int? id, SaveOfficialVacationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.EndsAt.Date < request.StartsAt.Date)
            return Result.Failure<OfficialVacationResponse>(HumanResourceErrors.InvalidRequest);

        EmployeeOfficialVacation vacation;
        if (id.HasValue)
        {
            vacation = await dbcontext.EmployeeOfficialVacations.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (vacation is null)
                return Result.Failure<OfficialVacationResponse>(HumanResourceErrors.OfficialVacationNotFound);
        }
        else
        {
            vacation = new EmployeeOfficialVacation();
            dbcontext.EmployeeOfficialVacations.Add(vacation);
        }

        vacation.Name = request.Name.Trim();
        vacation.StartsAt = request.StartsAt.Date;
        vacation.EndsAt = request.EndsAt.Date;
        vacation.IsRecurring = request.IsRecurring;
        vacation.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapOfficialVacation(vacation));
    }

    public async Task<Result<IEnumerable<AttendanceExcuseResponse>>> GetAttendanceExcusesAsync(int? employeeId, HumanResourceRequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeAttendanceExcuses
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var excuses = await query
            .OrderByDescending(x => x.WorkDate)
            .ThenBy(x => x.EmployeeProfile!.FullName)
            .Select(x => MapAttendanceExcuse(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<AttendanceExcuseResponse>>(excuses);
    }

    public async Task<Result<AttendanceExcuseResponse>> CreateAttendanceExcuseAsync(CreateAttendanceExcuseRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId, cancellationToken);
        if (employee is null)
            return Result.Failure<AttendanceExcuseResponse>(HumanResourceErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(request.ExcuseType) || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure<AttendanceExcuseResponse>(HumanResourceErrors.InvalidRequest);

        var excuse = new EmployeeAttendanceExcuse
        {
            EmployeeProfileId = request.EmployeeProfileId,
            WorkDate = request.WorkDate.Date,
            ExcuseType = request.ExcuseType.Trim(),
            FromTime = request.FromTime,
            ToTime = request.ToTime,
            Reason = request.Reason.Trim(),
            EmployeeProfile = employee
        };

        dbcontext.EmployeeAttendanceExcuses.Add(excuse);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AttendanceExcuse,
            excuse.Id,
            excuse.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            excuse.ExcuseType,
            null,
            excuse.Status.ToString(),
            excuse.Reason);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendanceExcuse(excuse));
    }

    public async Task<Result<AttendanceExcuseResponse>> DecideAttendanceExcuseAsync(int id, DecideHumanResourceItemRequest request, CancellationToken cancellationToken = default)
    {
        var excuse = await dbcontext.EmployeeAttendanceExcuses
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (excuse is null)
            return Result.Failure<AttendanceExcuseResponse>(HumanResourceErrors.AttendanceExcuseNotFound);

        if (excuse.Status != HumanResourceRequestStatus.Pending)
            return Result.Failure<AttendanceExcuseResponse>(HumanResourceErrors.InvalidRequest);

        var oldStatus = excuse.Status.ToString();
        excuse.Status = request.Status;
        excuse.DecisionNotes = request.Notes?.Trim();
        excuse.DecidedAt = DateTime.UtcNow.AddHours(3);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AttendanceExcuse,
            excuse.Id,
            excuse.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            excuse.ExcuseType,
            oldStatus,
            excuse.Status.ToString(),
            excuse.DecisionNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendanceExcuse(excuse));
    }

    public async Task<Result<IEnumerable<CurrentPresenceResponse>>> GetCurrentPresenceAsync(DateTime? workDate, CancellationToken cancellationToken = default)
    {
        var date = (workDate ?? DateTime.UtcNow.AddHours(3)).Date;
        var records = await dbcontext.EmployeeAttendanceRecords
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .Where(x => x.WorkDate.Date == date && x.Status != AttendanceStatus.Absent && x.CheckOut == null)
            .OrderBy(x => x.EmployeeProfile!.FullName)
            .Select(x => new CurrentPresenceResponse(
                x.EmployeeProfileId,
                x.EmployeeProfile!.FullName,
                x.WorkDate,
                x.CheckIn,
                x.CheckOut,
                x.Status.ToString(),
                x.Notes))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CurrentPresenceResponse>>(records);
    }

    public async Task<Result<IEnumerable<SafetyCategoryResponse>>> GetSafetyCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await dbcontext.HrSafetyCategories
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .Select(x => MapSafetyCategory(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SafetyCategoryResponse>>(categories);
    }

    public async Task<Result<SafetyCategoryResponse>> SaveSafetyCategoryAsync(int? id, SaveSafetyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<SafetyCategoryResponse>(HumanResourceErrors.InvalidRequest);

        HrSafetyCategory category;
        if (id.HasValue)
        {
            category = await dbcontext.HrSafetyCategories.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (category is null)
                return Result.Failure<SafetyCategoryResponse>(HumanResourceErrors.SafetyCategoryNotFound);
        }
        else
        {
            category = new HrSafetyCategory();
            dbcontext.HrSafetyCategories.Add(category);
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSafetyCategory(category));
    }

    public async Task<Result<IEnumerable<SafetyProcedureResponse>>> GetSafetyProceduresAsync(CancellationToken cancellationToken = default)
    {
        var procedures = await dbcontext.HrSafetyProcedures
            .AsNoTracking()
            .Include(x => x.HrSafetyCategory)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Title)
            .Select(x => MapSafetyProcedure(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SafetyProcedureResponse>>(procedures);
    }

    public async Task<Result<SafetyProcedureResponse>> SaveSafetyProcedureAsync(int? id, SaveSafetyProcedureRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ProcedureText))
            return Result.Failure<SafetyProcedureResponse>(HumanResourceErrors.InvalidRequest);

        if (request.HrSafetyCategoryId.HasValue)
        {
            var categoryExists = await dbcontext.HrSafetyCategories.AnyAsync(x => x.Id == request.HrSafetyCategoryId.Value, cancellationToken);
            if (!categoryExists)
                return Result.Failure<SafetyProcedureResponse>(HumanResourceErrors.SafetyCategoryNotFound);
        }

        HrSafetyProcedure procedure;
        if (id.HasValue)
        {
            procedure = await dbcontext.HrSafetyProcedures
                .Include(x => x.HrSafetyCategory)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (procedure is null)
                return Result.Failure<SafetyProcedureResponse>(HumanResourceErrors.SafetyProcedureNotFound);
        }
        else
        {
            procedure = new HrSafetyProcedure();
            dbcontext.HrSafetyProcedures.Add(procedure);
        }

        procedure.HrSafetyCategoryId = request.HrSafetyCategoryId;
        procedure.Title = request.Title.Trim();
        procedure.ProcedureText = request.ProcedureText.Trim();
        procedure.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(procedure).Reference(x => x.HrSafetyCategory).LoadAsync(cancellationToken);
        return Result.Success(MapSafetyProcedure(procedure));
    }

    public async Task<Result<IEnumerable<SafetyInspectionResponse>>> GetSafetyInspectionsAsync(SafetyRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.HrSafetyInspections
            .AsNoTracking()
            .Include(x => x.HrSafetyCategory)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var inspections = await query
            .OrderByDescending(x => x.InspectionDate)
            .Select(x => MapSafetyInspection(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SafetyInspectionResponse>>(inspections);
    }

    public async Task<Result<SafetyInspectionResponse>> SaveSafetyInspectionAsync(int? id, SaveSafetyInspectionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Location) || string.IsNullOrWhiteSpace(request.Description))
            return Result.Failure<SafetyInspectionResponse>(HumanResourceErrors.InvalidRequest);

        if (request.HrSafetyCategoryId.HasValue)
        {
            var categoryExists = await dbcontext.HrSafetyCategories.AnyAsync(x => x.Id == request.HrSafetyCategoryId.Value, cancellationToken);
            if (!categoryExists)
                return Result.Failure<SafetyInspectionResponse>(HumanResourceErrors.SafetyCategoryNotFound);
        }

        var isNew = !id.HasValue;
        HrSafetyInspection inspection;
        if (id.HasValue)
        {
            inspection = await dbcontext.HrSafetyInspections
                .Include(x => x.HrSafetyCategory)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (inspection is null)
                return Result.Failure<SafetyInspectionResponse>(HumanResourceErrors.SafetyInspectionNotFound);
        }
        else
        {
            inspection = new HrSafetyInspection();
            dbcontext.HrSafetyInspections.Add(inspection);
        }

        var oldStatus = inspection.Status.ToString();
        inspection.HrSafetyCategoryId = request.HrSafetyCategoryId;
        inspection.InspectionDate = request.InspectionDate;
        inspection.Location = request.Location.Trim();
        inspection.Description = request.Description.Trim();
        inspection.CorrectiveAction = request.CorrectiveAction?.Trim();
        inspection.Status = request.Status;

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.SafetyInspection,
            inspection.Id,
            null,
            isNew
                ? HumanResourceActivityAction.Created
                : oldStatus == inspection.Status.ToString()
                    ? HumanResourceActivityAction.Updated
                    : HumanResourceActivityAction.StatusChanged,
            inspection.Location,
            isNew ? null : oldStatus,
            inspection.Status.ToString(),
            inspection.CorrectiveAction);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(inspection).Reference(x => x.HrSafetyCategory).LoadAsync(cancellationToken);
        return Result.Success(MapSafetyInspection(inspection));
    }

    public async Task<Result<IEnumerable<RecruitmentRequestResponse>>> GetRecruitmentRequestsAsync(RecruitmentRequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.RecruitmentRequests
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .Include(x => x.ConvertedEmployeeProfile)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var requests = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapRecruitmentRequest(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<RecruitmentRequestResponse>>(requests);
    }

    public async Task<Result<RecruitmentRequestResponse>> SaveRecruitmentRequestAsync(int? id, SaveRecruitmentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestTitle) || request.RequestedCount <= 0)
            return Result.Failure<RecruitmentRequestResponse>(HumanResourceErrors.InvalidRequest);

        var lookupError = await ValidateLookupsAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (lookupError is not null)
            return Result.Failure<RecruitmentRequestResponse>(lookupError);

        var isNew = !id.HasValue;
        RecruitmentRequest entity;
        if (id.HasValue)
        {
            entity = await dbcontext.RecruitmentRequests
                .Include(x => x.Department)
                .Include(x => x.JobTitle)
                .Include(x => x.ConvertedEmployeeProfile)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;

            if (entity is null)
                return Result.Failure<RecruitmentRequestResponse>(HumanResourceErrors.RecruitmentRequestNotFound);
        }
        else
        {
            entity = new RecruitmentRequest();
            dbcontext.RecruitmentRequests.Add(entity);
        }

        entity.DepartmentId = request.DepartmentId;
        entity.JobTitleId = request.JobTitleId;
        entity.RequestTitle = request.RequestTitle.Trim();
        entity.RequestedCount = request.RequestedCount;
        entity.Justification = request.Justification?.Trim();
        entity.CandidateName = request.CandidateName?.Trim();
        entity.CandidateMobile = request.CandidateMobile?.Trim();
        entity.CandidateEmail = request.CandidateEmail?.Trim();
        entity.InterviewAt = request.InterviewAt;
        entity.InterviewNotes = request.InterviewNotes?.Trim();
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.RecruitmentRequest,
            entity.Id,
            null,
            isNew ? HumanResourceActivityAction.Created : HumanResourceActivityAction.Updated,
            entity.RequestTitle,
            null,
            entity.Status.ToString(),
            entity.Notes ?? entity.Justification);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(entity).Reference(x => x.Department).LoadAsync(cancellationToken);
        await dbcontext.Entry(entity).Reference(x => x.JobTitle).LoadAsync(cancellationToken);
        return Result.Success(MapRecruitmentRequest(entity));
    }

    public async Task<Result<RecruitmentRequestResponse>> UpdateRecruitmentStatusAsync(int id, UpdateRecruitmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.RecruitmentRequests
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .Include(x => x.ConvertedEmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return Result.Failure<RecruitmentRequestResponse>(HumanResourceErrors.RecruitmentRequestNotFound);

        var notes = request.Notes?.Trim();
        var transitionError = ValidateRecruitmentStatusTransition(entity, request, notes);
        if (transitionError is not null)
            return Result.Failure<RecruitmentRequestResponse>(transitionError);

        var oldStatus = entity.Status.ToString();
        entity.Status = request.Status;
        if (request.Status == RecruitmentRequestStatus.Announced)
            entity.AnnouncedAt = request.AnnouncedAt ?? entity.AnnouncedAt ?? DateTime.UtcNow.AddHours(3);

        if (request.Status is RecruitmentRequestStatus.Interviewed or RecruitmentRequestStatus.Completed)
        {
            entity.InterviewAt = request.InterviewAt ?? entity.InterviewAt ?? DateTime.UtcNow.AddHours(3);
            if (string.IsNullOrWhiteSpace(entity.InterviewNotes) && !string.IsNullOrWhiteSpace(notes))
                entity.InterviewNotes = notes;
        }

        if (request.Status == RecruitmentRequestStatus.Completed)
            entity.CompletedAt = request.CompletedAt ?? entity.CompletedAt ?? DateTime.UtcNow.AddHours(3);

        entity.Notes = notes ?? entity.Notes;
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.RecruitmentRequest,
            entity.Id,
            null,
            oldStatus == entity.Status.ToString() ? HumanResourceActivityAction.Updated : HumanResourceActivityAction.StatusChanged,
            entity.RequestTitle,
            oldStatus,
            entity.Status.ToString(),
            entity.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRecruitmentRequest(entity));
    }

    public async Task<Result<EmployeeResponse>> ConvertRecruitmentToEmployeeAsync(int id, ConvertRecruitmentToEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BasicSalary < 0 || request.Allowances < 0)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.InvalidRequest);

        var recruitment = await dbcontext.RecruitmentRequests
            .Include(x => x.Department)
            .Include(x => x.JobTitle)
            .Include(x => x.ConvertedEmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitment is null)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.RecruitmentRequestNotFound);
        if (recruitment.Status != RecruitmentRequestStatus.Completed)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.RecruitmentNotCompleted);
        if (recruitment.ConvertedEmployeeProfileId.HasValue)
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.RecruitmentAlreadyConverted);
        if (string.IsNullOrWhiteSpace(recruitment.CandidateName))
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.RecruitmentCandidateRequired);

        var lookupError = await ValidateLookupsAsync(recruitment.DepartmentId, recruitment.JobTitleId, cancellationToken);
        if (lookupError is not null)
            return Result.Failure<EmployeeResponse>(lookupError);

        var employeeNumber = string.IsNullOrWhiteSpace(request.EmployeeNumber)
            ? await GenerateEmployeeNumberAsync(cancellationToken)
            : request.EmployeeNumber.Trim();

        if (await dbcontext.EmployeeProfiles.AnyAsync(x => x.EmployeeNumber == employeeNumber, cancellationToken))
            return Result.Failure<EmployeeResponse>(HumanResourceErrors.DuplicateEmployeeNumber);

        var noteParts = new[]
        {
            $"تم إنشاء ملف الموظف من طلب التوظيف #{recruitment.Id}.",
            recruitment.RequestTitle,
            request.Notes?.Trim()
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var employee = new EmployeeProfile
        {
            EmployeeNumber = employeeNumber,
            FullName = recruitment.CandidateName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Email = recruitment.CandidateEmail?.Trim(),
            Mobile = recruitment.CandidateMobile?.Trim(),
            DepartmentId = recruitment.DepartmentId,
            JobTitleId = recruitment.JobTitleId,
            AccountType = EmployeeAccountType.Employee,
            HireDate = request.HireDate ?? recruitment.CompletedAt ?? DateTime.UtcNow.AddHours(3),
            BasicSalary = request.BasicSalary,
            Allowances = request.Allowances,
            Notes = string.Join(" ", noteParts)
        };

        dbcontext.EmployeeProfiles.Add(employee);
        await dbcontext.SaveChangesAsync(cancellationToken);

        recruitment.ConvertedEmployeeProfileId = employee.Id;
        recruitment.ConvertedEmployeeProfile = employee;
        recruitment.Notes = string.IsNullOrWhiteSpace(recruitment.Notes)
            ? $"تم تحويل الطلب إلى ملف موظف رقم {employee.EmployeeNumber}."
            : $"{recruitment.Notes.Trim()} | تم تحويل الطلب إلى ملف موظف رقم {employee.EmployeeNumber}.";

        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.EmployeeProfile,
            employee.Id,
            employee.Id,
            HumanResourceActivityAction.Created,
            employee.FullName,
            null,
            employee.Status.ToString(),
            $"إنشاء من طلب التوظيف: {recruitment.RequestTitle}");
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.RecruitmentRequest,
            recruitment.Id,
            employee.Id,
            HumanResourceActivityAction.Updated,
            recruitment.RequestTitle,
            recruitment.Status.ToString(),
            recruitment.Status.ToString(),
            $"تم إنشاء ملف الموظف {employee.EmployeeNumber}.");

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.Department).LoadAsync(cancellationToken);
        await dbcontext.Entry(employee).Reference(x => x.JobTitle).LoadAsync(cancellationToken);
        return Result.Success(MapEmployee(employee));
    }

    public async Task<Result<IEnumerable<EmployeeAdministrativeRequestResponse>>> GetAdministrativeRequestsAsync(int? employeeId, HumanResourceRequestStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmployeeAdministrativeRequests
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var requests = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapAdministrativeRequest(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmployeeAdministrativeRequestResponse>>(requests);
    }

    public async Task<Result<EmployeeAdministrativeRequestResponse>> CreateAdministrativeRequestAsync(CreateEmployeeAdministrativeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestType) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Details))
            return Result.Failure<EmployeeAdministrativeRequestResponse>(HumanResourceErrors.InvalidRequest);

        EmployeeProfile? employee = null;
        if (request.EmployeeProfileId.HasValue)
        {
            employee = await dbcontext.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == request.EmployeeProfileId.Value, cancellationToken);
            if (employee is null)
                return Result.Failure<EmployeeAdministrativeRequestResponse>(HumanResourceErrors.EmployeeNotFound);
        }

        var entity = new EmployeeAdministrativeRequest
        {
            EmployeeProfileId = request.EmployeeProfileId,
            EmployeeProfile = employee,
            RequestType = request.RequestType.Trim(),
            Title = request.Title.Trim(),
            Details = request.Details.Trim()
        };

        dbcontext.EmployeeAdministrativeRequests.Add(entity);
        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AdministrativeRequest,
            entity.Id,
            entity.EmployeeProfileId,
            HumanResourceActivityAction.Created,
            entity.Title,
            null,
            entity.Status.ToString(),
            entity.Details);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAdministrativeRequest(entity));
    }

    public async Task<Result<EmployeeAdministrativeRequestResponse>> DecideAdministrativeRequestAsync(int id, DecideHumanResourceItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.EmployeeAdministrativeRequests
            .Include(x => x.EmployeeProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return Result.Failure<EmployeeAdministrativeRequestResponse>(HumanResourceErrors.AdministrativeRequestNotFound);

        var validTransition = entity.Status switch
        {
            HumanResourceRequestStatus.Pending => request.Status is HumanResourceRequestStatus.Approved or HumanResourceRequestStatus.Rejected or HumanResourceRequestStatus.Cancelled,
            HumanResourceRequestStatus.Approved => request.Status is HumanResourceRequestStatus.Completed or HumanResourceRequestStatus.Cancelled,
            _ => false
        };
        if (!validTransition)
            return Result.Failure<EmployeeAdministrativeRequestResponse>(HumanResourceErrors.InvalidRequest);

        var oldStatus = entity.Status.ToString();
        entity.Status = request.Status;
        entity.DecisionNotes = request.Notes?.Trim();
        entity.DecidedAt = DateTime.UtcNow.AddHours(3);
        QueueHumanResourceActivity(
            HumanResourceActivityEntityType.AdministrativeRequest,
            entity.Id,
            entity.EmployeeProfileId,
            HumanResourceActivityAction.StatusChanged,
            entity.Title,
            oldStatus,
            entity.Status.ToString(),
            entity.DecisionNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAdministrativeRequest(entity));
    }

    public async Task<Result<IEnumerable<HumanResourceActivityResponse>>> GetActivitiesAsync(HumanResourceActivityEntityType? entityType, int? entityId, int? employeeId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.HumanResourceActivities
            .AsNoTracking()
            .Include(x => x.EmployeeProfile)
            .AsQueryable();

        if (entityType.HasValue)
            query = query.Where(x => x.EntityType == entityType.Value);

        if (entityId.HasValue)
            query = query.Where(x => x.EntityId == entityId.Value);

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeProfileId == employeeId.Value);

        var activities = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Take(100)
            .Select(x => MapHumanResourceActivity(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<HumanResourceActivityResponse>>(activities);
    }

    private static Error? ValidateRecruitmentStatusTransition(RecruitmentRequest entity, UpdateRecruitmentStatusRequest request, string? notes)
    {
        if (entity.Status == request.Status)
            return null;

        if (entity.Status is RecruitmentRequestStatus.Completed or RecruitmentRequestStatus.Cancelled)
            return HumanResourceErrors.RecruitmentAlreadyClosed;

        if (request.Status == RecruitmentRequestStatus.Cancelled)
            return string.IsNullOrWhiteSpace(notes)
                ? HumanResourceErrors.RecruitmentCancellationNotesRequired
                : null;

        return request.Status switch
        {
            RecruitmentRequestStatus.Announced when entity.Status == RecruitmentRequestStatus.Requested => null,
            RecruitmentRequestStatus.Received when entity.Status == RecruitmentRequestStatus.Announced =>
                HasRecruitmentCandidateInfo(entity)
                    ? null
                    : HumanResourceErrors.RecruitmentCandidateRequired,
            RecruitmentRequestStatus.Interviewed when entity.Status == RecruitmentRequestStatus.Received =>
                ValidateRecruitmentInterviewStep(entity, notes),
            RecruitmentRequestStatus.Completed when entity.Status == RecruitmentRequestStatus.Interviewed =>
                ValidateRecruitmentCompletionStep(entity, request, notes),
            _ => HumanResourceErrors.InvalidRecruitmentStatusTransition
        };
    }

    private static Error? ValidateRecruitmentInterviewStep(RecruitmentRequest entity, string? notes)
    {
        if (!HasRecruitmentCandidateInfo(entity))
            return HumanResourceErrors.RecruitmentCandidateRequired;

        return HasRecruitmentInterviewNotes(entity, notes)
            ? null
            : HumanResourceErrors.RecruitmentInterviewRequired;
    }

    private static Error? ValidateRecruitmentCompletionStep(RecruitmentRequest entity, UpdateRecruitmentStatusRequest request, string? notes)
    {
        if (!HasRecruitmentCandidateInfo(entity))
            return HumanResourceErrors.RecruitmentCandidateRequired;

        return (entity.InterviewAt.HasValue || request.InterviewAt.HasValue) && HasRecruitmentInterviewNotes(entity, notes)
            ? null
            : HumanResourceErrors.RecruitmentInterviewRequired;
    }

    private static bool HasRecruitmentCandidateInfo(RecruitmentRequest entity) =>
        !string.IsNullOrWhiteSpace(entity.CandidateName) &&
        (!string.IsNullOrWhiteSpace(entity.CandidateMobile) || !string.IsNullOrWhiteSpace(entity.CandidateEmail));

    private static bool HasRecruitmentInterviewNotes(RecruitmentRequest entity, string? notes) =>
        !string.IsNullOrWhiteSpace(entity.InterviewNotes) || !string.IsNullOrWhiteSpace(notes);

    private static Error? ValidateEvaluationDecision(EmployeeEvaluation evaluation, EmployeeEvaluationStatus status, string? notes)
    {
        if (evaluation.Status != EmployeeEvaluationStatus.PendingApproval)
            return HumanResourceErrors.InvalidEvaluationStatusTransition;

        if (status == EmployeeEvaluationStatus.Rejected && string.IsNullOrWhiteSpace(notes))
            return HumanResourceErrors.EvaluationDecisionNotesRequired;

        return status is EmployeeEvaluationStatus.Approved or EmployeeEvaluationStatus.Rejected
            ? null
            : HumanResourceErrors.InvalidEvaluationStatusTransition;
    }

    private static Error? ValidateCardIssueDecision(EmployeeCardIssue card, HumanResourceRequestStatus status, string? notes)
    {
        if (card.Status is HumanResourceRequestStatus.Completed or HumanResourceRequestStatus.Cancelled or HumanResourceRequestStatus.Rejected)
            return HumanResourceErrors.CardIssueAlreadyClosed;

        if (status == HumanResourceRequestStatus.Cancelled && string.IsNullOrWhiteSpace(notes))
            return HumanResourceErrors.CardIssueDecisionNotesRequired;

        return status is HumanResourceRequestStatus.Approved or HumanResourceRequestStatus.Completed or HumanResourceRequestStatus.Cancelled
            ? null
            : HumanResourceErrors.InvalidCardIssueStatusTransition;
    }

    private async Task<Error?> ValidateLookupsAsync(int departmentId, int jobTitleId, CancellationToken cancellationToken)
    {
        var departmentExists = await dbcontext.EmployeeDepartments.AnyAsync(x => x.Id == departmentId && x.IsActive, cancellationToken);
        if (!departmentExists)
            return HumanResourceErrors.DepartmentNotFound;

        var jobTitleExists = await dbcontext.JobTitles.AnyAsync(x => x.Id == jobTitleId && x.IsActive, cancellationToken);
        return jobTitleExists ? null : HumanResourceErrors.JobTitleNotFound;
    }

    private async Task<string> GenerateEmployeeNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.EmployeeProfiles.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"E-{year}-{count:0000}";
    }

    private async Task<string> GenerateCardNumberAsync(string cardType, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var prefix = cardType.Contains("تستاهل", StringComparison.OrdinalIgnoreCase) || cardType.Contains("testahel", StringComparison.OrdinalIgnoreCase)
            ? "TST"
            : "EMP";
        var count = await dbcontext.EmployeeCardIssues.CountAsync(x => x.IssuedAt.Year == year, cancellationToken) + 1;
        return $"{prefix}-{year}-{count:0000}";
    }

    private void QueueHumanResourceActivity(
        HumanResourceActivityEntityType entityType,
        int entityId,
        int? employeeProfileId,
        HumanResourceActivityAction action,
        string title,
        string? fromStatus,
        string? toStatus,
        string? notes)
    {
        dbcontext.HumanResourceActivities.Add(new HumanResourceActivity
        {
            EntityType = entityType,
            EntityId = entityId,
            EmployeeProfileId = employeeProfileId,
            Action = action,
            Title = string.IsNullOrWhiteSpace(title) ? entityType.ToString() : title.Trim(),
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OccurredAt = DateTime.UtcNow.AddHours(3)
        });
    }

    private async Task ApplyApprovedLeaveBalanceAsync(EmployeeLeaveRequest leave, CancellationToken cancellationToken)
    {
        var year = leave.StartsAt.Year;
        var days = GetLeaveDays(leave);
        var balance = await dbcontext.EmployeeLeaveBalances.FirstOrDefaultAsync(x =>
            x.EmployeeProfileId == leave.EmployeeProfileId &&
            x.Year == year &&
            x.LeaveType == leave.LeaveType,
            cancellationToken);

        if (balance is not null)
            balance.UsedDays += days;
    }

    private async Task<Error?> ValidateApprovedLeaveBalanceAsync(EmployeeLeaveRequest leave, CancellationToken cancellationToken)
    {
        var year = leave.StartsAt.Year;
        var balance = await dbcontext.EmployeeLeaveBalances.FirstOrDefaultAsync(x =>
            x.EmployeeProfileId == leave.EmployeeProfileId &&
            x.Year == year &&
            x.LeaveType == leave.LeaveType,
            cancellationToken);

        if (balance is null)
            return null;

        return GetLeaveDays(leave) > balance.EntitledDays + balance.CarriedDays - balance.UsedDays
            ? HumanResourceErrors.InsufficientLeaveBalance
            : null;
    }

    private static int GetLeaveDays(EmployeeLeaveRequest leave) => (leave.EndsAt.Date - leave.StartsAt.Date).Days + 1;

    private async Task<AttendanceStatus> ResolveAttendanceStatusAsync(RecordEmployeeAttendanceRequest request, CancellationToken cancellationToken)
    {
        if (request.Status != AttendanceStatus.Present || !request.CheckIn.HasValue)
            return request.Status;

        var policy = await dbcontext.EmployeeAttendancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && x.IsDefault, cancellationToken);
        if (policy is null || !IsPolicyWorkDay(policy, request.WorkDate))
            return request.Status;

        var latestOnTime = policy.WorkStart.Add(TimeSpan.FromMinutes(policy.GraceMinutes));
        return request.CheckIn.Value > latestOnTime ? AttendanceStatus.Late : request.Status;
    }

    private static bool IsPolicyWorkDay(EmployeeAttendancePolicy policy, DateTime workDate)
    {
        var day = workDate.DayOfWeek.ToString();
        return policy.WorkDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, day, StringComparison.OrdinalIgnoreCase));
    }

    private static DateTime FirstDayOfMonth(DateTime value) => new(value.Year, value.Month, 1);

    private static EmployeeDepartmentResponse MapDepartment(EmployeeDepartment department) =>
        new(department.Id, department.NameAr, department.NameEn, department.IsActive);

    private static JobTitleResponse MapJobTitle(JobTitle title) =>
        new(title.Id, title.NameAr, title.NameEn, title.IsActive);

    private static EmployeeResponse MapEmployee(EmployeeProfile employee) =>
        new(
            employee.Id,
            employee.EmployeeNumber,
            employee.FullName,
            employee.NationalId,
            employee.Email,
            employee.Mobile,
            employee.DepartmentId,
            employee.Department?.NameAr ?? string.Empty,
            employee.JobTitleId,
            employee.JobTitle?.NameAr ?? string.Empty,
            employee.HireDate,
            employee.Status.ToString(),
            employee.BasicSalary,
            employee.Allowances,
            employee.BasicSalary + employee.Allowances,
            employee.Notes,
            employee.TerminatedAt,
            employee.TerminationReason,
            employee.CreatedAt,
            employee.AccountType.ToString());

    private static EmployeeAttendanceResponse MapAttendance(EmployeeAttendance attendance) =>
        new(
            attendance.Id,
            attendance.EmployeeProfileId,
            attendance.EmployeeProfile?.FullName ?? string.Empty,
            attendance.WorkDate,
            attendance.CheckIn,
            attendance.CheckOut,
            attendance.Status.ToString(),
            attendance.Notes);

    private static EmployeeLeaveRequestResponse MapLeave(EmployeeLeaveRequest leave) =>
        new(
            leave.Id,
            leave.EmployeeProfileId,
            leave.EmployeeProfile?.FullName ?? string.Empty,
            leave.LeaveType,
            leave.StartsAt,
            leave.EndsAt,
            (leave.EndsAt.Date - leave.StartsAt.Date).Days + 1,
            leave.Status.ToString(),
            leave.Reason,
            leave.DecisionNotes,
            leave.DecidedAt,
            leave.CreatedAt);

    private static EmployeeDocumentResponse MapDocument(EmployeeDocument document) =>
        new(
            document.Id,
            document.EmployeeProfileId,
            document.EmployeeProfile?.FullName ?? string.Empty,
            document.Title,
            document.DocumentType,
            document.FilePath,
            document.ExpiresAt,
            document.Notes);

    private static EmployeeDisciplinaryRecordResponse MapDisciplinaryRecord(EmployeeDisciplinaryRecord record) =>
        new(
            record.Id,
            record.EmployeeProfileId,
            record.EmployeeProfile?.FullName ?? string.Empty,
            record.Type.ToString(),
            record.RecordDate,
            record.Title,
            record.Reason,
            record.ActionTaken,
            record.Status.ToString(),
            record.DecisionNotes,
            record.DecidedAt);

    private static EmployeeLeaveBalanceResponse MapLeaveBalance(EmployeeLeaveBalance balance) =>
        new(
            balance.Id,
            balance.EmployeeProfileId,
            balance.EmployeeProfile?.FullName ?? string.Empty,
            balance.Year,
            balance.LeaveType,
            balance.EntitledDays,
            balance.UsedDays,
            balance.CarriedDays,
            balance.EntitledDays + balance.CarriedDays - balance.UsedDays,
            balance.Notes);

    private static EmployeeEvaluationResponse MapEvaluation(EmployeeEvaluation evaluation) =>
        new(
            evaluation.Id,
            evaluation.EmployeeProfileId,
            evaluation.EmployeeProfile?.FullName ?? string.Empty,
            evaluation.PeriodStart,
            evaluation.PeriodEnd,
            evaluation.Score,
            evaluation.MaxScore,
            evaluation.MaxScore == 0 ? 0 : Math.Round(evaluation.Score / evaluation.MaxScore * 100, 2),
            evaluation.Rating,
            evaluation.EvaluatorName,
            evaluation.Strengths,
            evaluation.ImprovementAreas,
            evaluation.Status.ToString(),
            evaluation.DecisionNotes,
            evaluation.DecidedAt,
            evaluation.Notes);

    private static EmployeeCardIssueResponse MapCardIssue(EmployeeCardIssue card) =>
        new(
            card.Id,
            card.EmployeeProfileId,
            card.EmployeeProfile?.FullName ?? string.Empty,
            card.CardType,
            card.CardNumber,
            card.IssuedAt,
            card.ExpiresAt,
            card.Status.ToString(),
            card.Notes);

    private static EmployeeLetterRequestResponse MapLetterRequest(EmployeeLetterRequest letter) =>
        new(
            letter.Id,
            letter.EmployeeProfileId,
            letter.EmployeeProfile?.FullName,
            letter.LetterType,
            letter.Subject,
            letter.Purpose,
            letter.Body,
            letter.Status.ToString(),
            letter.IssuedAt,
            letter.DecisionNotes);

    private static EmployeePayrollRecordResponse MapPayrollRecord(EmployeePayrollRecord record) =>
        new(
            record.Id,
            record.EmployeeProfileId,
            record.EmployeeProfile?.FullName ?? string.Empty,
            record.PayrollMonth,
            record.BasicSalary,
            record.Allowances,
            record.Deductions,
            record.NetSalary,
            record.Status.ToString(),
            record.Notes,
            record.DecisionNotes,
            record.ReviewedAt,
            record.ApprovedAt,
            record.PaidAt);

    private static bool IsValidPayrollTransition(PayrollRecordStatus fromStatus, PayrollRecordStatus toStatus) =>
        (fromStatus, toStatus) switch
        {
            (PayrollRecordStatus.Draft, PayrollRecordStatus.Reviewed) => true,
            (PayrollRecordStatus.Reviewed, PayrollRecordStatus.Approved) => true,
            (PayrollRecordStatus.Approved, PayrollRecordStatus.Paid) => true,
            _ => false
        };

    private static AttendancePolicyResponse MapAttendancePolicy(EmployeeAttendancePolicy policy) =>
        new(policy.Id, policy.Name, policy.WorkStart, policy.WorkEnd, policy.GraceMinutes, policy.WorkDays, policy.IsDefault, policy.IsActive);

    private static AttendanceLocationResponse MapAttendanceLocation(EmployeeAttendanceLocation location) =>
        new(location.Id, location.Name, location.Latitude, location.Longitude, location.RadiusMeters, location.IsActive, location.Notes);

    private static OfficialVacationResponse MapOfficialVacation(EmployeeOfficialVacation vacation) =>
        new(vacation.Id, vacation.Name, vacation.StartsAt, vacation.EndsAt, (vacation.EndsAt.Date - vacation.StartsAt.Date).Days + 1, vacation.IsRecurring, vacation.Notes);

    private static AttendanceExcuseResponse MapAttendanceExcuse(EmployeeAttendanceExcuse excuse) =>
        new(
            excuse.Id,
            excuse.EmployeeProfileId,
            excuse.EmployeeProfile?.FullName ?? string.Empty,
            excuse.WorkDate,
            excuse.ExcuseType,
            excuse.FromTime,
            excuse.ToTime,
            excuse.Reason,
            excuse.Status.ToString(),
            excuse.DecisionNotes,
            excuse.DecidedAt);

    private static SafetyCategoryResponse MapSafetyCategory(HrSafetyCategory category) =>
        new(category.Id, category.Name, category.Description, category.IsActive);

    private static SafetyProcedureResponse MapSafetyProcedure(HrSafetyProcedure procedure) =>
        new(procedure.Id, procedure.HrSafetyCategoryId, procedure.HrSafetyCategory?.Name, procedure.Title, procedure.ProcedureText, procedure.IsActive);

    private static SafetyInspectionResponse MapSafetyInspection(HrSafetyInspection inspection) =>
        new(
            inspection.Id,
            inspection.HrSafetyCategoryId,
            inspection.HrSafetyCategory?.Name,
            inspection.InspectionDate,
            inspection.Location,
            inspection.Description,
            inspection.CorrectiveAction,
            inspection.Status.ToString());

    private static RecruitmentRequestResponse MapRecruitmentRequest(RecruitmentRequest request) =>
        new(
            request.Id,
            request.DepartmentId,
            request.Department?.NameAr ?? string.Empty,
            request.JobTitleId,
            request.JobTitle?.NameAr ?? string.Empty,
            request.RequestTitle,
            request.RequestedCount,
            request.Justification,
            request.Status.ToString(),
            request.AnnouncedAt,
            request.CandidateName,
            request.CandidateMobile,
            request.CandidateEmail,
            request.InterviewAt,
            request.InterviewNotes,
            request.CompletedAt,
            request.ConvertedEmployeeProfileId,
            request.ConvertedEmployeeProfile?.FullName,
            request.Notes);

    private static EmployeeAdministrativeRequestResponse MapAdministrativeRequest(EmployeeAdministrativeRequest request) =>
        new(
            request.Id,
            request.EmployeeProfileId,
            request.EmployeeProfile?.FullName,
            request.RequestType,
            request.Title,
            request.Details,
            request.Status.ToString(),
            request.DecisionNotes,
            request.DecidedAt);

    private static HumanResourceActivityResponse MapHumanResourceActivity(HumanResourceActivity activity) =>
        new(
            activity.Id,
            activity.EntityType.ToString(),
            activity.EntityId,
            activity.EmployeeProfileId,
            activity.EmployeeProfile?.FullName,
            activity.Action.ToString(),
            activity.Title,
            activity.FromStatus,
            activity.ToStatus,
            activity.Notes,
            activity.OccurredAt);
}
