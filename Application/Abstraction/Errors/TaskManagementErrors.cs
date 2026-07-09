using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class TaskManagementErrors
{
    public static readonly Error TaskNotFound =
        new("Tasks.NotFound", "Task was not found.", StatusCodes.Status404NotFound);

    public static readonly Error UserNotFound =
        new("Tasks.UserNotFound", "Selected user was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRequest =
        new("Tasks.InvalidRequest", "Task request is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error RouteNotFound =
        new("Approvals.RouteNotFound", "Approval route was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ApprovalRequestNotFound =
        new("Approvals.RequestNotFound", "Approval request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidApprovalState =
        new("Approvals.InvalidState", "Approval request cannot move to the requested state.", StatusCodes.Status409Conflict);
}
