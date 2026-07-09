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

        var receipt = await service.SaveReceiptAsync(null, new SaveReceiptVoucherRequest(null, ReceiptVoucherKind.Donation, new DateTime(2026, 7, 8), 5000, "فاعل خير", null, AccountingRecordStatus.Posted, null));
        var deferred = await service.SaveDeferredReceivableAsync(null, new SaveDeferredReceivableRequest(null, DeferredReceivableKind.ProjectIncome, "جهة داعمة", 2000, new DateTime(2026, 8, 1), null));
        var received = await service.ReceiveDeferredReceivableAsync(deferred.Value.Id, new ReceiveDeferredReceivableRequest(1000, null));
        var expense = await service.SaveExpenseAsync(null, new SaveExpenseVoucherRequest(null, ExpenseVoucherKind.Generic, new DateTime(2026, 7, 9), 1500, "مورد", AccountingRecordStatus.Posted, null));
        var budget = await service.SaveBudgetAsync(null, new SaveFinanceBudgetRequest(2026, "موازنة 2026", 10000, 4000, BudgetStatus.Approved));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(receipt.IsSuccess);
        Assert.True(received.IsSuccess);
        Assert.Equal(1000, received.Value.RemainingAmount);
        Assert.True(expense.IsSuccess);
        Assert.True(budget.IsSuccess);
        Assert.Equal(5000, dashboard.Value.PostedIncome);
        Assert.Equal(1500, dashboard.Value.PostedExpenses);
        Assert.Equal(1000, dashboard.Value.DeferredDue);
    }

    [Fact]
    public async Task ReviewAndSalaryWorkflows_SaveAndDecide()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new AccountingService(dbcontext);

        var salary = await service.SaveSalaryDisbursementAsync(null, new SaveSalaryDisbursementRequest(new DateTime(2026, 7, 1), 12000, "Rajhi", AccountingRecordStatus.Posted, null));
        var review = await service.SaveReviewItemAsync(null, new SaveFinancialReviewItemRequest(FinancialReviewKind.Expense, "EXP-1", 12000, AccountingRecordStatus.Draft, null));
        var decision = await service.DecideReviewItemAsync(review.Value.Id, new DecideFinancialReviewItemRequest(AccountingRecordStatus.Approved, "مطابق"));

        Assert.True(salary.IsSuccess);
        Assert.Equal("Posted", salary.Value.Status);
        Assert.True(decision.IsSuccess);
        Assert.Equal("Approved", decision.Value.Status);
    }
}
