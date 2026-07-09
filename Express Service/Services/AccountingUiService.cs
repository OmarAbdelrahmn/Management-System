using Application.Contracts.Accounting;
using Application.Service.Accounting;
using Domain.Entities;

namespace Express_Service.Services;

public class AccountingUiService(IAccountingService service)
{
    public async Task<AccountingDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<FinanceAccountResponse>> GetAccountsAsync(AccountingAccountType? type = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAccountsAsync(type, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveAccountAsync(int? id, SaveFinanceAccountRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveAccountAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الحساب.") : (false, result.Error.Description);
    }

    public async Task<List<FinanceBankAccountResponse>> GetBankAccountsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetBankAccountsAsync(isActive, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveBankAccountAsync(int? id, SaveFinanceBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveBankAccountAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الحساب البنكي.") : (false, result.Error.Description);
    }

    public async Task<List<FinanceCostCenterResponse>> GetCostCentersAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetCostCentersAsync(isActive, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveCostCenterAsync(int? id, SaveFinanceCostCenterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveCostCenterAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ مركز التكلفة.") : (false, result.Error.Description);
    }

    public async Task<List<LedgerEntryResponse>> GetLedgerEntriesAsync(AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerEntriesAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveLedgerEntryAsync(int? id, SaveLedgerEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveLedgerEntryAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ قيد اليومية.") : (false, result.Error.Description);
    }

    public async Task<List<ReceiptVoucherResponse>> GetReceiptsAsync(ReceiptVoucherKind? kind = null, AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetReceiptsAsync(kind, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveReceiptAsync(int? id, SaveReceiptVoucherRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveReceiptAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ سند القبض.") : (false, result.Error.Description);
    }

    public async Task<List<DeferredReceivableResponse>> GetDeferredReceivablesAsync(DeferredReceivableKind? kind = null, AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetDeferredReceivablesAsync(kind, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveDeferredReceivableAsync(int? id, SaveDeferredReceivableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveDeferredReceivableAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ المطالبة الآجلة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> ReceiveDeferredReceivableAsync(int id, decimal amount, string? notes = null, CancellationToken cancellationToken = default)
    {
        var result = await service.ReceiveDeferredReceivableAsync(id, new ReceiveDeferredReceivableRequest(amount, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل استلام المطالبة.") : (false, result.Error.Description);
    }

    public async Task<List<ExpenseVoucherResponse>> GetExpensesAsync(ExpenseVoucherKind? kind = null, AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetExpensesAsync(kind, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveExpenseAsync(int? id, SaveExpenseVoucherRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveExpenseAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ سند الصرف.") : (false, result.Error.Description);
    }

    public async Task<List<SalaryDisbursementResponse>> GetSalaryDisbursementsAsync(AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetSalaryDisbursementsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSalaryDisbursementAsync(int? id, SaveSalaryDisbursementRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveSalaryDisbursementAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ صرف الرواتب.") : (false, result.Error.Description);
    }

    public async Task<List<FinancialReviewItemResponse>> GetReviewItemsAsync(FinancialReviewKind? kind = null, AccountingRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetReviewItemsAsync(kind, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveReviewItemAsync(int? id, SaveFinancialReviewItemRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveReviewItemAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ سجل المراجعة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideReviewItemAsync(int id, AccountingRecordStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await service.DecideReviewItemAsync(id, new DecideFinancialReviewItemRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث سجل المراجعة.") : (false, result.Error.Description);
    }

    public async Task<List<FinanceBudgetResponse>> GetBudgetsAsync(int? fiscalYear = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetBudgetsAsync(fiscalYear, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveBudgetAsync(int? id, SaveFinanceBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveBudgetAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الموازنة.") : (false, result.Error.Description);
    }
}
