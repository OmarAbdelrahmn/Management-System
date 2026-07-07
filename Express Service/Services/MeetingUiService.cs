using System.Security.Claims;
using Application.Contracts.Invitations;
using Application.Contracts.Meetings;
using Application.Contracts.Minutes;
using Application.Contracts.Voting;
using Application.Service.Invitations;
using Application.Service.Meetings;
using Application.Service.Minutes;
using Application.Service.SystemCatalog;
using Application.Service.Voting;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Express_Service.Services;

public class MeetingUiService(
    ApplicationDbcontext dbcontext,
    IMeetingService meetingService,
    IInvitationService invitationService,
    IVotingService votingService,
    IMinuteService minuteService,
    ISystemCatalogService systemCatalogService,
    IHttpContextAccessor httpContextAccessor)
{
    public string? CurrentUserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<DashboardUiModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var pendingApprovals = await dbcontext.BoardMeetings.CountAsync(x => x.Status == MeetingStatus.PendingApproval, cancellationToken);
        var inProgress = await dbcontext.BoardMeetings.CountAsync(x => x.Status == MeetingStatus.InProgress, cancellationToken);
        var scheduled = await dbcontext.BoardMeetings.CountAsync(x => x.Status != MeetingStatus.ApprovedAndArchived && x.Status != MeetingStatus.Cancelled && x.Status != MeetingStatus.Finished, cancellationToken);
        var minutes = await dbcontext.MeetingMinutes.CountAsync(x => x.IsReadOnly, cancellationToken);
        var invitations = await dbcontext.MeetingInvitations.CountAsync(x => x.Status == InvitationStatus.Pending, cancellationToken);
        var repeatDrafts = await dbcontext.MeetingRepeatDrafts.CountAsync(cancellationToken);

        return new DashboardUiModel(pendingApprovals, inProgress, scheduled, minutes, invitations, repeatDrafts);
    }

    public async Task<List<BoardCycleOption>> GetBoardCyclesAsync(CancellationToken cancellationToken = default) =>
        await dbcontext.BoardCycles
            .AsNoTracking()
            .Include(x => x.Board)
            .Where(x => x.Board != null && x.Board.Status == BoardStatus.Active)
            .OrderByDescending(x => x.StartsAt)
            .Select(x => new BoardCycleOption(x.Id, x.Board!.Name, x.Board.Code, x.CycleNumber, x.StartsAt, x.EndsAt))
            .ToListAsync(cancellationToken);

    public async Task<List<UserOption>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        await dbcontext.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new UserOption(x.Id, x.FullName, x.Email ?? string.Empty))
            .ToListAsync(cancellationToken);

    public async Task<List<MeetingListItemResponse>> GetScheduledMeetingsAsync(CancellationToken cancellationToken = default)
    {
        var result = await meetingService.GetScheduledAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MeetingListItemResponse>> GetArchivedMeetingsAsync(string? type = null, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.GetArchiveAsync(type, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MinuteResponse>> GetArchivedMinutesAsync(CancellationToken cancellationToken = default)
    {
        var result = await minuteService.GetArchiveAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<MeetingResponse>> GetPendingChairmanMeetingsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await dbcontext.BoardMeetings
            .AsNoTracking()
            .Where(x => x.Status == MeetingStatus.WaitingChairmanApproval)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var meetings = new List<MeetingResponse>();
        foreach (var id in ids)
        {
            var result = await meetingService.GetAsync(id, cancellationToken);
            if (result.IsSuccess)
                meetings.Add(result.Value);
        }

        return meetings;
    }

    public async Task<List<InvitationUiModel>> GetCurrentUserInvitationsAsync(CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        return await dbcontext.MeetingInvitations
            .AsNoTracking()
            .Include(x => x.BoardMeeting)
            .ThenInclude(x => x!.AgendaItems)
            .Where(x => x.MemberUserId == userId && x.Status == InvitationStatus.Pending)
            .OrderBy(x => x.BoardMeeting!.ScheduledAt)
            .Select(x => new InvitationUiModel(
                x.Id,
                x.BoardMeetingId,
                x.BoardMeeting!.Title,
                x.BoardMeeting.ScheduledAt,
                x.ExpiresAt,
                x.AgendaReadAcknowledged,
                x.BoardMeeting.AgendaItems.OrderBy(a => a.ItemNumber).Select(a => new AgendaItemResponse(a.Id, a.ItemNumber, a.Title, a.Description, a.RequiresDecision, a.Status.ToString(), a.RejectionText)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string Message, MeetingResponse? Meeting)> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? (true, "تم إنشاء الاجتماع بنجاح.", result.Value)
            : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message)> AddAgendaItemAsync(int meetingId, AddAgendaItemRequest request, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.AddAgendaItemAsync(meetingId, request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة البند.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SendInvitationsAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var result = await invitationService.SendMeetingInvitationsAsync(meetingId, cancellationToken);
        return result.IsSuccess ? (true, $"تم إرسال {result.Value.Count()} دعوة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SubmitMeetingForApprovalAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.SubmitForApprovalAsync(meetingId, new SubmitMeetingApprovalRequest(null), cancellationToken);
        return result.IsSuccess ? (true, "تم إرسال الاجتماع للاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> ApproveMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.ApproveAsync(meetingId, new DecideMeetingApprovalRequest(CurrentUserId ?? "system", null), cancellationToken);
        return result.IsSuccess ? (true, "تم اعتماد الاجتماع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RejectMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.RejectAsync(meetingId, new DecideMeetingApprovalRequest(CurrentUserId ?? "system", null), cancellationToken);
        return result.IsSuccess ? (true, "تم رفض الاجتماع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> FinishMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var result = await meetingService.FinishAsync(meetingId, cancellationToken);
        return result.IsSuccess ? (true, "تم إنهاء الاجتماع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> AcceptInvitationAsync(int invitationId, bool agendaRead, string? note, MeetingNoteVisibility visibility, int? agendaItemId, CancellationToken cancellationToken = default)
    {
        var request = new AcceptInvitationRequest(
            agendaRead,
            string.IsNullOrWhiteSpace(note) ? null : new MeetingNoteRequest(note, visibility, agendaItemId));
        var result = await invitationService.AcceptAsync(invitationId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم قبول الدعوة.") : (false, result.Error.Description);
    }

    public async Task<List<VoteSessionUiModel>> GetVoteSessionsAsync(CancellationToken cancellationToken = default) =>
        await dbcontext.VoteSessions
            .AsNoTracking()
            .Include(x => x.MeetingAgendaItem)
            .ThenInclude(x => x!.BoardMeeting)
            .Include(x => x.Votes)
            .Where(x => x.Status == VoteSessionStatus.Open)
            .OrderByDescending(x => x.OpenedAt)
            .Select(x => new VoteSessionUiModel(
                x.Id,
                x.MeetingAgendaItemId,
                x.MeetingAgendaItem!.BoardMeetingId,
                x.MeetingAgendaItem.BoardMeeting!.Title,
                x.MeetingAgendaItem.Title,
                x.Status.ToString(),
                x.Votes.Select(v => new VoteResponse(v.MemberUserId, v.Choice, v.Weight, v.RejectionReason)).ToList()))
            .ToListAsync(cancellationToken);

    public async Task<(bool Success, string Message)> OpenVoteAsync(int agendaItemId, CancellationToken cancellationToken = default)
    {
        var result = await votingService.OpenAsync(agendaItemId, cancellationToken);
        return result.IsSuccess ? (true, "تم فتح التصويت.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CloseVoteAsync(int voteSessionId, CancellationToken cancellationToken = default)
    {
        var result = await votingService.CloseAsync(voteSessionId, cancellationToken);
        return result.IsSuccess ? (true, "تم إغلاق التصويت.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SignMinuteAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId ?? "system";
        var result = await minuteService.SignAndPublishAsync(meetingId, new SignMinuteRequest(userId), cancellationToken);
        return result.IsSuccess ? (true, "تم اعتماد وتوقيع المحضر.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SeedSystemCatalogAsync(CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.SeedFirstPagesAsync(cancellationToken);
        return result.IsSuccess
            ? (true, $"تم تجهيز {result.Value.TotalPages} صفحة. جديد: {result.Value.PagesCreated}، تحديث: {result.Value.PagesUpdated}.")
            : (false, result.Error.Description);
    }

    public async Task<List<Application.Contracts.SystemCatalog.SystemModuleResponse>> GetSystemModulesAsync(CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.GetModulesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<Application.Contracts.SystemCatalog.SystemPageResponse>> GetSystemPagesAsync(string? moduleKey = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.GetPagesAsync(moduleKey, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<Application.Contracts.SystemCatalog.SystemPageResponse?> GetSystemPageAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.GetPageAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<(bool Success, string Message)> UpdateSystemPageStatusAsync(int id, SystemPageStatus status, CancellationToken cancellationToken = default)
    {
        var result = await systemCatalogService.UpdatePageStatusAsync(id, new Application.Contracts.SystemCatalog.UpdateSystemPageStatusRequest(status), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الصفحة.") : (false, result.Error.Description);
    }
}

public record DashboardUiModel(int PendingApprovals, int InProgress, int Scheduled, int PublishedMinutes, int PendingInvitations, int RepeatDrafts);
public record BoardCycleOption(int Id, string BoardName, string BoardCode, int CycleNumber, DateTime StartsAt, DateTime EndsAt);
public record UserOption(string Id, string FullName, string Email);
public record InvitationUiModel(int Id, int MeetingId, string MeetingTitle, DateTime ScheduledAt, DateTime ExpiresAt, bool AgendaReadAcknowledged, IReadOnlyList<AgendaItemResponse> AgendaItems);
public record VoteSessionUiModel(int Id, int AgendaItemId, int MeetingId, string MeetingTitle, string AgendaTitle, string Status, IReadOnlyList<VoteResponse> Votes);
