using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Volunteering;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Volunteering;

public class VolunteeringService(ApplicationDbcontext dbcontext) : IVolunteeringService
{
    public async Task<Result<VolunteeringDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        Result.Success(new VolunteeringDashboardResponse(
            await dbcontext.VolunteerUsers.CountAsync(cancellationToken),
            await dbcontext.VolunteerUsers.CountAsync(x => x.Status == VolunteerUserStatus.Active, cancellationToken),
            await dbcontext.VolunteerRequests.CountAsync(x => x.Status == VolunteerRequestStatus.Submitted || x.Status == VolunteerRequestStatus.UnderReview, cancellationToken),
            await dbcontext.VolunteerRequests.CountAsync(x => x.Status == VolunteerRequestStatus.Approved, cancellationToken),
            await dbcontext.VolunteerOpportunities.CountAsync(x => x.Status == VolunteerOpportunityStatus.Open || x.Status == VolunteerOpportunityStatus.InProgress, cancellationToken),
            await dbcontext.VolunteerOpportunityTasks.CountAsync(x => x.Status == VolunteerTaskStatus.Completed, cancellationToken),
            await dbcontext.VolunteerAttendanceRecords.SumAsync(x => (decimal?)x.Hours, cancellationToken) ?? 0));

    public async Task<Result<IEnumerable<VolunteerUserResponse>>> GetUsersAsync(VolunteerUserStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VolunteerUsers.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var users = await query.OrderBy(x => x.FullName).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<VolunteerUserResponse>>(users.Select(MapUser).ToList());
    }

    public async Task<Result<VolunteerUserResponse>> SaveUserAsync(int? id, SaveVolunteerUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.VolunteerNumber) || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Mobile))
            return Result.Failure<VolunteerUserResponse>(VolunteeringErrors.InvalidRequest);

        var volunteerNumber = request.VolunteerNumber.Trim();
        if (await dbcontext.VolunteerUsers.AnyAsync(x => x.VolunteerNumber == volunteerNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<VolunteerUserResponse>(VolunteeringErrors.DuplicateVolunteerNumber);

        var entity = id.HasValue
            ? await dbcontext.VolunteerUsers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VolunteerUser();
        if (entity is null)
            return Result.Failure<VolunteerUserResponse>(VolunteeringErrors.VolunteerUserNotFound);
        if (!id.HasValue) dbcontext.VolunteerUsers.Add(entity);

        entity.VolunteerNumber = volunteerNumber;
        entity.FullName = request.FullName.Trim();
        entity.NationalId = TrimOrNull(request.NationalId);
        entity.Mobile = request.Mobile.Trim();
        entity.Email = TrimOrNull(request.Email);
        entity.Skills = TrimOrNull(request.Skills);
        entity.Status = request.Status;
        entity.JoinedAt = request.JoinedAt;
        entity.Notes = TrimOrNull(request.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapUser(entity));
    }

    public async Task<Result<IEnumerable<VolunteerRequestResponse>>> GetRequestsAsync(VolunteerRequestSource? source = null, VolunteerRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VolunteerRequests
            .AsNoTracking()
            .Include(x => x.VolunteerUser)
            .Include(x => x.VolunteerOpportunity)
            .AsQueryable();
        if (source.HasValue) query = query.Where(x => x.Source == source.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var requests = await query.OrderByDescending(x => x.RequestDate).ThenBy(x => x.RequestNumber).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<VolunteerRequestResponse>>(requests.Select(MapRequest).ToList());
    }

    public async Task<Result<VolunteerRequestResponse>> SaveRequestAsync(int? id, SaveVolunteerRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestNumber) || string.IsNullOrWhiteSpace(request.ApplicantName) || string.IsNullOrWhiteSpace(request.Mobile))
            return Result.Failure<VolunteerRequestResponse>(VolunteeringErrors.InvalidRequest);

        if (request.VolunteerUserId.HasValue && !await dbcontext.VolunteerUsers.AnyAsync(x => x.Id == request.VolunteerUserId.Value, cancellationToken))
            return Result.Failure<VolunteerRequestResponse>(VolunteeringErrors.VolunteerUserNotFound);
        if (request.VolunteerOpportunityId.HasValue && !await dbcontext.VolunteerOpportunities.AnyAsync(x => x.Id == request.VolunteerOpportunityId.Value, cancellationToken))
            return Result.Failure<VolunteerRequestResponse>(VolunteeringErrors.OpportunityNotFound);

        var requestNumber = request.RequestNumber.Trim();
        if (await dbcontext.VolunteerRequests.AnyAsync(x => x.RequestNumber == requestNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<VolunteerRequestResponse>(VolunteeringErrors.DuplicateRequestNumber);

        var entity = id.HasValue
            ? await dbcontext.VolunteerRequests.Include(x => x.VolunteerUser).Include(x => x.VolunteerOpportunity).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VolunteerRequest();
        if (entity is null)
            return Result.Failure<VolunteerRequestResponse>(VolunteeringErrors.VolunteerRequestNotFound);
        if (!id.HasValue) dbcontext.VolunteerRequests.Add(entity);

        entity.RequestNumber = requestNumber;
        entity.Source = request.Source;
        entity.ApplicantName = request.ApplicantName.Trim();
        entity.Mobile = request.Mobile.Trim();
        entity.OpportunityTitle = TrimOrNull(request.OpportunityTitle);
        entity.Status = request.Status;
        entity.RequestDate = request.RequestDate;
        entity.DecisionNote = TrimOrNull(request.DecisionNote);
        entity.Notes = TrimOrNull(request.Notes);
        entity.VolunteerUserId = request.VolunteerUserId;
        entity.VolunteerOpportunityId = request.VolunteerOpportunityId;

        await dbcontext.SaveChangesAsync(cancellationToken);
        entity.VolunteerUser ??= request.VolunteerUserId.HasValue ? await dbcontext.VolunteerUsers.FirstOrDefaultAsync(x => x.Id == request.VolunteerUserId.Value, cancellationToken) : null;
        entity.VolunteerOpportunity ??= request.VolunteerOpportunityId.HasValue ? await dbcontext.VolunteerOpportunities.FirstOrDefaultAsync(x => x.Id == request.VolunteerOpportunityId.Value, cancellationToken) : null;
        return Result.Success(MapRequest(entity));
    }

    public async Task<Result> UpdateRequestStatusAsync(int id, UpdateVolunteerRequestStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.VolunteerRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure(VolunteeringErrors.VolunteerRequestNotFound);

        entity.Status = request.Status;
        entity.DecisionNote = TrimOrNull(request.DecisionNote) ?? entity.DecisionNote;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<VolunteerRequestConversionResponse>> ConvertRequestToVolunteerAsync(int id, ConvertVolunteerRequestRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.VolunteerRequests
            .Include(x => x.VolunteerUser)
            .Include(x => x.VolunteerOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.VolunteerRequestNotFound);
        if (entity.Status == VolunteerRequestStatus.Rejected)
            return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.InvalidRequest);

        VolunteerOpportunity? opportunity = null;
        var opportunityId = request.VolunteerOpportunityId ?? entity.VolunteerOpportunityId;
        if (opportunityId.HasValue)
        {
            opportunity = await dbcontext.VolunteerOpportunities.FirstOrDefaultAsync(x => x.Id == opportunityId.Value, cancellationToken);
            if (opportunity is null)
                return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.OpportunityNotFound);
            if (opportunity.Status is not (VolunteerOpportunityStatus.Open or VolunteerOpportunityStatus.InProgress))
                return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.OpportunityNotOpen);
            if (await IsOpportunityFullAsync(opportunity.Id, opportunity.Seats, entity.Id, cancellationToken))
                return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.OpportunityCapacityReached);
        }

        var volunteer = entity.VolunteerUser;
        if (volunteer is null)
            volunteer = await dbcontext.VolunteerUsers.FirstOrDefaultAsync(x => x.Mobile == entity.Mobile, cancellationToken);

        if (volunteer is null)
        {
            var volunteerNumber = string.IsNullOrWhiteSpace(request.VolunteerNumber)
                ? await GenerateVolunteerNumberAsync(cancellationToken)
                : request.VolunteerNumber.Trim();

            if (await dbcontext.VolunteerUsers.AnyAsync(x => x.VolunteerNumber == volunteerNumber, cancellationToken))
                return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.DuplicateVolunteerNumber);

            volunteer = new VolunteerUser
            {
                VolunteerNumber = volunteerNumber,
                FullName = entity.ApplicantName,
                Mobile = entity.Mobile,
                Skills = TrimOrNull(request.Skills),
                Status = VolunteerUserStatus.Active,
                JoinedAt = (request.JoinedAt ?? DateTime.UtcNow.AddHours(3)).Date,
                Notes = TrimOrNull(request.Notes)
            };
            dbcontext.VolunteerUsers.Add(volunteer);
        }
        else
        {
            if (volunteer.Status == VolunteerUserStatus.Suspended)
                return Result.Failure<VolunteerRequestConversionResponse>(VolunteeringErrors.InvalidRequest);

            volunteer.Status = VolunteerUserStatus.Active;
            volunteer.Skills = TrimOrNull(request.Skills) ?? volunteer.Skills;
            volunteer.Notes = TrimOrNull(request.Notes) ?? volunteer.Notes;
        }

        entity.VolunteerUser = volunteer;
        if (volunteer.Id > 0)
            entity.VolunteerUserId = volunteer.Id;

        if (opportunity is not null)
        {
            entity.VolunteerOpportunity = opportunity;
            entity.VolunteerOpportunityId = opportunity.Id;
            entity.OpportunityTitle = TrimOrNull(entity.OpportunityTitle) ?? opportunity.Title;
        }

        entity.Status = VolunteerRequestStatus.Approved;
        entity.DecisionNote = TrimOrNull(request.DecisionNote) ?? entity.DecisionNote ?? "Converted to volunteer account.";
        entity.Notes = TrimOrNull(request.Notes) ?? entity.Notes;

        await dbcontext.SaveChangesAsync(cancellationToken);

        var savedRequest = await dbcontext.VolunteerRequests
            .AsNoTracking()
            .Include(x => x.VolunteerUser)
            .Include(x => x.VolunteerOpportunity)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);
        var savedVolunteer = await dbcontext.VolunteerUsers.AsNoTracking().FirstAsync(x => x.Id == savedRequest.VolunteerUserId!.Value, cancellationToken);
        VolunteerOpportunityResponse? savedOpportunity = null;
        if (savedRequest.VolunteerOpportunityId.HasValue)
        {
            var savedOpportunityEntity = await dbcontext.VolunteerOpportunities
                .AsNoTracking()
                .Include(x => x.Requests)
                .Include(x => x.Tasks)
                .Include(x => x.AttendanceRecords)
                .FirstAsync(x => x.Id == savedRequest.VolunteerOpportunityId.Value, cancellationToken);
            savedOpportunity = MapOpportunity(savedOpportunityEntity);
        }

        return Result.Success(new VolunteerRequestConversionResponse(MapRequest(savedRequest), MapUser(savedVolunteer), savedOpportunity));
    }

    public async Task<Result<IEnumerable<VolunteerOpportunityResponse>>> GetOpportunitiesAsync(VolunteerOpportunityStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VolunteerOpportunities
            .AsNoTracking()
            .Include(x => x.Requests)
            .Include(x => x.Tasks)
            .Include(x => x.AttendanceRecords)
            .AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var opportunities = await query.OrderByDescending(x => x.StartDate).ThenBy(x => x.Title).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<VolunteerOpportunityResponse>>(opportunities.Select(MapOpportunity).ToList());
    }

    public async Task<Result<VolunteerOpportunityResponse>> SaveOpportunityAsync(int? id, SaveVolunteerOpportunityRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OpportunityNumber) || string.IsNullOrWhiteSpace(request.Title) || request.Seats < 0)
            return Result.Failure<VolunteerOpportunityResponse>(VolunteeringErrors.InvalidRequest);

        var opportunityNumber = request.OpportunityNumber.Trim();
        if (await dbcontext.VolunteerOpportunities.AnyAsync(x => x.OpportunityNumber == opportunityNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<VolunteerOpportunityResponse>(VolunteeringErrors.DuplicateOpportunityNumber);

        var entity = id.HasValue
            ? await dbcontext.VolunteerOpportunities.Include(x => x.Requests).Include(x => x.Tasks).Include(x => x.AttendanceRecords).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VolunteerOpportunity();
        if (entity is null)
            return Result.Failure<VolunteerOpportunityResponse>(VolunteeringErrors.OpportunityNotFound);
        if (!id.HasValue) dbcontext.VolunteerOpportunities.Add(entity);

        entity.OpportunityNumber = opportunityNumber;
        entity.Title = request.Title.Trim();
        entity.Description = TrimOrNull(request.Description);
        entity.Department = TrimOrNull(request.Department);
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Seats = request.Seats;
        entity.Status = request.Status;
        entity.ProcedureNotes = TrimOrNull(request.ProcedureNotes);
        entity.ReportSummary = TrimOrNull(request.ReportSummary);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapOpportunity(entity));
    }

    public async Task<Result> SaveOpportunityReportAsync(int id, SaveVolunteerOpportunityReportRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.VolunteerOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure(VolunteeringErrors.OpportunityNotFound);

        if (request.Status == VolunteerOpportunityStatus.Completed)
        {
            var hasOpenTasks = await dbcontext.VolunteerOpportunityTasks.AnyAsync(x =>
                x.VolunteerOpportunityId == id &&
                x.Status != VolunteerTaskStatus.Completed &&
                x.Status != VolunteerTaskStatus.Cancelled,
                cancellationToken);
            if (hasOpenTasks)
                return Result.Failure(VolunteeringErrors.OpportunityHasOpenTasks);

            var hasPresentAttendance = await dbcontext.VolunteerAttendanceRecords.AnyAsync(x =>
                x.VolunteerOpportunityId == id &&
                x.Status == VolunteerAttendanceStatus.Present &&
                x.Hours > 0,
                cancellationToken);
            if (!hasPresentAttendance)
                return Result.Failure(VolunteeringErrors.OpportunityAttendanceRequired);
        }

        entity.ProcedureNotes = TrimOrNull(request.ProcedureNotes) ?? entity.ProcedureNotes;
        entity.ReportSummary = TrimOrNull(request.ReportSummary) ?? entity.ReportSummary;
        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<VolunteerOpportunityTaskResponse>>> GetTasksAsync(int? opportunityId = null, VolunteerTaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VolunteerOpportunityTasks.AsNoTracking().Include(x => x.VolunteerOpportunity).AsQueryable();
        if (opportunityId.HasValue) query = query.Where(x => x.VolunteerOpportunityId == opportunityId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var tasks = await query.OrderBy(x => x.Status).ThenBy(x => x.DueDate ?? DateTime.MaxValue).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<VolunteerOpportunityTaskResponse>>(tasks.Select(MapTask).ToList());
    }

    public async Task<Result<VolunteerOpportunityTaskResponse>> SaveTaskAsync(int? id, SaveVolunteerOpportunityTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VolunteerOpportunityId <= 0 || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<VolunteerOpportunityTaskResponse>(VolunteeringErrors.InvalidRequest);
        if (!await dbcontext.VolunteerOpportunities.AnyAsync(x => x.Id == request.VolunteerOpportunityId, cancellationToken))
            return Result.Failure<VolunteerOpportunityTaskResponse>(VolunteeringErrors.OpportunityNotFound);

        var entity = id.HasValue
            ? await dbcontext.VolunteerOpportunityTasks.Include(x => x.VolunteerOpportunity).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VolunteerOpportunityTask();
        if (entity is null)
            return Result.Failure<VolunteerOpportunityTaskResponse>(VolunteeringErrors.TaskNotFound);
        if (!id.HasValue) dbcontext.VolunteerOpportunityTasks.Add(entity);

        entity.VolunteerOpportunityId = request.VolunteerOpportunityId;
        entity.Title = request.Title.Trim();
        entity.AssignedTo = TrimOrNull(request.AssignedTo);
        entity.DueDate = request.DueDate;
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        entity.VolunteerOpportunity ??= await dbcontext.VolunteerOpportunities.FirstOrDefaultAsync(x => x.Id == entity.VolunteerOpportunityId, cancellationToken);
        return Result.Success(MapTask(entity));
    }

    public async Task<Result<IEnumerable<VolunteerAttendanceResponse>>> GetAttendanceAsync(int? opportunityId = null, int? volunteerUserId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VolunteerAttendanceRecords.AsNoTracking().Include(x => x.VolunteerOpportunity).Include(x => x.VolunteerUser).AsQueryable();
        if (opportunityId.HasValue) query = query.Where(x => x.VolunteerOpportunityId == opportunityId.Value);
        if (volunteerUserId.HasValue) query = query.Where(x => x.VolunteerUserId == volunteerUserId.Value);
        var records = await query.OrderByDescending(x => x.AttendanceDate).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<VolunteerAttendanceResponse>>(records.Select(MapAttendance).ToList());
    }

    public async Task<Result<VolunteerAttendanceResponse>> SaveAttendanceAsync(int? id, SaveVolunteerAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VolunteerOpportunityId <= 0 || request.VolunteerUserId <= 0 || request.Hours < 0 || request.Status == VolunteerAttendanceStatus.Present && request.Hours <= 0)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.InvalidRequest);

        var opportunity = await dbcontext.VolunteerOpportunities.FirstOrDefaultAsync(x => x.Id == request.VolunteerOpportunityId, cancellationToken);
        if (opportunity is null)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.OpportunityNotFound);
        if (opportunity.Status is not (VolunteerOpportunityStatus.Open or VolunteerOpportunityStatus.InProgress))
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.OpportunityNotOpen);

        var volunteer = await dbcontext.VolunteerUsers.FirstOrDefaultAsync(x => x.Id == request.VolunteerUserId, cancellationToken);
        if (volunteer is null)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.VolunteerUserNotFound);
        if (volunteer.Status != VolunteerUserStatus.Active)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.VolunteerUserNotActive);

        var isApprovedForOpportunity = await dbcontext.VolunteerRequests.AnyAsync(x =>
            x.VolunteerOpportunityId == request.VolunteerOpportunityId &&
            x.VolunteerUserId == request.VolunteerUserId &&
            (x.Status == VolunteerRequestStatus.Approved || x.Status == VolunteerRequestStatus.Completed),
            cancellationToken);
        if (!isApprovedForOpportunity)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.VolunteerNotApprovedForOpportunity);

        var attendanceDate = request.AttendanceDate.Date;
        var nextDate = attendanceDate.AddDays(1);
        if (await dbcontext.VolunteerAttendanceRecords.AnyAsync(x =>
                x.VolunteerOpportunityId == request.VolunteerOpportunityId &&
                x.VolunteerUserId == request.VolunteerUserId &&
                x.AttendanceDate >= attendanceDate &&
                x.AttendanceDate < nextDate &&
                (!id.HasValue || x.Id != id.Value),
                cancellationToken))
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.DuplicateAttendance);

        var entity = id.HasValue
            ? await dbcontext.VolunteerAttendanceRecords.Include(x => x.VolunteerOpportunity).Include(x => x.VolunteerUser).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VolunteerAttendanceRecord();
        if (entity is null)
            return Result.Failure<VolunteerAttendanceResponse>(VolunteeringErrors.AttendanceNotFound);
        if (!id.HasValue) dbcontext.VolunteerAttendanceRecords.Add(entity);

        entity.VolunteerOpportunityId = request.VolunteerOpportunityId;
        entity.VolunteerUserId = request.VolunteerUserId;
        entity.AttendanceDate = request.AttendanceDate;
        entity.Hours = request.Hours;
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        entity.VolunteerOpportunity ??= opportunity;
        entity.VolunteerUser ??= volunteer;
        return Result.Success(MapAttendance(entity));
    }

    private static VolunteerUserResponse MapUser(VolunteerUser x) =>
        new(x.Id, x.VolunteerNumber, x.FullName, x.NationalId, x.Mobile, x.Email, x.Skills, x.Status.ToString(), x.JoinedAt, x.Notes);

    private static VolunteerRequestResponse MapRequest(VolunteerRequest x) =>
        new(x.Id, x.RequestNumber, x.Source.ToString(), x.ApplicantName, x.Mobile, x.OpportunityTitle, x.Status.ToString(), x.RequestDate, x.DecisionNote, x.Notes, x.VolunteerUserId, x.VolunteerUser?.FullName, x.VolunteerOpportunityId, x.VolunteerOpportunity?.Title);

    private static VolunteerOpportunityResponse MapOpportunity(VolunteerOpportunity x) =>
        new(x.Id, x.OpportunityNumber, x.Title, x.Description, x.Department, x.StartDate, x.EndDate, x.Seats, x.Status.ToString(), x.ProcedureNotes, x.ReportSummary, x.Requests.Count, x.Tasks.Count, x.AttendanceRecords.Count);

    private static VolunteerOpportunityTaskResponse MapTask(VolunteerOpportunityTask x) =>
        new(x.Id, x.VolunteerOpportunityId, x.VolunteerOpportunity?.Title ?? string.Empty, x.Title, x.AssignedTo, x.DueDate, x.Status.ToString(), x.Notes);

    private static VolunteerAttendanceResponse MapAttendance(VolunteerAttendanceRecord x) =>
        new(x.Id, x.VolunteerOpportunityId, x.VolunteerOpportunity?.Title ?? string.Empty, x.VolunteerUserId, x.VolunteerUser?.FullName ?? string.Empty, x.AttendanceDate, x.Hours, x.Status.ToString(), x.Notes);

    private async Task<bool> IsOpportunityFullAsync(int opportunityId, int seats, int ignoredRequestId, CancellationToken cancellationToken)
    {
        if (seats <= 0)
            return false;

        var acceptedCount = await dbcontext.VolunteerRequests.CountAsync(x =>
            x.Id != ignoredRequestId &&
            x.VolunteerOpportunityId == opportunityId &&
            x.VolunteerUserId.HasValue &&
            (x.Status == VolunteerRequestStatus.Approved || x.Status == VolunteerRequestStatus.Completed),
            cancellationToken);

        return acceptedCount >= seats;
    }

    private async Task<string> GenerateVolunteerNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var token = $"VOL-{year}-";
        var count = await dbcontext.VolunteerUsers.CountAsync(x => x.VolunteerNumber.StartsWith(token), cancellationToken);
        string volunteerNumber;
        do
        {
            count++;
            volunteerNumber = $"{token}{count:0000}";
        }
        while (await dbcontext.VolunteerUsers.AnyAsync(x => x.VolunteerNumber == volunteerNumber, cancellationToken));

        return volunteerNumber;
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
