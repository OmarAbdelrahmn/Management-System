using Application.Contracts.Accounting;
using Application.Service.Accounting;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class AccountingServiceTests
{
    [Fact]
    public async Task SaveLedgerEntryAsync_RequiresBalancedLinesAndGeneratesNumber()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);
        var cash = await service.SaveAccountAsync(null, new SaveFinanceAccountRequest("1000", "الصندوق", AccountingAccountType.Asset, null, true));
        var income = await service.SaveAccountAsync(null, new SaveFinanceAccountRequest("4000", "التبرعات", AccountingAccountType.Income, null, true));
        var center = await service.SaveCostCenterAsync(null, new SaveFinanceCostCenterRequest("PRG", "برامج", true));

        var ledger = await service.SaveLedgerEntryAsync(null, new SaveLedgerEntryRequest(
            null,
            new DateTime(2026, 7, 8),
            "قيد تبرع",
            AccountingRecordStatus.Posted,
            [
                new SaveLedgerLineRequest(cash.Value.Id, center.Value.Id, 1000, 0, "قبض"),
                new SaveLedgerLineRequest(income.Value.Id, center.Value.Id, 0, 1000, "إيراد")
            ]));
        var entries = (await service.GetLedgerEntriesAsync(AccountingRecordStatus.Posted)).Value.ToList();

        Assert.True(ledger.IsSuccess);
        Assert.StartsWith("JRN-2026-", ledger.Value.EntryNumber);
        Assert.Equal(ledger.Value.DebitTotal, ledger.Value.CreditTotal);
        Assert.Equal(2, ledger.Value.Lines.Count);
        Assert.Contains(ledger.Value.Lines, x => x.AccountCode == "1000" && x.AccountNameAr == "الصندوق" && x.CostCenterCode == "PRG");
        Assert.Contains(entries.Single().Lines, x => x.Notes == "إيراد" && x.Credit == 1000);
    }

    [Fact]
    public async Task ReceiptsDeferredExpensesAndBudgets_UpdateDashboard()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);

        var receipt = await service.SaveReceiptAsync(null, new SaveReceiptVoucherRequest(null, ReceiptVoucherKind.Donation, new DateTime(2026, 7, 8), 5000, "فاعل خير", null, AccountingRecordStatus.Draft, null));
        var approvedReceipt = await service.DecideReceiptAsync(receipt.Value.Id, new DecideAccountingRecordRequest(AccountingRecordStatus.Approved, "مراجع"));
        var deferred = await service.SaveDeferredReceivableAsync(null, new SaveDeferredReceivableRequest(null, DeferredReceivableKind.ProjectIncome, "جهة داعمة", 2000, new DateTime(2026, 8, 1), null));
        var received = await service.ReceiveDeferredReceivableAsync(deferred.Value.Id, new ReceiveDeferredReceivableRequest(1000, null));
        var expense = await service.SaveExpenseAsync(null, new SaveExpenseVoucherRequest(null, ExpenseVoucherKind.Generic, new DateTime(2026, 7, 9), 1500, "مورد", AccountingRecordStatus.Draft, null));
        var approvedExpense = await service.DecideExpenseAsync(expense.Value.Id, new DecideAccountingRecordRequest(AccountingRecordStatus.Approved, "مطابق"));
        var budget = await service.SaveBudgetAsync(null, new SaveFinanceBudgetRequest(2026, "موازنة 2026", 10000, 4000, BudgetStatus.Approved));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(receipt.IsSuccess);
        Assert.Equal("Approved", approvedReceipt.Value.Status);
        Assert.True(received.IsSuccess);
        Assert.Equal(1000, received.Value.RemainingAmount);
        Assert.True(expense.IsSuccess);
        Assert.Equal("Approved", approvedExpense.Value.Status);
        Assert.True(budget.IsSuccess);
        Assert.Equal(5000, dashboard.Value.PostedIncome);
        Assert.Equal(1500, dashboard.Value.PostedExpenses);
        Assert.Equal(1000, dashboard.Value.DeferredDue);
    }

    [Fact]
    public async Task ReceiveDeferredReceivableWithReceipt_CreatesReceiptAndClosesWhenFullyPaid()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);
        var deferred = await service.SaveDeferredReceivableAsync(null, new SaveDeferredReceivableRequest("DEF-X", DeferredReceivableKind.ProjectIncome, "جهة داعمة", 1200, new DateTime(2026, 8, 1), null));

        var partial = await service.ReceiveDeferredReceivableWithReceiptAsync(deferred.Value.Id, new ReceiveDeferredReceivableWithReceiptRequest(500, ReceiptVoucherKind.Project, new DateTime(2026, 7, 10), AccountingRecordStatus.Posted, null, "دفعة أولى"));
        var final = await service.ReceiveDeferredReceivableWithReceiptAsync(deferred.Value.Id, new ReceiveDeferredReceivableWithReceiptRequest(700, ReceiptVoucherKind.Project, new DateTime(2026, 7, 20), AccountingRecordStatus.Approved, "BANK-7", "دفعة أخيرة"));
        var overpay = await service.ReceiveDeferredReceivableWithReceiptAsync(deferred.Value.Id, new ReceiveDeferredReceivableWithReceiptRequest(1, ReceiptVoucherKind.Project, null, AccountingRecordStatus.Posted, null, null));
        var receipts = (await service.GetReceiptsAsync(ReceiptVoucherKind.Project, null)).Value.ToList();
        var closedDeferred = Assert.Single((await service.GetDeferredReceivablesAsync(null, AccountingRecordStatus.Closed)).Value);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(partial.IsSuccess);
        Assert.StartsWith("RCV-2026-", partial.Value.Receipt.ReceiptNumber);
        Assert.Equal(700, partial.Value.DeferredReceivable.RemainingAmount);
        Assert.True(final.IsSuccess);
        Assert.Equal("BANK-7", final.Value.Receipt.ReferenceNumber);
        Assert.Equal("Closed", final.Value.DeferredReceivable.Status);
        Assert.True(overpay.IsFailure);
        Assert.Equal(2, receipts.Count);
        Assert.Equal(1200, closedDeferred.ReceivedAmount);
        Assert.Equal(1200, dashboard.Value.PostedIncome);
    }

    [Fact]
    public async Task ReviewAndSalaryWorkflows_SaveAndDecide()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);

        var salary = await service.SaveSalaryDisbursementAsync(null, new SaveSalaryDisbursementRequest(new DateTime(2026, 7, 1), 12000, "Rajhi", AccountingRecordStatus.Draft, null));
        var salaryDecision = await service.DecideSalaryDisbursementAsync(salary.Value.Id, new DecideAccountingRecordRequest(AccountingRecordStatus.Posted, "ملف البنك جاهز"));
        var review = await service.SaveReviewItemAsync(null, new SaveFinancialReviewItemRequest(FinancialReviewKind.Expense, "EXP-1", 12000, AccountingRecordStatus.Draft, null));
        var decision = await service.DecideReviewItemAsync(review.Value.Id, new DecideFinancialReviewItemRequest(AccountingRecordStatus.Approved, "مطابق"));

        Assert.True(salary.IsSuccess);
        Assert.Equal("Posted", salaryDecision.Value.Status);
        Assert.Equal("ملف البنك جاهز", salaryDecision.Value.Notes);
        Assert.True(decision.IsSuccess);
        Assert.Equal("Approved", decision.Value.Status);
    }

    [Fact]
    public async Task CreateExpenseFromSalaryDisbursement_CreatesVoucherAndBlocksDuplicates()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);
        var salary = await service.SaveSalaryDisbursementAsync(null, new SaveSalaryDisbursementRequest(new DateTime(2026, 7, 1), 18000, "BankExport", AccountingRecordStatus.Draft, null));

        var beforeApproval = await service.CreateExpenseFromSalaryDisbursementAsync(salary.Value.Id, new CreateSalaryExpenseVoucherRequest(new DateTime(2026, 7, 25), AccountingRecordStatus.Approved, null, null));
        await service.DecideSalaryDisbursementAsync(salary.Value.Id, new DecideAccountingRecordRequest(AccountingRecordStatus.Approved, "جاهز للصرف"));
        var expense = await service.CreateExpenseFromSalaryDisbursementAsync(salary.Value.Id, new CreateSalaryExpenseVoucherRequest(new DateTime(2026, 7, 25), AccountingRecordStatus.Approved, "الموظفون", "سند صرف رواتب"));
        var duplicate = await service.CreateExpenseFromSalaryDisbursementAsync(salary.Value.Id, new CreateSalaryExpenseVoucherRequest(new DateTime(2026, 7, 25), AccountingRecordStatus.Approved, null, null));
        var salaryExpenses = (await service.GetExpensesAsync(ExpenseVoucherKind.Salary, AccountingRecordStatus.Approved)).Value.ToList();
        var postedSalary = Assert.Single((await service.GetSalaryDisbursementsAsync(AccountingRecordStatus.Posted)).Value);

        Assert.True(beforeApproval.IsFailure);
        Assert.True(expense.IsSuccess);
        Assert.Equal("EXP-SAL-202607", expense.Value.ExpenseNumber);
        Assert.Equal("Salary", expense.Value.Kind);
        Assert.Equal(18000, expense.Value.Amount);
        Assert.True(duplicate.IsFailure);
        Assert.Single(salaryExpenses);
        Assert.Equal(salary.Value.Id, postedSalary.Id);
    }

    [Fact]
    public async Task CreateLedgerFromReceiptAndExpense_PostsApprovedVouchers()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);
        var cash = await service.SaveAccountAsync(null, new SaveFinanceAccountRequest("1000", "الصندوق", AccountingAccountType.Asset, null, true));
        var income = await service.SaveAccountAsync(null, new SaveFinanceAccountRequest("4000", "التبرعات", AccountingAccountType.Income, null, true));
        var expenseAccount = await service.SaveAccountAsync(null, new SaveFinanceAccountRequest("5000", "مصروفات عامة", AccountingAccountType.Expense, null, true));

        var receipt = await service.SaveReceiptAsync(null, new SaveReceiptVoucherRequest("RCV-X", ReceiptVoucherKind.Donation, new DateTime(2026, 7, 8), 800, "فاعل خير", null, AccountingRecordStatus.Approved, null));
        var receiptLedger = await service.CreateLedgerFromReceiptAsync(receipt.Value.Id, new CreateVoucherLedgerEntryRequest(cash.Value.Id, income.Value.Id, null, AccountingRecordStatus.Posted, "ترحيل قبض"));
        var duplicateReceiptLedger = await service.CreateLedgerFromReceiptAsync(receipt.Value.Id, new CreateVoucherLedgerEntryRequest(cash.Value.Id, income.Value.Id, null, AccountingRecordStatus.Posted, "تكرار"));
        var expense = await service.SaveExpenseAsync(null, new SaveExpenseVoucherRequest("EXP-X", ExpenseVoucherKind.Generic, new DateTime(2026, 7, 9), 300, "مورد", AccountingRecordStatus.Approved, null));
        var expenseLedger = await service.CreateLedgerFromExpenseAsync(expense.Value.Id, new CreateVoucherLedgerEntryRequest(expenseAccount.Value.Id, cash.Value.Id, null, AccountingRecordStatus.Posted, "ترحيل صرف"));
        var ledgers = (await service.GetLedgerEntriesAsync(AccountingRecordStatus.Posted)).Value.ToList();
        var postedReceipts = (await service.GetReceiptsAsync(null, AccountingRecordStatus.Posted)).Value.ToList();
        var postedExpenses = (await service.GetExpensesAsync(null, AccountingRecordStatus.Posted)).Value.ToList();

        Assert.True(receiptLedger.IsSuccess);
        Assert.False(duplicateReceiptLedger.IsSuccess);
        Assert.True(expenseLedger.IsSuccess);
        Assert.Equal("JRN-RCV-X", receiptLedger.Value.EntryNumber);
        Assert.Equal("JRN-EXP-X", expenseLedger.Value.EntryNumber);
        Assert.Contains(receiptLedger.Value.Lines, x => x.Debit == 800 && x.AccountCode == "1000");
        Assert.Contains(expenseLedger.Value.Lines, x => x.Credit == 300 && x.AccountCode == "1000");
        Assert.Single(postedReceipts);
        Assert.Single(postedExpenses);
        Assert.Equal(2, ledgers.Count);
    }
}
