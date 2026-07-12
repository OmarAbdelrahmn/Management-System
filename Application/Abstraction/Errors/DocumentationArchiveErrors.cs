using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class DocumentationArchiveErrors
{
    public static readonly Error InvalidRequest = new("DocumentationArchive.InvalidRequest", "Documentation/archive request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error ArchiveDocumentNotFound = new("DocumentationArchive.ArchiveDocumentNotFound", "Archive document was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateDocumentNumber = new("DocumentationArchive.DuplicateDocumentNumber", "Archive document number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error CorrespondenceNotFound = new("DocumentationArchive.CorrespondenceNotFound", "Correspondence record was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateMailNumber = new("DocumentationArchive.DuplicateMailNumber", "Correspondence mail number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error OperationNotFound = new("DocumentationArchive.OperationNotFound", "Correspondence operation was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateOperationNumber = new("DocumentationArchive.DuplicateOperationNumber", "Correspondence operation number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error CorrespondenceHasOpenOperations = new("DocumentationArchive.CorrespondenceHasOpenOperations", "Correspondence has open operations and cannot be removed.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidCorrespondenceStatusTransition = new("DocumentationArchive.InvalidCorrespondenceStatusTransition", "Correspondence status transition is not allowed.", StatusCodes.Status409Conflict);
}
