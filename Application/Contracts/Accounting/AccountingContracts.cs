using Domain.Entities;

namespace Application.Contracts.Accounting;

public record AccountingDashboardResponse(
    int AccountsCount,
    int BankAccountsCount,
    int LedgerEntriesCount,
    decimal PostedIncome,
    decimal PostedExpenses,
    decimal DeferredDue,
    decimal BudgetDeviation);

public record FinanceAccountResponse(int Id, string Code, string NameAr, string AccountType, bool IsActive);
public record SaveFinanceAccountRequest(string Code, string NameAr, AccountingAccountType AccountType, int? ParentAccountId, bool IsActive);

public record FinanceBankAccountResponse(int Id, string BankName, string AccountName, string Iban, decimal OpeningBalance, bool IsActive);
public record SaveFinanceBankAccountRequest(string BankName, string AccountName, string Iban, decimal OpeningBalance, bool IsActive);

public record FinanceCostCenterResponse(int Id, string Code, string NameAr, bool IsActive);
public record SaveFinanceCostCenterRequest(string Code, string NameAr, bool IsActive);

public record LedgerEntryResponse(int Id, string EntryNumber, DateTime EntryDate, string Description, string Status, decimal DebitTotal, decimal CreditTotal, IReadOnlyList<LedgerLineResponse> Lines);
public record LedgerLineResponse(int Id, int FinanceAccountId, string AccountCode, string AccountNameAr, int? FinanceCostCenterId, string? CostCenterCode, string? CostCenterNameAr, decimal Debit, decimal Credit, string? Notes);
public record SaveLedgerEntryRequest(string? EntryNumber, DateTime? EntryDate, string Description, AccountingRecordStatus Status, IReadOnlyList<SaveLedgerLineRequest> Lines);
public record SaveLedgerLineRequest(int FinanceAccountId, int? FinanceCostCenterId, decimal Debit, decimal Credit, string? Notes);

public record ReceiptVoucherResponse(int Id, string ReceiptNumber, string Kind, DateTime ReceiptDate, decimal Amount, string PayerName, string? ReferenceNumber, string Status, string? Notes);
public record SaveReceiptVoucherRequest(string? ReceiptNumber, ReceiptVoucherKind Kind, DateTime? ReceiptDate, decimal Amount, string PayerName, string? ReferenceNumber, AccountingRecordStatus Status, string? Notes);
public record DecideAccountingRecordRequest(AccountingRecordStatus Status, string? Notes);
public record CreateVoucherLedgerEntryRequest(int DebitAccountId, int CreditAccountId, int? FinanceCostCenterId, AccountingRecordStatus Status, string? Notes);

public record DeferredReceivableResponse(int Id, string ReceivableNumber, string Kind, string DebtorName, decimal Amount, decimal ReceivedAmount, decimal RemainingAmount, DateTime DueDate, string Status, string? Notes);
public record SaveDeferredReceivableRequest(string? ReceivableNumber, DeferredReceivableKind Kind, string DebtorName, decimal Amount, DateTime DueDate, string? Notes);
public record ReceiveDeferredReceivableRequest(decimal ReceivedAmount, string? Notes);
public record ReceiveDeferredReceivableWithReceiptRequest(decimal Amount, ReceiptVoucherKind ReceiptKind, DateTime? ReceiptDate, AccountingRecordStatus ReceiptStatus, string? ReferenceNumber, string? Notes);
public record DeferredReceivableSettlementResponse(DeferredReceivableResponse DeferredReceivable, ReceiptVoucherResponse Receipt);

public record ExpenseVoucherResponse(int Id, string ExpenseNumber, string Kind, DateTime ExpenseDate, decimal Amount, string PayeeName, string Status, string? Notes);
public record SaveExpenseVoucherRequest(string? ExpenseNumber, ExpenseVoucherKind Kind, DateTime? ExpenseDate, decimal Amount, string PayeeName, AccountingRecordStatus Status, string? Notes);

public record SalaryDisbursementResponse(int Id, DateTime PayrollMonth, decimal TotalAmount, string ExportFormat, string Status, string? Notes);
public record SaveSalaryDisbursementRequest(DateTime PayrollMonth, decimal TotalAmount, string ExportFormat, AccountingRecordStatus Status, string? Notes);
public record CreateSalaryExpenseVoucherRequest(DateTime? ExpenseDate, AccountingRecordStatus Status, string? PayeeName, string? Notes);

public record FinancialReviewItemResponse(int Id, string Kind, string ReferenceNumber, decimal Amount, string Status, string? DecisionNotes);
public record SaveFinancialReviewItemRequest(FinancialReviewKind Kind, string ReferenceNumber, decimal Amount, AccountingRecordStatus Status, string? DecisionNotes);
public record DecideFinancialReviewItemRequest(AccountingRecordStatus Status, string? DecisionNotes);

public record FinanceBudgetResponse(int Id, int FiscalYear, string Name, decimal PlannedIncome, decimal PlannedExpenses, decimal ActualIncome, decimal ActualExpenses, decimal Deviation, string Status);
public record SaveFinanceBudgetRequest(int FiscalYear, string Name, decimal PlannedIncome, decimal PlannedExpenses, BudgetStatus Status);
