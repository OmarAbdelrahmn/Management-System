using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class PublicRelationsMediaErrors
{
    public static readonly Error PartnerNotFound = new("PublicRelationsMedia.PartnerNotFound", "Media partner was not found.", StatusCodes.Status404NotFound);
    public static readonly Error EventNotFound = new("PublicRelationsMedia.EventNotFound", "Media event was not found.", StatusCodes.Status404NotFound);
    public static readonly Error VisitNotFound = new("PublicRelationsMedia.VisitNotFound", "Media visit was not found.", StatusCodes.Status404NotFound);
    public static readonly Error WebsiteUserNotFound = new("PublicRelationsMedia.WebsiteUserNotFound", "Website user was not found.", StatusCodes.Status404NotFound);
    public static readonly Error TemplateNotFound = new("PublicRelationsMedia.TemplateNotFound", "Communication template was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ListNotFound = new("PublicRelationsMedia.ListNotFound", "Communication list was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CampaignNotFound = new("PublicRelationsMedia.CampaignNotFound", "Communication campaign was not found.", StatusCodes.Status404NotFound);
    public static readonly Error SubscriberNotFound = new("PublicRelationsMedia.SubscriberNotFound", "Push subscriber was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DesignNotFound = new("PublicRelationsMedia.DesignNotFound", "Website design setting was not found.", StatusCodes.Status404NotFound);
    public static readonly Error NavigationNotFound = new("PublicRelationsMedia.NavigationNotFound", "Website navigation item was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ContentNotFound = new("PublicRelationsMedia.ContentNotFound", "Website content item was not found.", StatusCodes.Status404NotFound);
    public static readonly Error FormNotFound = new("PublicRelationsMedia.FormNotFound", "Website form was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ContactRequestNotFound = new("PublicRelationsMedia.ContactRequestNotFound", "Website contact request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateUsername = new("PublicRelationsMedia.DuplicateUsername", "Website username is already used.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidRequest = new("PublicRelationsMedia.InvalidRequest", "Public relations and media request is invalid.", StatusCodes.Status400BadRequest);
}
