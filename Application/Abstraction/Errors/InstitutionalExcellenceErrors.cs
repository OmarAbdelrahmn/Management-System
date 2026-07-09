using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class InstitutionalExcellenceErrors
{
    public static readonly Error PerformanceMeasureNotFound = new("InstitutionalExcellence.PerformanceMeasureNotFound", "Performance measure was not found.", StatusCodes.Status404NotFound);
    public static readonly Error GovernanceCycleNotFound = new("InstitutionalExcellence.GovernanceCycleNotFound", "Governance cycle was not found.", StatusCodes.Status404NotFound);
    public static readonly Error GovernanceCriterionNotFound = new("InstitutionalExcellence.GovernanceCriterionNotFound", "Governance criterion was not found.", StatusCodes.Status404NotFound);
    public static readonly Error GovernanceAttachmentNotFound = new("InstitutionalExcellence.GovernanceAttachmentNotFound", "Governance attachment was not found.", StatusCodes.Status404NotFound);
    public static readonly Error GovernanceTaskNotFound = new("InstitutionalExcellence.GovernanceTaskNotFound", "Governance task was not found.", StatusCodes.Status404NotFound);
    public static readonly Error StrategicPlanNotFound = new("InstitutionalExcellence.StrategicPlanNotFound", "Strategic plan was not found.", StatusCodes.Status404NotFound);
    public static readonly Error StrategicPerspectiveNotFound = new("InstitutionalExcellence.StrategicPerspectiveNotFound", "Strategic perspective was not found.", StatusCodes.Status404NotFound);
    public static readonly Error StrategicGoalNotFound = new("InstitutionalExcellence.StrategicGoalNotFound", "Strategic goal was not found.", StatusCodes.Status404NotFound);
    public static readonly Error StrategicIndicatorNotFound = new("InstitutionalExcellence.StrategicIndicatorNotFound", "Strategic indicator was not found.", StatusCodes.Status404NotFound);
    public static readonly Error StrategicVariableNotFound = new("InstitutionalExcellence.StrategicVariableNotFound", "Strategic variable was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidRequest = new("InstitutionalExcellence.InvalidRequest", "Institutional excellence request is invalid.", StatusCodes.Status400BadRequest);
}
