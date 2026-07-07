using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Minutes;
using Application.Service.Permissions;
using Application.Service.Realtime;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Application.Service.Minutes;

public class MinuteService(
    ApplicationDbcontext dbcontext,
    IBoardAccessService? boardAccessService = null,
    IMeetingRealtimeNotifier? realtimeNotifier = null) : IMinuteService
{
    public async Task<Result<IEnumerable<MinuteResponse>>> GetArchiveAsync(CancellationToken cancellationToken = default)
    {
        var minutes = await dbcontext.MeetingMinutes
            .AsNoTracking()
            .Where(x => x.IsReadOnly)
            .Include(x => x.BoardMeeting)
            .ThenInclude(x => x!.AgendaItems)
            .ThenInclude(x => x.Decision)
            .OrderByDescending(x => x.PublishedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MinuteResponse>>(minutes.Select(MapMinute));
    }

    public async Task<Result<MinuteResponse>> GetAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        if (boardAccessService is not null && !await boardAccessService.CanAccessMeetingAsync(meetingId, cancellationToken))
            return Result.Failure<MinuteResponse>(PermissionErrors.Forbidden);

        var minute = await LoadMinuteQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BoardMeetingId == meetingId, cancellationToken);

        return minute is null
            ? Result.Failure<MinuteResponse>(MinuteErrors.NotFound)
            : Result.Success(MapMinute(minute));
    }

    public async Task<Result<MinuteResponse>> SignAndPublishAsync(int meetingId, SignMinuteRequest request, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.Minute)
            .Include(x => x.AgendaItems)
            .ThenInclude(x => x.Decision)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<MinuteResponse>(MeetingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanChairMeetingAsync(meetingId, cancellationToken))
            return Result.Failure<MinuteResponse>(PermissionErrors.Forbidden);

        var decisions = meeting.AgendaItems
            .Select(x => x.Decision)
            .Where(x => x is not null && x.Status == DecisionStatus.WaitingChairmanSignature)
            .Cast<Decision>()
            .ToList();

        if (decisions.Count == 0)
            return Result.Failure<MinuteResponse>(MinuteErrors.NoApprovedDecisions);

        var now = DateTime.UtcNow.AddHours(3);
        foreach (var decision in decisions)
        {
            decision.Status = DecisionStatus.Published;
            decision.SignedAt = now;
            decision.SignedByUserId = request.ChairmanUserId;
        }

        var minute = meeting.Minute ?? new MeetingMinute { BoardMeetingId = meeting.Id };
        minute.IsReadOnly = true;
        minute.PublishedAt = now;
        minute.DraftText = BuildMinuteText(meeting);
        if (meeting.Minute is null)
            dbcontext.MeetingMinutes.Add(minute);

        meeting.Status = MeetingStatus.ApprovedAndArchived;
        meeting.EndedAt ??= now;

        dbcontext.AuditLogs.Add(new AuditLog
        {
            ActorUserId = request.ChairmanUserId,
            Action = "ChairmanDigitalSignature",
            EntityName = nameof(BoardMeeting),
            EntityId = meeting.Id.ToString(),
            Details = "Chairman signed and published meeting minute and approved decisions."
        });

        await dbcontext.SaveChangesAsync(cancellationToken);

        var result = await GetAsync(meeting.Id, cancellationToken);
        if (result.IsSuccess && realtimeNotifier is not null)
            await realtimeNotifier.MinutePublishedAsync(meeting.Id, result.Value, cancellationToken);

        return result;
    }

    public async Task<Result<byte[]>> GeneratePdfAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        if (boardAccessService is not null && !await boardAccessService.CanAccessMeetingAsync(meetingId, cancellationToken))
            return Result.Failure<byte[]>(PermissionErrors.Forbidden);

        var meeting = await dbcontext.BoardMeetings
            .AsNoTracking()
            .Include(x => x.BoardCycle)
            .ThenInclude(x => x!.Board)
            .Include(x => x.AgendaItems)
            .ThenInclude(x => x.Decision)
            .Include(x => x.AgendaItems)
            .ThenInclude(x => x.VoteSession)
            .ThenInclude(x => x!.Votes)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<byte[]>(MeetingErrors.NotFound);

        var memberIds = meeting.AgendaItems
            .SelectMany(x => x.VoteSession?.Votes ?? Enumerable.Empty<Vote>())
            .Where(x => x.Choice == VoteChoice.Reject && !string.IsNullOrWhiteSpace(x.RejectionReason))
            .Select(x => x.MemberUserId)
            .Distinct()
            .ToList();

        var members = await dbcontext.Users
            .AsNoTracking()
            .Where(x => memberIds.Contains(x.Id))
            .Select(x => new { x.Id, x.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Header().Text($"محضر اجتماع {meeting.Title} - تاريخ الاجتماع الفعلي: {meeting.ScheduledAt:yyyy-MM-dd HH:mm}")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);
                page.Content().Column(column =>
                {
                    foreach (var item in meeting.AgendaItems.OrderBy(x => x.ItemNumber))
                    {
                        column.Item().PaddingVertical(6).Text($"{item.ItemNumber}. {item.Title}").SemiBold();
                        if (item.Status == AgendaItemStatus.Rejected)
                            column.Item().Text("بند مرفوض").FontColor(Colors.Red.Darken2);
                        if (item.Decision is not null)
                            column.Item().Text($"كود القرار: {item.Decision.Code}");

                        foreach (var vote in item.VoteSession?.Votes.Where(x => x.Choice == VoteChoice.Reject && !string.IsNullOrWhiteSpace(x.RejectionReason)) ?? Enumerable.Empty<Vote>())
                        {
                            var name = members.TryGetValue(vote.MemberUserId, out var fullName) ? fullName : vote.MemberUserId;
                            column.Item().Text($"وقد تحفظ العضو/ {name} على هذا البند بناءً على: {vote.RejectionReason}.");
                        }
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("تم توليد المستند آلياً من نظام إدارة المجالس");
                });
            });
        }).GeneratePdf();

        return Result.Success(pdf);
    }

    public async Task<Result> CancelApprovalAsync(int meetingId, string actorUserId, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.Minute)
            .Include(x => x.AgendaItems)
            .ThenInclude(x => x.Decision)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.NotFound);

        if (meeting.Minute is null || !meeting.Minute.IsReadOnly)
            return Result.Failure(MinuteErrors.NotFound);

        var now = DateTime.UtcNow.AddHours(3);
        meeting.Minute.IsReadOnly = false;
        meeting.Minute.PublishedAt = null;
        meeting.Status = MeetingStatus.WaitingChairmanApproval;

        foreach (var decision in meeting.AgendaItems.Select(x => x.Decision).Where(x => x is not null).Cast<Decision>())
        {
            decision.Status = DecisionStatus.WaitingChairmanSignature;
            decision.SignedAt = null;
            decision.SignedByUserId = null;
        }

        dbcontext.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "CancelMinuteApproval",
            EntityName = nameof(BoardMeeting),
            EntityId = meeting.Id.ToString(),
            Details = $"Minute approval was cancelled at {now:O}."
        });

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private IQueryable<MeetingMinute> LoadMinuteQuery() =>
        dbcontext.MeetingMinutes
            .Include(x => x.BoardMeeting)
            .ThenInclude(x => x!.AgendaItems)
            .ThenInclude(x => x.Decision);

    private static MinuteResponse MapMinute(MeetingMinute minute) =>
        new(
            minute.Id,
            minute.BoardMeetingId,
            minute.DraftText,
            minute.IsReadOnly,
            minute.CreatedAt,
            minute.PublishedAt,
            minute.BoardMeeting?.AgendaItems
                .Select(x => x.Decision)
                .Where(x => x is not null)
                .Cast<Decision>()
                .OrderBy(x => x.Sequence)
                .Select(x => new DecisionResponse(
                    x.Id,
                    x.MeetingAgendaItemId,
                    x.Code,
                    x.Sequence,
                    x.Status.ToString(),
                    x.CreatedAt,
                    x.SignedAt,
                    x.SignedByUserId)) ?? Enumerable.Empty<DecisionResponse>());

    private static string BuildMinuteText(BoardMeeting meeting)
    {
        var lines = new List<string>
        {
            $"محضر اجتماع: {meeting.Title}",
            $"تاريخ الاجتماع الفعلي: {meeting.ScheduledAt:yyyy-MM-dd HH:mm}"
        };

        foreach (var item in meeting.AgendaItems.OrderBy(x => x.ItemNumber))
        {
            lines.Add($"{item.ItemNumber}. {item.Title}");
            if (item.Status == AgendaItemStatus.Rejected)
                lines.Add("بند مرفوض");
            if (item.Decision is not null)
                lines.Add($"قرار معتمد: {item.Decision.Code}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
