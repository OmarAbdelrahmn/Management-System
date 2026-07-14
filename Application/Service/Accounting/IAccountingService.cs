using Application.Abstraction;
using Application.Contracts.Accounting;
using Domain.Entities;

namespace Application.Service.Accounting;

public interface IAccountingService
{
    Task<Result<AccountingDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FiscalPeriodResponse>>> GetFiscalPeriodsAsync(CancellationToken cancellationToken = default);
    Task<Result<FiscalPeriodResponse>> SaveFiscalPeriodAsync(int? id, SaveFiscalPeriodRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinanceAccountResponse>>> GetAccountsAsync(AccountingAccountType? type, CancellationToken cancellationToken = default);
    Task<Result<FinanceAccountResponse>> SaveAccountAsync(int? id, SaveFinanceAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinanceBankAccountResponse>>> GetBankAccountsAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<Result<FinanceBankAccountResponse>> SaveBankAccountAsync(int? id, SaveFinanceBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinanceCostCenterResponse>>> GetCostCentersAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<Result<FinanceCostCenterResponse>> SaveCostCenterAsync(int? id, SaveFinanceCostCenterRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LedgerEntryResponse>>> GetLedgerEntriesAsync(AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<LedgerEntryResponse>> SaveLedgerEntryAsync(int? id, SaveLedgerEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<LedgerEntryResponse>> SetLedgerPostingAsync(int id, SetLedgerPostingRequest request, CancellationToken cancellationToken = default);
    Task<Result<LedgerEntryResponse>> CreateOpeningBalanceAsync(CreateOpeningBalanceRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ReceiptVoucherResponse>>> GetReceiptsAsync(ReceiptVoucherKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<ReceiptVoucherResponse>> SaveReceiptAsync(int? id, SaveReceiptVoucherRequest request, CancellationToken cancellationToken = default);
    Task<Result<ReceiptVoucherResponse>> DecideReceiptAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result<LedgerEntryResponse>> CreateLedgerFromReceiptAsync(int id, CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DeferredReceivableResponse>>> GetDeferredReceivablesAsync(DeferredReceivableKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<DeferredReceivableResponse>> SaveDeferredReceivableAsync(int? id, SaveDeferredReceivableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DeferredReceivableResponse>> ReceiveDeferredReceivableAsync(int id, ReceiveDeferredReceivableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DeferredReceivableSettlementResponse>> ReceiveDeferredReceivableWithReceiptAsync(int id, ReceiveDeferredReceivableWithReceiptRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ExpenseVoucherResponse>>> GetExpensesAsync(ExpenseVoucherKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<ExpenseVoucherResponse>> SaveExpenseAsync(int? id, SaveExpenseVoucherRequest request, CancellationToken cancellationToken = default);
    Task<Result<ExpenseVoucherResponse>> DecideExpenseAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result<LedgerEntryResponse>> CreateLedgerFromExpenseAsync(int id, CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SalaryDisbursementResponse>>> GetSalaryDisbursementsAsync(AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<SalaryDisbursementResponse>> SaveSalaryDisbursementAsync(int? id, SaveSalaryDisbursementRequest request, CancellationToken cancellationToken = default);
    Task<Result<SalaryDisbursementResponse>> DecideSalaryDisbursementAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result<ExpenseVoucherResponse>> CreateExpenseFromSalaryDisbursementAsync(int id, CreateSalaryExpenseVoucherRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinancialReviewItemResponse>>> GetReviewItemsAsync(FinancialReviewKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<FinancialReviewItemResponse>> SaveReviewItemAsync(int? id, SaveFinancialReviewItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<FinancialReviewItemResponse>> DecideReviewItemAsync(int id, DecideFinancialReviewItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinanceBudgetResponse>>> GetBudgetsAsync(int? fiscalYear, CancellationToken cancellationToken = default);
    Task<Result<FinanceBudgetResponse>> SaveBudgetAsync(int? id, SaveFinanceBudgetRequest request, CancellationToken cancellationToken = default);
    Task<Result<FinanceBudgetResponse>> DecideBudgetAsync(int id, DecideBudgetRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BankReconciliationResponse>>> GetBankReconciliationsAsync(int? bankAccountId, CancellationToken cancellationToken = default);
    Task<Result<BankReconciliationResponse>> SaveBankReconciliationAsync(int? id, SaveBankReconciliationRequest request, CancellationToken cancellationToken = default);
    Task<Result<BankReconciliationResponse>> ApproveBankReconciliationAsync(int id, ApproveBankReconciliationRequest request, CancellationToken cancellationToken = default);
}
