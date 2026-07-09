using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class ExecutiveSupervisionErrors
{
    public static readonly Error InvalidRequest = new("ExecutiveSupervision.InvalidRequest", "Executive supervision request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error FoundationDocumentNotFound = new("ExecutiveSupervision.FoundationDocumentNotFound", "Foundation document was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateFoundationDocument = new("ExecutiveSupervision.DuplicateFoundationDocument", "Foundation document code is already used.", StatusCodes.Status409Conflict);
    public static readonly Error AidCommitteeEntryNotFound = new("ExecutiveSupervision.AidCommitteeEntryNotFound", "Aid committee credit entry was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateAidCommitteeEntry = new("ExecutiveSupervision.DuplicateAidCommitteeEntry", "Aid committee entry number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error ApprovalNotFound = new("ExecutiveSupervision.ApprovalNotFound", "Executive approval request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateApprovalRequest = new("ExecutiveSupervision.DuplicateApprovalRequest", "Executive approval request number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error AuthorizationNotFound = new("ExecutiveSupervision.AuthorizationNotFound", "Payment authorization was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateAuthorization = new("ExecutiveSupervision.DuplicateAuthorization", "Payment authorization number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error DecisionNotFound = new("ExecutiveSupervision.DecisionNotFound", "Administrative decision was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateDecision = new("ExecutiveSupervision.DuplicateDecision", "Administrative decision number is already used.", StatusCodes.Status409Conflict);
}
