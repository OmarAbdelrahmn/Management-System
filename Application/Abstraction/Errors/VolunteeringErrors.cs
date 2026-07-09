using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class VolunteeringErrors
{
    public static readonly Error InvalidRequest = new("Volunteering.InvalidRequest", "Volunteering request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error VolunteerUserNotFound = new("Volunteering.VolunteerUserNotFound", "Volunteer account was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateVolunteerNumber = new("Volunteering.DuplicateVolunteerNumber", "Volunteer number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error VolunteerRequestNotFound = new("Volunteering.VolunteerRequestNotFound", "Volunteer request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateRequestNumber = new("Volunteering.DuplicateRequestNumber", "Volunteer request number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error OpportunityNotFound = new("Volunteering.OpportunityNotFound", "Volunteer opportunity was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateOpportunityNumber = new("Volunteering.DuplicateOpportunityNumber", "Volunteer opportunity number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error TaskNotFound = new("Volunteering.TaskNotFound", "Volunteer opportunity task was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AttendanceNotFound = new("Volunteering.AttendanceNotFound", "Volunteer attendance record was not found.", StatusCodes.Status404NotFound);
}
