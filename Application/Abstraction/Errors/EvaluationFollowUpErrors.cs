using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class EvaluationFollowUpErrors
{
    public static readonly Error CaseNotFound = new("EvaluationFollowUp.CaseNotFound", "Follow-up case was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ActivityNotFound = new("EvaluationFollowUp.ActivityNotFound", "Follow-up activity was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidRequest = new("EvaluationFollowUp.InvalidRequest", "Evaluation follow-up request is invalid.", StatusCodes.Status400BadRequest);
}
