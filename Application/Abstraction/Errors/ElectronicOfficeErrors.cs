using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class ElectronicOfficeErrors
{
    public static readonly Error InvalidRequest = new("ElectronicOffice.InvalidRequest", "Electronic office request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error ReminderNotFound = new("ElectronicOffice.ReminderNotFound", "Office reminder was not found.", StatusCodes.Status404NotFound);
    public static readonly Error RequestNotFound = new("ElectronicOffice.RequestNotFound", "Office administrative request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateRequestNumber = new("ElectronicOffice.DuplicateRequestNumber", "Office administrative request number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error TransactionNotFound = new("ElectronicOffice.TransactionNotFound", "Office transaction was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateTransactionNumber = new("ElectronicOffice.DuplicateTransactionNumber", "Office transaction number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error LogRecordNotFound = new("ElectronicOffice.LogRecordNotFound", "Office log record was not found.", StatusCodes.Status404NotFound);
}
