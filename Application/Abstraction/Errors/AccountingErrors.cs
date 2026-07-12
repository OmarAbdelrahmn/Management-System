using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class AccountingErrors
{
    public static readonly Error AccountNotFound = new("Accounting.AccountNotFound", "Finance account was not found.", StatusCodes.Status404NotFound);
    public static readonly Error BankAccountNotFound = new("Accounting.BankAccountNotFound", "Bank account was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CostCenterNotFound = new("Accounting.CostCenterNotFound", "Cost center was not found.", StatusCodes.Status404NotFound);
    public static readonly Error LedgerEntryNotFound = new("Accounting.LedgerEntryNotFound", "Ledger entry was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ReceiptNotFound = new("Accounting.ReceiptNotFound", "Receipt voucher was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DeferredReceivableNotFound = new("Accounting.DeferredReceivableNotFound", "Deferred receivable was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ExpenseNotFound = new("Accounting.ExpenseNotFound", "Expense voucher was not found.", StatusCodes.Status404NotFound);
    public static readonly Error SalaryDisbursementNotFound = new("Accounting.SalaryDisbursementNotFound", "Salary disbursement was not found.", StatusCodes.Status404NotFound);
    public static readonly Error ReviewItemNotFound = new("Accounting.ReviewItemNotFound", "Financial review item was not found.", StatusCodes.Status404NotFound);
    public static readonly Error BudgetNotFound = new("Accounting.BudgetNotFound", "Budget was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateNumber = new("Accounting.DuplicateNumber", "Accounting number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidRequest = new("Accounting.InvalidRequest", "Accounting request is invalid.", StatusCodes.Status400BadRequest);
}
