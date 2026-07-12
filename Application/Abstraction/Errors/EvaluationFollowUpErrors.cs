using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class EvaluationFollowUpErrors
{
    public static readonly Error CaseNotFound = new("EvaluationFollowUp.CaseNotFound", "Follow-up case was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ActivityNotFound = new("EvaluationFollowUp.ActivityNotFound", "Follow-up activity was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidStatusTransition = new("EvaluationFollowUp.InvalidStatusTransition", "Follow-up case status transition is not allowed.", StatusCodes.Status409Conflict);
    public static readonly Error CompletionSummaryRequired = new("EvaluationFollowUp.CompletionSummaryRequired", "Follow-up case requires a completion summary before approval.", StatusCodes.Status400BadRequest);
    public static readonly Error OpenNextActions = new("EvaluationFollowUp.OpenNextActions", "Follow-up case has open next actions that must be resolved before approval.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidRequest = new("EvaluationFollowUp.InvalidRequest", "Evaluation follow-up request is invalid.", StatusCodes.Status400BadRequest);
}
