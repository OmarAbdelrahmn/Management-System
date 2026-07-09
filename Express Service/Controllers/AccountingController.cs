using Application.Contracts.Accounting;
using Application.Service.Accounting;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class AccountingController(IAccountingService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken) => ToAction(await service.GetDashboardAsync(cancellationToken));

    [HttpGet("accounts")]
    public async Task<IActionResult> Accounts([FromQuery] AccountingAccountType? type, CancellationToken cancellationToken) => ToAction(await service.GetAccountsAsync(type, cancellationToken));

    [HttpPost("accounts")]
    public async Task<IActionResult> SaveAccount([FromBody] SaveFinanceAccountRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveAccountAsync(null, request, cancellationToken));

    [HttpGet("bank-accounts")]
    public async Task<IActionResult> BankAccounts([FromQuery] bool? isActive, CancellationToken cancellationToken) => ToAction(await service.GetBankAccountsAsync(isActive, cancellationToken));

    [HttpPost("bank-accounts")]
    public async Task<IActionResult> SaveBankAccount([FromBody] SaveFinanceBankAccountRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBankAccountAsync(null, request, cancellationToken));

    [HttpGet("cost-centers")]
    public async Task<IActionResult> CostCenters([FromQuery] bool? isActive, CancellationToken cancellationToken) => ToAction(await service.GetCostCentersAsync(isActive, cancellationToken));

    [HttpPost("cost-centers")]
    public async Task<IActionResult> SaveCostCenter([FromBody] SaveFinanceCostCenterRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveCostCenterAsync(null, request, cancellationToken));

    [HttpGet("ledgers")]
    public async Task<IActionResult> Ledgers([FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetLedgerEntriesAsync(status, cancellationToken));

    [HttpPost("ledgers")]
    public async Task<IActionResult> SaveLedger([FromBody] SaveLedgerEntryRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveLedgerEntryAsync(null, request, cancellationToken));

    [HttpGet("receipts")]
    public async Task<IActionResult> Receipts([FromQuery] ReceiptVoucherKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetReceiptsAsync(kind, status, cancellationToken));

    [HttpPost("receipts")]
    public async Task<IActionResult> SaveReceipt([FromBody] SaveReceiptVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReceiptAsync(null, request, cancellationToken));

    [HttpGet("deferred")]
    public async Task<IActionResult> Deferred([FromQuery] DeferredReceivableKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetDeferredReceivablesAsync(kind, status, cancellationToken));

    [HttpPost("deferred")]
    public async Task<IActionResult> SaveDeferred([FromBody] SaveDeferredReceivableRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveDeferredReceivableAsync(null, request, cancellationToken));

    [HttpPost("deferred/{id:int}/receive")]
    public async Task<IActionResult> ReceiveDeferred(int id, [FromBody] ReceiveDeferredReceivableRequest request, CancellationToken cancellationToken) => ToAction(await service.ReceiveDeferredReceivableAsync(id, request, cancellationToken));

    [HttpGet("expenses")]
    public async Task<IActionResult> Expenses([FromQuery] ExpenseVoucherKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetExpensesAsync(kind, status, cancellationToken));

    [HttpPost("expenses")]
    public async Task<IActionResult> SaveExpense([FromBody] SaveExpenseVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveExpenseAsync(null, request, cancellationToken));

    [HttpGet("salaries")]
    public async Task<IActionResult> Salaries([FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetSalaryDisbursementsAsync(status, cancellationToken));

    [HttpPost("salaries")]
    public async Task<IActionResult> SaveSalary([FromBody] SaveSalaryDisbursementRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveSalaryDisbursementAsync(null, request, cancellationToken));

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews([FromQuery] FinancialReviewKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetReviewItemsAsync(kind, status, cancellationToken));

    [HttpPost("reviews")]
    public async Task<IActionResult> SaveReview([FromBody] SaveFinancialReviewItemRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReviewItemAsync(null, request, cancellationToken));

    [HttpPost("reviews/{id:int}/decision")]
    public async Task<IActionResult> DecideReview(int id, [FromBody] DecideFinancialReviewItemRequest request, CancellationToken cancellationToken) => ToAction(await service.DecideReviewItemAsync(id, request, cancellationToken));

    [HttpGet("budgets")]
    public async Task<IActionResult> Budgets([FromQuery] int? fiscalYear, CancellationToken cancellationToken) => ToAction(await service.GetBudgetsAsync(fiscalYear, cancellationToken));

    [HttpPost("budgets")]
    public async Task<IActionResult> SaveBudget([FromBody] SaveFinanceBudgetRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBudgetAsync(null, request, cancellationToken));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
