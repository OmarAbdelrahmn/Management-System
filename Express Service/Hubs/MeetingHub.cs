using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Application.Service.Permissions;

namespace Express_Service.Hubs;

[Authorize]
public class MeetingHub(IBoardAccessService boardAccessService) : Hub
{
    public async Task JoinMeetingGroup(int meetingId)
    {
        if (!await boardAccessService.CanAccessMeetingAsync(meetingId))
            throw new HubException("Current user does not have permission to join this meeting.");

        await Groups.AddToGroupAsync(Context.ConnectionId, MeetingGroup(meetingId));
    }

    public Task LeaveMeetingGroup(int meetingId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, MeetingGroup(meetingId));

    public static string MeetingGroup(int meetingId) => $"meeting-{meetingId}";
}
