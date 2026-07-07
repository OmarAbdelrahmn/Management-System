using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Meetings;
using Application.Service.Permissions;
using Application.Service.Realtime;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Meetings;

public class MeetingService(
    ApplicationDbcontext dbcontext,
    IBoardAccessService? boardAccessService = null,
    IMeetingRealtimeNotifier? realtimeNotifier = null) : IMeetingService
{
    public async Task<Result<MeetingResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var meeting = await LoadMeetingQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (meeting is not null && boardAccessService is not null && !await boardAccessService.CanAccessMeetingAsync(meeting.Id, cancellationToken))
            return Result.Failure<MeetingResponse>(PermissionErrors.Forbidden);

        return meeting is null
            ? Result.Failure<MeetingResponse>(MeetingErrors.NotFound)
            : Result.Success(MapMeeting(meeting));
    }

    public async Task<Result<IEnumerable<MeetingListItemResponse>>> GetScheduledAsync(CancellationToken cancellationToken = default)
    {
        var meetings = await dbcontext.BoardMeetings
            .AsNoTracking()
            .Where(x => x.Status != MeetingStatus.ApprovedAndArchived && x.Status != MeetingStatus.Cancelled && x.Status != MeetingStatus.Finished)
            .OrderBy(x => x.ScheduledAt)
            .Select(x => MapListItem(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MeetingListItemResponse>>(meetings);
    }

    public async Task<Result<IEnumerable<MeetingCalendarItemResponse>>> GetCalendarAsync(CancellationToken cancellationToken = default)
    {
        var meetings = await dbcontext.BoardMeetings
            .AsNoTracking()
            .OrderBy(x => x.ScheduledAt)
            .Select(x => new MeetingCalendarItemResponse(
                x.Id,
                x.Title,
                x.ScheduledAt,
                x.DurationMinutes.HasValue ? x.ScheduledAt.AddMinutes(x.DurationMinutes.Value) : null,
                x.Type.ToString(),
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MeetingCalendarItemResponse>>(meetings);
    }

    public async Task<Result<IEnumerable<MeetingRepeatDraftResponse>>> GetRepeatedDraftsAsync(CancellationToken cancellationToken = default)
    {
        var drafts = await dbcontext.MeetingRepeatDrafts
            .AsNoTracking()
            .OrderBy(x => x.ScheduledAt)
            .Select(x => MapRepeatDraft(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MeetingRepeatDraftResponse>>(drafts);
    }

    public async Task<Result<IEnumerable<MeetingListItemResponse>>> GetArchiveAsync(string? type, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BoardMeetings
            .AsNoTracking()
            .Where(x => x.Status == MeetingStatus.ApprovedAndArchived || x.Status == MeetingStatus.Finished);

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!TryParseMeetingType(type, out var parsedType))
                return Result.Failure<IEnumerable<MeetingListItemResponse>>(MeetingErrors.InvalidMeetingType);

            query = query.Where(x => x.Type == parsedType);
        }

        var meetings = await query
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => MapListItem(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MeetingListItemResponse>>(meetings);
    }

    public async Task<Result<MeetingResponse>> CreateAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var deadlineDays = request.AcceptanceDeadlineDays ?? 15;
        if (deadlineDays < 1)
            return Result.Failure<MeetingResponse>(MeetingErrors.InvalidDeadline);

        var cycle = await dbcontext.BoardCycles
            .Include(x => x.Board)
            .FirstOrDefaultAsync(x => x.Id == request.BoardCycleId, cancellationToken);

        if (cycle is null)
            return Result.Failure<MeetingResponse>(BoardErrors.CycleNotFound);

        if (cycle.Board?.Status == BoardStatus.Closed)
            return Result.Failure<MeetingResponse>(BoardErrors.Closed);

        if (boardAccessService is not null && !await boardAccessService.CanManageBoardAsync(cycle.BoardId, cancellationToken))
            return Result.Failure<MeetingResponse>(PermissionErrors.Forbidden);

        if (request.ScheduledAt < cycle.StartsAt || request.ScheduledAt > cycle.EndsAt)
            return Result.Failure<MeetingResponse>(MeetingErrors.InvalidMeetingDate);

        var meeting = new BoardMeeting
        {
            BoardCycleId = request.BoardCycleId,
            Serial = request.Serial,
            Title = request.Title.Trim(),
            ScheduledAt = request.ScheduledAt,
            AcceptanceDeadlineDays = deadlineDays,
            Type = request.Type,
            Category = request.Category?.Trim(),
            Importance = request.Importance,
            HasVoting = request.HasVoting,
            IsOnline = request.IsOnline,
            Platform = request.Platform?.Trim(),
            Location = request.Location?.Trim(),
            ReminderEnabled = request.ReminderEnabled,
            ReminderAt = request.ReminderAt,
            DurationMinutes = request.DurationMinutes,
            RepeatMode = request.RepeatMode,
            RepeatUntil = request.RepeatUntil,
            MinimumAttendancePercentage = request.MinimumAttendancePercentage,
            AvailableSeats = request.AvailableSeats,
            Notes = request.Notes?.Trim()
        };

        foreach (var userId in request.ManagerUserIds?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct() ?? [])
            meeting.Managers.Add(new MeetingManager { UserId = userId.Trim() });

        foreach (var userId in request.CandidateUserIds?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct() ?? [])
            meeting.Candidates.Add(new MeetingCandidate { UserId = userId.Trim() });

        foreach (var guest in request.Guests?.Where(x => !string.IsNullOrWhiteSpace(x.FullName)) ?? [])
        {
            meeting.Guests.Add(new MeetingGuest
            {
                FullName = guest.FullName.Trim(),
                Email = guest.Email?.Trim(),
                PhoneNumber = guest.PhoneNumber?.Trim()
            });
        }

        AddRepeatDrafts(meeting);

        dbcontext.BoardMeetings.Add(meeting);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return await GetAsync(meeting.Id, cancellationToken);
    }

    public async Task<Result<AgendaItemResponse>> AddAgendaItemAsync(int meetingId, AddAgendaItemRequest request, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.Minute)
            .Include(x => x.AgendaItems)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<AgendaItemResponse>(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure<AgendaItemResponse>(PermissionErrors.Forbidden);

        if (meeting.Minute?.IsReadOnly == true)
            return Result.Failure<AgendaItemResponse>(MeetingErrors.ReadOnlyMinute);

        var nextNumber = meeting.AgendaItems.Count == 0 ? 1 : meeting.AgendaItems.Max(x => x.ItemNumber) + 1;
        var item = new MeetingAgendaItem
        {
            BoardMeetingId = meetingId,
            ItemNumber = nextNumber,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            RequiresDecision = request.RequiresDecision
        };

        dbcontext.MeetingAgendaItems.Add(item);
        await dbcontext.SaveChangesAsync(cancellationToken);

        var response = MapAgendaItem(item);
        if (realtimeNotifier is not null)
            await realtimeNotifier.AgendaItemAddedAsync(meetingId, response, cancellationToken);

        return Result.Success(response);
    }

    public async Task<Result> SubmitForApprovalAsync(int meetingId, SubmitMeetingApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings.FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure(PermissionErrors.Forbidden);

        if (meeting.Status != MeetingStatus.Draft)
            return Result.Failure(MeetingErrors.InvalidWorkflowState);

        meeting.Status = MeetingStatus.PendingApproval;
        dbcontext.MeetingApprovals.Add(new MeetingApproval
        {
            BoardMeetingId = meetingId,
            Status = MeetingApprovalStatus.Pending,
            Comments = request.Comments?.Trim()
        });

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ApproveAsync(int meetingId, DecideMeetingApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure(PermissionErrors.Forbidden);

        var approval = meeting.Approvals.LastOrDefault(x => x.Status == MeetingApprovalStatus.Pending)
            ?? new MeetingApproval { BoardMeetingId = meetingId };
        approval.ApproverUserId = request.ApproverUserId;
        approval.Status = MeetingApprovalStatus.Approved;
        approval.Comments = request.Comments?.Trim();
        approval.DecidedAt = DateTime.UtcNow.AddHours(3);
        if (approval.Id == 0)
            dbcontext.MeetingApprovals.Add(approval);

        meeting.Status = MeetingStatus.Draft;
        await dbcontext.SaveChangesAsync(cancellationToken);

        if (realtimeNotifier is not null)
            await realtimeNotifier.MeetingApprovedAsync(meetingId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(int meetingId, DecideMeetingApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure(PermissionErrors.Forbidden);

        var approval = meeting.Approvals.LastOrDefault(x => x.Status == MeetingApprovalStatus.Pending)
            ?? new MeetingApproval { BoardMeetingId = meetingId };
        approval.ApproverUserId = request.ApproverUserId;
        approval.Status = MeetingApprovalStatus.Rejected;
        approval.Comments = request.Comments?.Trim();
        approval.DecidedAt = DateTime.UtcNow.AddHours(3);
        if (approval.Id == 0)
            dbcontext.MeetingApprovals.Add(approval);

        meeting.Status = MeetingStatus.Rejected;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> StartAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings.FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure(PermissionErrors.Forbidden);

        meeting.Status = MeetingStatus.InProgress;
        meeting.StartedAt ??= DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);

        if (realtimeNotifier is not null)
            await realtimeNotifier.MeetingStartedAsync(meetingId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> FinishAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings.FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure(PermissionErrors.Forbidden);

        meeting.Status = MeetingStatus.Finished;
        meeting.EndedAt ??= DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);

        if (realtimeNotifier is not null)
            await realtimeNotifier.MeetingFinishedAsync(meetingId, cancellationToken);

        return Result.Success();
    }

    private IQueryable<BoardMeeting> LoadMeetingQuery() =>
        dbcontext.BoardMeetings
            .Include(x => x.AgendaItems)
            .Include(x => x.Managers)
            .Include(x => x.Candidates)
            .Include(x => x.Guests)
            .Include(x => x.RepeatDrafts);

    private static MeetingResponse MapMeeting(BoardMeeting meeting) =>
        new(
            meeting.Id,
            meeting.BoardCycleId,
            meeting.Title,
            meeting.ScheduledAt,
            meeting.AcceptanceDeadlineDays,
            meeting.Serial,
            meeting.Type.ToString(),
            meeting.Category,
            meeting.Importance.ToString(),
            meeting.HasVoting,
            meeting.IsOnline,
            meeting.Platform,
            meeting.Location,
            meeting.ReminderEnabled,
            meeting.ReminderAt,
            meeting.DurationMinutes,
            meeting.RepeatMode.ToString(),
            meeting.RepeatUntil,
            meeting.MinimumAttendancePercentage,
            meeting.AvailableSeats,
            meeting.Notes,
            meeting.Status.ToString(),
            meeting.Managers.Select(x => x.UserId),
            meeting.Candidates.Select(x => x.UserId),
            meeting.Guests.Select(x => new MeetingGuestResponse(x.Id, x.FullName, x.Email, x.PhoneNumber)),
            meeting.RepeatDrafts.OrderBy(x => x.ScheduledAt).Select(MapRepeatDraft),
            meeting.AgendaItems.OrderBy(x => x.ItemNumber).Select(MapAgendaItem));

    private static AgendaItemResponse MapAgendaItem(MeetingAgendaItem item) =>
        new(
            item.Id,
            item.ItemNumber,
            item.Title,
            item.Description,
            item.RequiresDecision,
            item.Status.ToString(),
            item.RejectionText);

    private static MeetingListItemResponse MapListItem(BoardMeeting meeting) =>
        new(
            meeting.Id,
            meeting.Title,
            meeting.ScheduledAt,
            meeting.Type.ToString(),
            meeting.Status.ToString(),
            meeting.IsOnline,
            meeting.Location,
            meeting.Platform);

    private static MeetingRepeatDraftResponse MapRepeatDraft(MeetingRepeatDraft draft) =>
        new(draft.Id, draft.SourceBoardMeetingId, draft.CreatedBoardMeetingId, draft.ScheduledAt);

    private static void AddRepeatDrafts(BoardMeeting meeting)
    {
        if (meeting.RepeatMode == MeetingRepeatMode.None || meeting.RepeatUntil is null)
            return;

        var next = NextOccurrence(meeting.ScheduledAt, meeting.RepeatMode);
        while (next <= meeting.RepeatUntil.Value)
        {
            meeting.RepeatDrafts.Add(new MeetingRepeatDraft { ScheduledAt = next });
            next = NextOccurrence(next, meeting.RepeatMode);
        }
    }

    private static DateTime NextOccurrence(DateTime date, MeetingRepeatMode repeatMode) =>
        repeatMode switch
        {
            MeetingRepeatMode.Daily => date.AddDays(1),
            MeetingRepeatMode.Weekly => date.AddDays(7),
            MeetingRepeatMode.Monthly => date.AddMonths(1),
            MeetingRepeatMode.Yearly => date.AddYears(1),
            _ => date
        };

    private static bool TryParseMeetingType(string type, out MeetingType parsedType)
    {
        parsedType = type.Trim().ToLowerInvariant() switch
        {
            "general" => MeetingType.General,
            "board" => MeetingType.Board,
            "assembly" => MeetingType.Assembly,
            _ => default
        };

        return parsedType != default;
    }
}
