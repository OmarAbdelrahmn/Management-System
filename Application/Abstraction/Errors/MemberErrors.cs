using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class MemberErrors
{
    public static readonly Error NotFound =
        new("Members.NotFound", "Member was not found.", StatusCodes.Status404NotFound);

    public static readonly Error MembershipTypeNotFound =
        new("Members.MembershipTypeNotFound", "Membership type was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateMemberNumber =
        new("Members.DuplicateMemberNumber", "Member number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRequest =
        new("Members.InvalidRequest", "Member request is invalid.", StatusCodes.Status400BadRequest);
}
