using Domain.Auditing;

namespace Domain.Entities;

public enum AccountingAccountType
{
    Asset = 0,
    Liability = 1,
    Equity = 2,
    Income = 3,
    Expense = 4
}

public enum AccountingRecordStatus
{
    Draft = 0,
    Posted = 1,
    Approved = 2,
    Rejected = 3,
    Canceled = 4,
    Archived = 5,
    Closed = 6
}

public enum ReceiptVoucherKind
{
    Donation = 0,
    Project = 1,
    Beneficiary = 2,
    Dependent = 3,
    AidCase = 4,
    Sponsorship = 5,
    Program = 6,
    Generic = 7,
    Board = 8,
    ProjectIncome = 9,
    Investment = 10,
    GeneralIncome = 11,
    Qualification = 12,
    Credit = 13,
    Return = 14,
    ReturnedExpense = 15
}

public enum ExpenseVoucherKind
{
    Generic = 0,
    Authorization = 1,
    PaymentOrder = 2,
    Sponsorship = 3,
    Demand = 4,
    CustodySettlement = 5,
    ExchangeOrder = 6,
    ExchangeSponsorship = 7,
    ExchangeSponsorshipMultiple = 8,
    Salary = 9
}

public enum DeferredReceivableKind
{
    Generic = 0,
    ProjectIncome = 1,
    Investment = 2,
    GeneralIncome = 3
}

public enum FinancialReviewKind
{
    IncomeDonation = 0,
    Income = 1,
    Expense = 2,
    MarketingOnline = 3,
    MarketingBank = 4,
    MarketingBalance = 5,
    MarketingLocation = 6,
    MarketingFailed = 7
}

public enum BudgetStatus
{
    Draft = 0,
    Approved = 1,
    Closed = 2
}

public class AccountingSetting : IAuditable
{
    public int Id { get; set; }
    public string FiscalYearName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class FinanceBankAccount : IAuditable
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class FinanceAccount : IAuditable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public AccountingAccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FinanceAccount? ParentAccount { get; set; }
}

public class FinanceCostCenter : IAuditable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class LedgerEntry : IAuditable
{
    public int Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string Description { get; set; } = string.Empty;
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<LedgerLine> Lines { get; set; } = new List<LedgerLine>();
}

public class LedgerLine : IAuditable
{
    public int Id { get; set; }
    public int LedgerEntryId { get; set; }
    public int FinanceAccountId { get; set; }
    public int? FinanceCostCenterId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public LedgerEntry? LedgerEntry { get; set; }
    public FinanceAccount? FinanceAccount { get; set; }
    public FinanceCostCenter? FinanceCostCenter { get; set; }
}

public class ReceiptVoucher : IAuditable
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public ReceiptVoucherKind Kind { get; set; }
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public decimal Amount { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class DeferredReceivable : IAuditable
{
    public int Id { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public DeferredReceivableKind Kind { get; set; }
    public string DebtorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public DateTime DueDate { get; set; }
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class ExpenseVoucher : IAuditable
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public ExpenseVoucherKind Kind { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public decimal Amount { get; set; }
    public string PayeeName { get; set; } = string.Empty;
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class SalaryDisbursement : IAuditable
{
    public int Id { get; set; }
    public DateTime PayrollMonth { get; set; }
    public decimal TotalAmount { get; set; }
    public string ExportFormat { get; set; } = "Preview";
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class FinancialReviewItem : IAuditable
{
    public int Id { get; set; }
    public FinancialReviewKind Kind { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public AccountingRecordStatus Status { get; set; } = AccountingRecordStatus.Draft;
    public string? DecisionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class FinanceBudget : IAuditable
{
    public int Id { get; set; }
    public int FiscalYear { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PlannedIncome { get; set; }
    public decimal PlannedExpenses { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class BankReconciliation : IAuditable
{
    public int Id { get; set; }
    public int FinanceBankAccountId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public decimal StatementBalance { get; set; }
    public decimal BookBalance { get; set; }
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public FinanceBankAccount? FinanceBankAccount { get; set; }
}
