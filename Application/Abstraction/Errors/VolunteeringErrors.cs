using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class VolunteeringErrors
{
    public static readonly Error InvalidRequest = new("Volunteering.InvalidRequest", "Volunteering request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error VolunteerUserNotFound = new("Volunteering.VolunteerUserNotFound", "Volunteer account was not found.", StatusCodes.Status404NotFound);
    public static readonly Error VolunteerUserNotActive = new("Volunteering.VolunteerUserNotActive", "Volunteer account must be active.", StatusCodes.Status409Conflict);
    public static readonly Error VolunteerNotApprovedForOpportunity = new("Volunteering.VolunteerNotApprovedForOpportunity", "Volunteer is not approved for this opportunity.", StatusCodes.Status409Conflict);
    public static readonly Error DuplicateVolunteerNumber = new("Volunteering.DuplicateVolunteerNumber", "Volunteer number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error VolunteerRequestNotFound = new("Volunteering.VolunteerRequestNotFound", "Volunteer request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateRequestNumber = new("Volunteering.DuplicateRequestNumber", "Volunteer request number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityNotFound = new("Volunteering.OpportunityNotFound", "Volunteer opportunity was not found.", StatusCodes.Status404NotFound);
    public static readonly Error OpportunityNotOpen = new("Volunteering.OpportunityNotOpen", "Volunteer opportunity is not open for enrollment.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityCapacityReached = new("Volunteering.OpportunityCapacityReached", "Volunteer opportunity capacity has been reached.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityHasOpenTasks = new("Volunteering.OpportunityHasOpenTasks", "Volunteer opportunity has open or in-progress tasks.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityAttendanceRequired = new("Volunteering.OpportunityAttendanceRequired", "Volunteer opportunity requires present attendance before completion.", StatusCodes.Status409Conflict);
    public static readonly Error DuplicateOpportunityNumber = new("Volunteering.DuplicateOpportunityNumber", "Volunteer opportunity number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error TaskNotFound = new("Volunteering.TaskNotFound", "Volunteer opportunity task was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AttendanceNotFound = new("Volunteering.AttendanceNotFound", "Volunteer attendance record was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateAttendance = new("Volunteering.DuplicateAttendance", "Volunteer attendance is already recorded for this opportunity and date.", StatusCodes.Status409Conflict);
}
