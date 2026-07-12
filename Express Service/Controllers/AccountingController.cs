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

    [HttpPut("accounts/{id:int}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] SaveFinanceAccountRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveAccountAsync(id, request, cancellationToken));

    [HttpGet("bank-accounts")]
    public async Task<IActionResult> BankAccounts([FromQuery] bool? isActive, CancellationToken cancellationToken) => ToAction(await service.GetBankAccountsAsync(isActive, cancellationToken));

    [HttpPost("bank-accounts")]
    public async Task<IActionResult> SaveBankAccount([FromBody] SaveFinanceBankAccountRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBankAccountAsync(null, request, cancellationToken));

    [HttpPut("bank-accounts/{id:int}")]
    public async Task<IActionResult> UpdateBankAccount(int id, [FromBody] SaveFinanceBankAccountRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBankAccountAsync(id, request, cancellationToken));

    [HttpGet("cost-centers")]
    public async Task<IActionResult> CostCenters([FromQuery] bool? isActive, CancellationToken cancellationToken) => ToAction(await service.GetCostCentersAsync(isActive, cancellationToken));

    [HttpPost("cost-centers")]
    public async Task<IActionResult> SaveCostCenter([FromBody] SaveFinanceCostCenterRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveCostCenterAsync(null, request, cancellationToken));

    [HttpPut("cost-centers/{id:int}")]
    public async Task<IActionResult> UpdateCostCenter(int id, [FromBody] SaveFinanceCostCenterRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveCostCenterAsync(id, request, cancellationToken));

    [HttpGet("ledgers")]
    public async Task<IActionResult> Ledgers([FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetLedgerEntriesAsync(status, cancellationToken));

    [HttpPost("ledgers")]
    public async Task<IActionResult> SaveLedger([FromBody] SaveLedgerEntryRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveLedgerEntryAsync(null, request, cancellationToken));

    [HttpPut("ledgers/{id:int}")]
    public async Task<IActionResult> UpdateLedger(int id, [FromBody] SaveLedgerEntryRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveLedgerEntryAsync(id, request, cancellationToken));

    [HttpGet("receipts")]
    public async Task<IActionResult> Receipts([FromQuery] ReceiptVoucherKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetReceiptsAsync(kind, status, cancellationToken));

    [HttpPost("receipts")]
    public async Task<IActionResult> SaveReceipt([FromBody] SaveReceiptVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReceiptAsync(null, request, cancellationToken));

    [HttpPut("receipts/{id:int}")]
    public async Task<IActionResult> UpdateReceipt(int id, [FromBody] SaveReceiptVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReceiptAsync(id, request, cancellationToken));

    [HttpPost("receipts/{id:int}/decision")]
    public async Task<IActionResult> DecideReceipt(int id, [FromBody] DecideAccountingRecordRequest request, CancellationToken cancellationToken) => ToAction(await service.DecideReceiptAsync(id, request, cancellationToken));

    [HttpPost("receipts/{id:int}/ledger")]
    public async Task<IActionResult> CreateLedgerFromReceipt(int id, [FromBody] CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken) => ToAction(await service.CreateLedgerFromReceiptAsync(id, request, cancellationToken));

    [HttpGet("deferred")]
    public async Task<IActionResult> Deferred([FromQuery] DeferredReceivableKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetDeferredReceivablesAsync(kind, status, cancellationToken));

    [HttpPost("deferred")]
    public async Task<IActionResult> SaveDeferred([FromBody] SaveDeferredReceivableRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveDeferredReceivableAsync(null, request, cancellationToken));

    [HttpPut("deferred/{id:int}")]
    public async Task<IActionResult> UpdateDeferred(int id, [FromBody] SaveDeferredReceivableRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveDeferredReceivableAsync(id, request, cancellationToken));

    [HttpPost("deferred/{id:int}/receive")]
    public async Task<IActionResult> ReceiveDeferred(int id, [FromBody] ReceiveDeferredReceivableRequest request, CancellationToken cancellationToken) => ToAction(await service.ReceiveDeferredReceivableAsync(id, request, cancellationToken));

    [HttpPost("deferred/{id:int}/receive-receipt")]
    public async Task<IActionResult> ReceiveDeferredWithReceipt(int id, [FromBody] ReceiveDeferredReceivableWithReceiptRequest request, CancellationToken cancellationToken) => ToAction(await service.ReceiveDeferredReceivableWithReceiptAsync(id, request, cancellationToken));

    [HttpGet("expenses")]
    public async Task<IActionResult> Expenses([FromQuery] ExpenseVoucherKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetExpensesAsync(kind, status, cancellationToken));

    [HttpPost("expenses")]
    public async Task<IActionResult> SaveExpense([FromBody] SaveExpenseVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveExpenseAsync(null, request, cancellationToken));

    [HttpPut("expenses/{id:int}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] SaveExpenseVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveExpenseAsync(id, request, cancellationToken));

    [HttpPost("expenses/{id:int}/decision")]
    public async Task<IActionResult> DecideExpense(int id, [FromBody] DecideAccountingRecordRequest request, CancellationToken cancellationToken) => ToAction(await service.DecideExpenseAsync(id, request, cancellationToken));

    [HttpPost("expenses/{id:int}/ledger")]
    public async Task<IActionResult> CreateLedgerFromExpense(int id, [FromBody] CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken) => ToAction(await service.CreateLedgerFromExpenseAsync(id, request, cancellationToken));

    [HttpGet("salaries")]
    public async Task<IActionResult> Salaries([FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetSalaryDisbursementsAsync(status, cancellationToken));

    [HttpPost("salaries")]
    public async Task<IActionResult> SaveSalary([FromBody] SaveSalaryDisbursementRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveSalaryDisbursementAsync(null, request, cancellationToken));

    [HttpPut("salaries/{id:int}")]
    public async Task<IActionResult> UpdateSalary(int id, [FromBody] SaveSalaryDisbursementRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveSalaryDisbursementAsync(id, request, cancellationToken));

    [HttpPost("salaries/{id:int}/decision")]
    public async Task<IActionResult> DecideSalary(int id, [FromBody] DecideAccountingRecordRequest request, CancellationToken cancellationToken) => ToAction(await service.DecideSalaryDisbursementAsync(id, request, cancellationToken));

    [HttpPost("salaries/{id:int}/expense")]
    public async Task<IActionResult> CreateExpenseFromSalary(int id, [FromBody] CreateSalaryExpenseVoucherRequest request, CancellationToken cancellationToken) => ToAction(await service.CreateExpenseFromSalaryDisbursementAsync(id, request, cancellationToken));

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews([FromQuery] FinancialReviewKind? kind, [FromQuery] AccountingRecordStatus? status, CancellationToken cancellationToken) => ToAction(await service.GetReviewItemsAsync(kind, status, cancellationToken));

    [HttpPost("reviews")]
    public async Task<IActionResult> SaveReview([FromBody] SaveFinancialReviewItemRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReviewItemAsync(null, request, cancellationToken));

    [HttpPut("reviews/{id:int}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] SaveFinancialReviewItemRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveReviewItemAsync(id, request, cancellationToken));

    [HttpPost("reviews/{id:int}/decision")]
    public async Task<IActionResult> DecideReview(int id, [FromBody] DecideFinancialReviewItemRequest request, CancellationToken cancellationToken) => ToAction(await service.DecideReviewItemAsync(id, request, cancellationToken));

    [HttpGet("budgets")]
    public async Task<IActionResult> Budgets([FromQuery] int? fiscalYear, CancellationToken cancellationToken) => ToAction(await service.GetBudgetsAsync(fiscalYear, cancellationToken));

    [HttpPost("budgets")]
    public async Task<IActionResult> SaveBudget([FromBody] SaveFinanceBudgetRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBudgetAsync(null, request, cancellationToken));

    [HttpPut("budgets/{id:int}")]
    public async Task<IActionResult> UpdateBudget(int id, [FromBody] SaveFinanceBudgetRequest request, CancellationToken cancellationToken) => ToAction(await service.SaveBudgetAsync(id, request, cancellationToken));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
