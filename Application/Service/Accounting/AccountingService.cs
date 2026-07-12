using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Accounting;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Accounting;

public class AccountingService(ApplicationDbcontext dbcontext) : IAccountingService
{
    public async Task<Result<AccountingDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var accountsCount = await dbcontext.FinanceAccounts.CountAsync(cancellationToken);
        var bankAccountsCount = await dbcontext.FinanceBankAccounts.CountAsync(x => x.IsActive, cancellationToken);
        var ledgerEntriesCount = await dbcontext.LedgerEntries.CountAsync(cancellationToken);
        var postedIncome = await dbcontext.ReceiptVouchers.Where(x => x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved).SumAsync(x => x.Amount, cancellationToken);
        var postedExpenses = await dbcontext.ExpenseVouchers.Where(x => x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved || x.Status == AccountingRecordStatus.Closed).SumAsync(x => x.Amount, cancellationToken);
        var deferredDue = await dbcontext.DeferredReceivables.Where(x => x.Status != AccountingRecordStatus.Closed).SumAsync(x => x.Amount - x.ReceivedAmount, cancellationToken);
        var plannedIncome = await dbcontext.FinanceBudgets.Where(x => x.Status == BudgetStatus.Approved).SumAsync(x => x.PlannedIncome, cancellationToken);
        var plannedExpenses = await dbcontext.FinanceBudgets.Where(x => x.Status == BudgetStatus.Approved).SumAsync(x => x.PlannedExpenses, cancellationToken);

        return Result.Success(new AccountingDashboardResponse(accountsCount, bankAccountsCount, ledgerEntriesCount, postedIncome, postedExpenses, deferredDue, (plannedIncome - postedIncome) - (plannedExpenses - postedExpenses)));
    }

    public async Task<Result<IEnumerable<FinanceAccountResponse>>> GetAccountsAsync(AccountingAccountType? type, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinanceAccounts.AsNoTracking().AsQueryable();
        if (type.HasValue)
            query = query.Where(x => x.AccountType == type.Value);

        return Result.Success<IEnumerable<FinanceAccountResponse>>(await query.OrderBy(x => x.Code).Select(x => MapAccount(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FinanceAccountResponse>> SaveAccountAsync(int? id, SaveFinanceAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<FinanceAccountResponse>(AccountingErrors.InvalidRequest);
        if (request.ParentAccountId.HasValue && !await dbcontext.FinanceAccounts.AnyAsync(x => x.Id == request.ParentAccountId.Value, cancellationToken))
            return Result.Failure<FinanceAccountResponse>(AccountingErrors.AccountNotFound);
        var code = request.Code.Trim();
        if (await dbcontext.FinanceAccounts.AnyAsync(x => x.Code == code && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<FinanceAccountResponse>(AccountingErrors.DuplicateNumber);

        FinanceAccount account;
        if (id.HasValue)
        {
            account = await dbcontext.FinanceAccounts.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (account is null)
                return Result.Failure<FinanceAccountResponse>(AccountingErrors.AccountNotFound);
        }
        else
        {
            account = new FinanceAccount();
            dbcontext.FinanceAccounts.Add(account);
        }

        account.Code = code;
        account.NameAr = request.NameAr.Trim();
        account.AccountType = request.AccountType;
        account.ParentAccountId = request.ParentAccountId;
        account.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAccount(account));
    }

    public async Task<Result<IEnumerable<FinanceBankAccountResponse>>> GetBankAccountsAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinanceBankAccounts.AsNoTracking().AsQueryable();
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<FinanceBankAccountResponse>>(await query.OrderBy(x => x.BankName).Select(x => MapBankAccount(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FinanceBankAccountResponse>> SaveBankAccountAsync(int? id, SaveFinanceBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.BankName) || string.IsNullOrWhiteSpace(request.AccountName) || string.IsNullOrWhiteSpace(request.Iban))
            return Result.Failure<FinanceBankAccountResponse>(AccountingErrors.InvalidRequest);

        FinanceBankAccount bankAccount;
        if (id.HasValue)
        {
            bankAccount = await dbcontext.FinanceBankAccounts.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (bankAccount is null)
                return Result.Failure<FinanceBankAccountResponse>(AccountingErrors.BankAccountNotFound);
        }
        else
        {
            bankAccount = new FinanceBankAccount();
            dbcontext.FinanceBankAccounts.Add(bankAccount);
        }

        bankAccount.BankName = request.BankName.Trim();
        bankAccount.AccountName = request.AccountName.Trim();
        bankAccount.Iban = request.Iban.Trim();
        bankAccount.OpeningBalance = request.OpeningBalance;
        bankAccount.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapBankAccount(bankAccount));
    }

    public async Task<Result<IEnumerable<FinanceCostCenterResponse>>> GetCostCentersAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinanceCostCenters.AsNoTracking().AsQueryable();
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<FinanceCostCenterResponse>>(await query.OrderBy(x => x.Code).Select(x => MapCostCenter(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FinanceCostCenterResponse>> SaveCostCenterAsync(int? id, SaveFinanceCostCenterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.NameAr))
            return Result.Failure<FinanceCostCenterResponse>(AccountingErrors.InvalidRequest);
        var code = request.Code.Trim();
        if (await dbcontext.FinanceCostCenters.AnyAsync(x => x.Code == code && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<FinanceCostCenterResponse>(AccountingErrors.DuplicateNumber);

        FinanceCostCenter costCenter;
        if (id.HasValue)
        {
            costCenter = await dbcontext.FinanceCostCenters.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (costCenter is null)
                return Result.Failure<FinanceCostCenterResponse>(AccountingErrors.CostCenterNotFound);
        }
        else
        {
            costCenter = new FinanceCostCenter();
            dbcontext.FinanceCostCenters.Add(costCenter);
        }

        costCenter.Code = code;
        costCenter.NameAr = request.NameAr.Trim();
        costCenter.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCostCenter(costCenter));
    }

    public async Task<Result<IEnumerable<LedgerEntryResponse>>> GetLedgerEntriesAsync(AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.LedgerEntries
            .AsNoTracking()
            .Include(x => x.Lines)
            .ThenInclude(x => x.FinanceAccount)
            .Include(x => x.Lines)
            .ThenInclude(x => x.FinanceCostCenter)
            .AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<LedgerEntryResponse>>(await query.OrderByDescending(x => x.EntryDate).Select(x => MapLedgerEntry(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<LedgerEntryResponse>> SaveLedgerEntryAsync(int? id, SaveLedgerEntryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description) || request.Lines.Count < 2)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);
        var debit = request.Lines.Sum(x => x.Debit);
        var credit = request.Lines.Sum(x => x.Credit);
        if (debit <= 0 || debit != credit || request.Lines.Any(x => x.Debit < 0 || x.Credit < 0 || x.Debit > 0 && x.Credit > 0))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);
        var accountIds = request.Lines.Select(x => x.FinanceAccountId).Distinct().ToList();
        if (await dbcontext.FinanceAccounts.CountAsync(x => accountIds.Contains(x.Id), cancellationToken) != accountIds.Count)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.AccountNotFound);

        var entryNumber = string.IsNullOrWhiteSpace(request.EntryNumber) ? await GenerateNumberAsync("JRN", dbcontext.LedgerEntries, cancellationToken) : request.EntryNumber.Trim();
        if (await dbcontext.LedgerEntries.AnyAsync(x => x.EntryNumber == entryNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.DuplicateNumber);

        LedgerEntry entry;
        if (id.HasValue)
        {
            entry = await dbcontext.LedgerEntries.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (entry is null)
                return Result.Failure<LedgerEntryResponse>(AccountingErrors.LedgerEntryNotFound);
            dbcontext.LedgerLines.RemoveRange(entry.Lines);
        }
        else
        {
            entry = new LedgerEntry();
            dbcontext.LedgerEntries.Add(entry);
        }

        entry.EntryNumber = entryNumber;
        entry.EntryDate = request.EntryDate ?? DateTime.UtcNow.AddHours(3);
        entry.Description = request.Description.Trim();
        entry.Status = request.Status;
        entry.Lines = request.Lines.Select(x => new LedgerLine { FinanceAccountId = x.FinanceAccountId, FinanceCostCenterId = x.FinanceCostCenterId, Debit = x.Debit, Credit = x.Credit, Notes = x.Notes?.Trim() }).ToList();
        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadLedgerLinesAsync(entry, cancellationToken);
        return Result.Success(MapLedgerEntry(entry));
    }

    public async Task<Result<IEnumerable<ReceiptVoucherResponse>>> GetReceiptsAsync(ReceiptVoucherKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ReceiptVouchers.AsNoTracking().AsQueryable();
        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<ReceiptVoucherResponse>>(await query.OrderByDescending(x => x.ReceiptDate).Select(x => MapReceipt(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<ReceiptVoucherResponse>> SaveReceiptAsync(int? id, SaveReceiptVoucherRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.PayerName))
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.InvalidRequest);
        var receiptNumber = string.IsNullOrWhiteSpace(request.ReceiptNumber) ? await GenerateNumberAsync("RCV", dbcontext.ReceiptVouchers, cancellationToken) : request.ReceiptNumber.Trim();
        if (await dbcontext.ReceiptVouchers.AnyAsync(x => x.ReceiptNumber == receiptNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.DuplicateNumber);

        ReceiptVoucher receipt;
        if (id.HasValue)
        {
            receipt = await dbcontext.ReceiptVouchers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (receipt is null)
                return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.ReceiptNotFound);
        }
        else
        {
            receipt = new ReceiptVoucher();
            dbcontext.ReceiptVouchers.Add(receipt);
        }

        receipt.ReceiptNumber = receiptNumber;
        receipt.Kind = request.Kind;
        receipt.ReceiptDate = request.ReceiptDate ?? DateTime.UtcNow.AddHours(3);
        receipt.Amount = request.Amount;
        receipt.PayerName = request.PayerName.Trim();
        receipt.ReferenceNumber = request.ReferenceNumber?.Trim();
        receipt.Status = request.Status;
        receipt.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReceipt(receipt));
    }

    public async Task<Result<ReceiptVoucherResponse>> DecideReceiptAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default)
    {
        var receipt = await dbcontext.ReceiptVouchers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (receipt is null)
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.ReceiptNotFound);

        receipt.Status = request.Status;
        receipt.Notes = request.Notes?.Trim() ?? receipt.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReceipt(receipt));
    }

    public async Task<Result<LedgerEntryResponse>> CreateLedgerFromReceiptAsync(int id, CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken = default)
    {
        var receipt = await dbcontext.ReceiptVouchers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (receipt is null)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.ReceiptNotFound);
        if (receipt.Status is not (AccountingRecordStatus.Approved or AccountingRecordStatus.Posted) || request.Status is not (AccountingRecordStatus.Approved or AccountingRecordStatus.Posted))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);

        var ledger = await SaveLedgerEntryAsync(null, new SaveLedgerEntryRequest(
            $"JRN-{receipt.ReceiptNumber}",
            receipt.ReceiptDate,
            $"ترحيل سند قبض {receipt.ReceiptNumber} - {receipt.PayerName}",
            request.Status,
            [
                new SaveLedgerLineRequest(request.DebitAccountId, request.FinanceCostCenterId, receipt.Amount, 0, request.Notes ?? receipt.Notes),
                new SaveLedgerLineRequest(request.CreditAccountId, request.FinanceCostCenterId, 0, receipt.Amount, request.Notes ?? receipt.Notes)
            ]), cancellationToken);

        if (!ledger.IsSuccess)
            return ledger;

        receipt.Status = AccountingRecordStatus.Posted;
        receipt.Notes = request.Notes?.Trim() ?? receipt.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return ledger;
    }

    public async Task<Result<IEnumerable<DeferredReceivableResponse>>> GetDeferredReceivablesAsync(DeferredReceivableKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.DeferredReceivables.AsNoTracking().AsQueryable();
        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<DeferredReceivableResponse>>(await query.OrderBy(x => x.DueDate).Select(x => MapDeferred(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<DeferredReceivableResponse>> SaveDeferredReceivableAsync(int? id, SaveDeferredReceivableRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.DebtorName))
            return Result.Failure<DeferredReceivableResponse>(AccountingErrors.InvalidRequest);
        var number = string.IsNullOrWhiteSpace(request.ReceivableNumber) ? await GenerateNumberAsync("DEF", dbcontext.DeferredReceivables, cancellationToken) : request.ReceivableNumber.Trim();
        if (await dbcontext.DeferredReceivables.AnyAsync(x => x.ReceivableNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<DeferredReceivableResponse>(AccountingErrors.DuplicateNumber);

        DeferredReceivable deferred;
        if (id.HasValue)
        {
            deferred = await dbcontext.DeferredReceivables.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (deferred is null)
                return Result.Failure<DeferredReceivableResponse>(AccountingErrors.DeferredReceivableNotFound);
        }
        else
        {
            deferred = new DeferredReceivable();
            dbcontext.DeferredReceivables.Add(deferred);
        }

        deferred.ReceivableNumber = number;
        deferred.Kind = request.Kind;
        deferred.DebtorName = request.DebtorName.Trim();
        deferred.Amount = request.Amount;
        deferred.DueDate = request.DueDate.Date;
        deferred.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDeferred(deferred));
    }

    public async Task<Result<DeferredReceivableResponse>> ReceiveDeferredReceivableAsync(int id, ReceiveDeferredReceivableRequest request, CancellationToken cancellationToken = default)
    {
        var deferred = await dbcontext.DeferredReceivables.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (deferred is null)
            return Result.Failure<DeferredReceivableResponse>(AccountingErrors.DeferredReceivableNotFound);
        if (request.ReceivedAmount < 0 || request.ReceivedAmount > deferred.Amount)
            return Result.Failure<DeferredReceivableResponse>(AccountingErrors.InvalidRequest);

        deferred.ReceivedAmount = request.ReceivedAmount;
        deferred.Status = request.ReceivedAmount >= deferred.Amount ? AccountingRecordStatus.Closed : AccountingRecordStatus.Posted;
        deferred.Notes = request.Notes?.Trim() ?? deferred.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDeferred(deferred));
    }

    public async Task<Result<DeferredReceivableSettlementResponse>> ReceiveDeferredReceivableWithReceiptAsync(int id, ReceiveDeferredReceivableWithReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var deferred = await dbcontext.DeferredReceivables.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (deferred is null)
            return Result.Failure<DeferredReceivableSettlementResponse>(AccountingErrors.DeferredReceivableNotFound);
        if (request.Amount <= 0 || request.Amount > deferred.Amount - deferred.ReceivedAmount)
            return Result.Failure<DeferredReceivableSettlementResponse>(AccountingErrors.InvalidRequest);
        if (request.ReceiptStatus is not (AccountingRecordStatus.Draft or AccountingRecordStatus.Posted or AccountingRecordStatus.Approved))
            return Result.Failure<DeferredReceivableSettlementResponse>(AccountingErrors.InvalidRequest);

        var receipt = new ReceiptVoucher
        {
            ReceiptNumber = await GenerateNumberAsync("RCV", dbcontext.ReceiptVouchers, cancellationToken),
            Kind = request.ReceiptKind,
            ReceiptDate = request.ReceiptDate ?? DateTime.UtcNow.AddHours(3),
            Amount = request.Amount,
            PayerName = deferred.DebtorName,
            ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? deferred.ReceivableNumber : request.ReferenceNumber.Trim(),
            Status = request.ReceiptStatus,
            Notes = request.Notes?.Trim()
        };

        deferred.ReceivedAmount += request.Amount;
        deferred.Status = deferred.ReceivedAmount >= deferred.Amount ? AccountingRecordStatus.Closed : AccountingRecordStatus.Posted;
        deferred.Notes = request.Notes?.Trim() ?? deferred.Notes;

        dbcontext.ReceiptVouchers.Add(receipt);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new DeferredReceivableSettlementResponse(MapDeferred(deferred), MapReceipt(receipt)));
    }

    public async Task<Result<IEnumerable<ExpenseVoucherResponse>>> GetExpensesAsync(ExpenseVoucherKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ExpenseVouchers.AsNoTracking().AsQueryable();
        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<ExpenseVoucherResponse>>(await query.OrderByDescending(x => x.ExpenseDate).Select(x => MapExpense(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<ExpenseVoucherResponse>> SaveExpenseAsync(int? id, SaveExpenseVoucherRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.PayeeName))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.InvalidRequest);
        var number = string.IsNullOrWhiteSpace(request.ExpenseNumber) ? await GenerateNumberAsync("EXP", dbcontext.ExpenseVouchers, cancellationToken) : request.ExpenseNumber.Trim();
        if (await dbcontext.ExpenseVouchers.AnyAsync(x => x.ExpenseNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.DuplicateNumber);

        ExpenseVoucher expense;
        if (id.HasValue)
        {
            expense = await dbcontext.ExpenseVouchers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (expense is null)
                return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.ExpenseNotFound);
        }
        else
        {
            expense = new ExpenseVoucher();
            dbcontext.ExpenseVouchers.Add(expense);
        }

        expense.ExpenseNumber = number;
        expense.Kind = request.Kind;
        expense.ExpenseDate = request.ExpenseDate ?? DateTime.UtcNow.AddHours(3);
        expense.Amount = request.Amount;
        expense.PayeeName = request.PayeeName.Trim();
        expense.Status = request.Status;
        expense.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapExpense(expense));
    }

    public async Task<Result<ExpenseVoucherResponse>> DecideExpenseAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default)
    {
        var expense = await dbcontext.ExpenseVouchers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (expense is null)
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.ExpenseNotFound);

        expense.Status = request.Status;
        expense.Notes = request.Notes?.Trim() ?? expense.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapExpense(expense));
    }

    public async Task<Result<LedgerEntryResponse>> CreateLedgerFromExpenseAsync(int id, CreateVoucherLedgerEntryRequest request, CancellationToken cancellationToken = default)
    {
        var expense = await dbcontext.ExpenseVouchers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (expense is null)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.ExpenseNotFound);
        if (expense.Status is not (AccountingRecordStatus.Approved or AccountingRecordStatus.Posted or AccountingRecordStatus.Closed) || request.Status is not (AccountingRecordStatus.Approved or AccountingRecordStatus.Posted))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);

        var ledger = await SaveLedgerEntryAsync(null, new SaveLedgerEntryRequest(
            $"JRN-{expense.ExpenseNumber}",
            expense.ExpenseDate,
            $"ترحيل سند صرف {expense.ExpenseNumber} - {expense.PayeeName}",
            request.Status,
            [
                new SaveLedgerLineRequest(request.DebitAccountId, request.FinanceCostCenterId, expense.Amount, 0, request.Notes ?? expense.Notes),
                new SaveLedgerLineRequest(request.CreditAccountId, request.FinanceCostCenterId, 0, expense.Amount, request.Notes ?? expense.Notes)
            ]), cancellationToken);

        if (!ledger.IsSuccess)
            return ledger;

        expense.Status = AccountingRecordStatus.Posted;
        expense.Notes = request.Notes?.Trim() ?? expense.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return ledger;
    }

    public async Task<Result<IEnumerable<SalaryDisbursementResponse>>> GetSalaryDisbursementsAsync(AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SalaryDisbursements.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<SalaryDisbursementResponse>>(await query.OrderByDescending(x => x.PayrollMonth).Select(x => MapSalary(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<SalaryDisbursementResponse>> SaveSalaryDisbursementAsync(int? id, SaveSalaryDisbursementRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TotalAmount <= 0 || string.IsNullOrWhiteSpace(request.ExportFormat))
            return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.InvalidRequest);

        SalaryDisbursement salary;
        if (id.HasValue)
        {
            salary = await dbcontext.SalaryDisbursements.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (salary is null)
                return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.ExpenseNotFound);
        }
        else
        {
            salary = new SalaryDisbursement();
            dbcontext.SalaryDisbursements.Add(salary);
        }

        salary.PayrollMonth = new DateTime(request.PayrollMonth.Year, request.PayrollMonth.Month, 1);
        salary.TotalAmount = request.TotalAmount;
        salary.ExportFormat = request.ExportFormat.Trim();
        salary.Status = request.Status;
        salary.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSalary(salary));
    }

    public async Task<Result<SalaryDisbursementResponse>> DecideSalaryDisbursementAsync(int id, DecideAccountingRecordRequest request, CancellationToken cancellationToken = default)
    {
        var salary = await dbcontext.SalaryDisbursements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (salary is null)
            return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.ExpenseNotFound);

        salary.Status = request.Status;
        salary.Notes = request.Notes?.Trim() ?? salary.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSalary(salary));
    }

    public async Task<Result<ExpenseVoucherResponse>> CreateExpenseFromSalaryDisbursementAsync(int id, CreateSalaryExpenseVoucherRequest request, CancellationToken cancellationToken = default)
    {
        var salary = await dbcontext.SalaryDisbursements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (salary is null)
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.SalaryDisbursementNotFound);
        if (salary.Status is not (AccountingRecordStatus.Approved or AccountingRecordStatus.Posted) || request.Status is not (AccountingRecordStatus.Draft or AccountingRecordStatus.Approved or AccountingRecordStatus.Posted))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.InvalidRequest);

        var expenseNumber = $"EXP-SAL-{salary.PayrollMonth:yyyyMM}";
        if (await dbcontext.ExpenseVouchers.AnyAsync(x => x.ExpenseNumber == expenseNumber, cancellationToken))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.DuplicateNumber);

        var expense = new ExpenseVoucher
        {
            ExpenseNumber = expenseNumber,
            Kind = ExpenseVoucherKind.Salary,
            ExpenseDate = request.ExpenseDate ?? salary.PayrollMonth,
            Amount = salary.TotalAmount,
            PayeeName = string.IsNullOrWhiteSpace(request.PayeeName) ? $"مسير رواتب {salary.PayrollMonth:yyyy-MM}" : request.PayeeName.Trim(),
            Status = request.Status,
            Notes = request.Notes?.Trim() ?? salary.Notes
        };

        salary.Status = AccountingRecordStatus.Posted;
        salary.Notes = request.Notes?.Trim() ?? salary.Notes;
        dbcontext.ExpenseVouchers.Add(expense);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapExpense(expense));
    }

    public async Task<Result<IEnumerable<FinancialReviewItemResponse>>> GetReviewItemsAsync(FinancialReviewKind? kind, AccountingRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinancialReviewItems.AsNoTracking().AsQueryable();
        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<FinancialReviewItemResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapReview(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FinancialReviewItemResponse>> SaveReviewItemAsync(int? id, SaveFinancialReviewItemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenceNumber) || request.Amount < 0)
            return Result.Failure<FinancialReviewItemResponse>(AccountingErrors.InvalidRequest);

        FinancialReviewItem item;
        if (id.HasValue)
        {
            item = await dbcontext.FinancialReviewItems.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (item is null)
                return Result.Failure<FinancialReviewItemResponse>(AccountingErrors.ReviewItemNotFound);
        }
        else
        {
            item = new FinancialReviewItem();
            dbcontext.FinancialReviewItems.Add(item);
        }

        item.Kind = request.Kind;
        item.ReferenceNumber = request.ReferenceNumber.Trim();
        item.Amount = request.Amount;
        item.Status = request.Status;
        item.DecisionNotes = request.DecisionNotes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReview(item));
    }

    public async Task<Result<FinancialReviewItemResponse>> DecideReviewItemAsync(int id, DecideFinancialReviewItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbcontext.FinancialReviewItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
            return Result.Failure<FinancialReviewItemResponse>(AccountingErrors.ReviewItemNotFound);
        item.Status = request.Status;
        item.DecisionNotes = request.DecisionNotes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReview(item));
    }

    public async Task<Result<IEnumerable<FinanceBudgetResponse>>> GetBudgetsAsync(int? fiscalYear, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinanceBudgets.AsNoTracking().AsQueryable();
        if (fiscalYear.HasValue)
            query = query.Where(x => x.FiscalYear == fiscalYear.Value);
        var budgets = await query.OrderByDescending(x => x.FiscalYear).ToListAsync(cancellationToken);
        var income = await dbcontext.ReceiptVouchers.Where(x => x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved).SumAsync(x => x.Amount, cancellationToken);
        var expenses = await dbcontext.ExpenseVouchers.Where(x => x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved || x.Status == AccountingRecordStatus.Closed).SumAsync(x => x.Amount, cancellationToken);
        return Result.Success<IEnumerable<FinanceBudgetResponse>>(budgets.Select(x => MapBudget(x, income, expenses)));
    }

    public async Task<Result<FinanceBudgetResponse>> SaveBudgetAsync(int? id, SaveFinanceBudgetRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FiscalYear < 2000 || string.IsNullOrWhiteSpace(request.Name) || request.PlannedIncome < 0 || request.PlannedExpenses < 0)
            return Result.Failure<FinanceBudgetResponse>(AccountingErrors.InvalidRequest);

        FinanceBudget budget;
        if (id.HasValue)
        {
            budget = await dbcontext.FinanceBudgets.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (budget is null)
                return Result.Failure<FinanceBudgetResponse>(AccountingErrors.BudgetNotFound);
        }
        else
        {
            budget = new FinanceBudget();
            dbcontext.FinanceBudgets.Add(budget);
        }

        budget.FiscalYear = request.FiscalYear;
        budget.Name = request.Name.Trim();
        budget.PlannedIncome = request.PlannedIncome;
        budget.PlannedExpenses = request.PlannedExpenses;
        budget.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapBudget(budget, 0, 0));
    }

    private async Task<string> GenerateNumberAsync<T>(string prefix, DbSet<T> set, CancellationToken cancellationToken) where T : class, IAuditable
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await set.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"{prefix}-{year}-{count:0000}";
    }

    private async Task LoadLedgerLinesAsync(LedgerEntry entry, CancellationToken cancellationToken)
    {
        await dbcontext.Entry(entry)
            .Collection(x => x.Lines)
            .Query()
            .Include(x => x.FinanceAccount)
            .Include(x => x.FinanceCostCenter)
            .LoadAsync(cancellationToken);
    }

    private static FinanceAccountResponse MapAccount(FinanceAccount account) => new(account.Id, account.Code, account.NameAr, account.AccountType.ToString(), account.IsActive);
    private static FinanceBankAccountResponse MapBankAccount(FinanceBankAccount account) => new(account.Id, account.BankName, account.AccountName, account.Iban, account.OpeningBalance, account.IsActive);
    private static FinanceCostCenterResponse MapCostCenter(FinanceCostCenter center) => new(center.Id, center.Code, center.NameAr, center.IsActive);
    private static LedgerEntryResponse MapLedgerEntry(LedgerEntry entry) =>
        new(
            entry.Id,
            entry.EntryNumber,
            entry.EntryDate,
            entry.Description,
            entry.Status.ToString(),
            entry.Lines.Sum(x => x.Debit),
            entry.Lines.Sum(x => x.Credit),
            entry.Lines.Select(MapLedgerLine).ToList());

    private static LedgerLineResponse MapLedgerLine(LedgerLine line) =>
        new(
            line.Id,
            line.FinanceAccountId,
            line.FinanceAccount?.Code ?? string.Empty,
            line.FinanceAccount?.NameAr ?? string.Empty,
            line.FinanceCostCenterId,
            line.FinanceCostCenter?.Code,
            line.FinanceCostCenter?.NameAr,
            line.Debit,
            line.Credit,
            line.Notes);
    private static ReceiptVoucherResponse MapReceipt(ReceiptVoucher receipt) => new(receipt.Id, receipt.ReceiptNumber, receipt.Kind.ToString(), receipt.ReceiptDate, receipt.Amount, receipt.PayerName, receipt.ReferenceNumber, receipt.Status.ToString(), receipt.Notes);
    private static DeferredReceivableResponse MapDeferred(DeferredReceivable deferred) => new(deferred.Id, deferred.ReceivableNumber, deferred.Kind.ToString(), deferred.DebtorName, deferred.Amount, deferred.ReceivedAmount, deferred.Amount - deferred.ReceivedAmount, deferred.DueDate, deferred.Status.ToString(), deferred.Notes);
    private static ExpenseVoucherResponse MapExpense(ExpenseVoucher expense) => new(expense.Id, expense.ExpenseNumber, expense.Kind.ToString(), expense.ExpenseDate, expense.Amount, expense.PayeeName, expense.Status.ToString(), expense.Notes);
    private static SalaryDisbursementResponse MapSalary(SalaryDisbursement salary) => new(salary.Id, salary.PayrollMonth, salary.TotalAmount, salary.ExportFormat, salary.Status.ToString(), salary.Notes);
    private static FinancialReviewItemResponse MapReview(FinancialReviewItem item) => new(item.Id, item.Kind.ToString(), item.ReferenceNumber, item.Amount, item.Status.ToString(), item.DecisionNotes);
    private static FinanceBudgetResponse MapBudget(FinanceBudget budget, decimal actualIncome, decimal actualExpenses) => new(budget.Id, budget.FiscalYear, budget.Name, budget.PlannedIncome, budget.PlannedExpenses, actualIncome, actualExpenses, (budget.PlannedIncome - actualIncome) - (budget.PlannedExpenses - actualExpenses), budget.Status.ToString());
}
