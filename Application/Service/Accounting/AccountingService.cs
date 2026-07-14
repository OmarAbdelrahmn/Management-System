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
    public async Task<Result<IEnumerable<FiscalPeriodResponse>>> GetFiscalPeriodsAsync(CancellationToken cancellationToken = default)
    {
        var periods = await dbcontext.AccountingSettings.AsNoTracking().OrderByDescending(x => x.StartsAt)
            .Select(x => new FiscalPeriodResponse(x.Id, x.FiscalYearName, x.StartsAt, x.EndsAt, x.IsClosed)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<FiscalPeriodResponse>>(periods);
    }

    public async Task<Result<FiscalPeriodResponse>> SaveFiscalPeriodAsync(int? id, SaveFiscalPeriodRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FiscalYearName) || request.StartsAt.Date > request.EndsAt.Date)
            return Result.Failure<FiscalPeriodResponse>(AccountingErrors.InvalidRequest);
        var overlaps = await dbcontext.AccountingSettings.AnyAsync(x => x.StartsAt.Date <= request.EndsAt.Date && request.StartsAt.Date <= x.EndsAt.Date && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (overlaps) return Result.Failure<FiscalPeriodResponse>(AccountingErrors.InvalidRequest);
        AccountingSetting period;
        if (id.HasValue)
        {
            period = await dbcontext.AccountingSettings.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (period is null) return Result.Failure<FiscalPeriodResponse>(AccountingErrors.InvalidRequest);
            if (period.IsClosed && !request.IsClosed)
                return Result.Failure<FiscalPeriodResponse>(AccountingErrors.FiscalPeriodLocked);
        }
        else { period = new AccountingSetting(); dbcontext.AccountingSettings.Add(period); }

        if (request.IsClosed && await HasDraftRecordsInPeriodAsync(request.StartsAt.Date, request.EndsAt.Date, cancellationToken))
            return Result.Failure<FiscalPeriodResponse>(AccountingErrors.InvalidRequest);
        period.FiscalYearName = request.FiscalYearName.Trim();
        period.StartsAt = request.StartsAt.Date;
        period.EndsAt = request.EndsAt.Date;
        period.IsClosed = request.IsClosed;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new FiscalPeriodResponse(period.Id, period.FiscalYearName, period.StartsAt, period.EndsAt, period.IsClosed));
    }

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
        if (await IsDateLockedAsync(request.EntryDate ?? DateTime.UtcNow.AddHours(3), cancellationToken))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.FiscalPeriodLocked);

        var entryNumber = string.IsNullOrWhiteSpace(request.EntryNumber) ? await GenerateNumberAsync("JRN", dbcontext.LedgerEntries, cancellationToken) : request.EntryNumber.Trim();
        if (await dbcontext.LedgerEntries.AnyAsync(x => x.EntryNumber == entryNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.DuplicateNumber);

        LedgerEntry entry;
        if (id.HasValue)
        {
            entry = await dbcontext.LedgerEntries.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (entry is null)
                return Result.Failure<LedgerEntryResponse>(AccountingErrors.LedgerEntryNotFound);
            if (IsImmutable(entry.Status)) return Result.Failure<LedgerEntryResponse>(AccountingErrors.PostedRecordImmutable);
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

    public async Task<Result<LedgerEntryResponse>> SetLedgerPostingAsync(int id, SetLedgerPostingRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await dbcontext.LedgerEntries.Include(x => x.Lines).ThenInclude(x => x.FinanceAccount).Include(x => x.Lines).ThenInclude(x => x.FinanceCostCenter)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entry is null) return Result.Failure<LedgerEntryResponse>(AccountingErrors.LedgerEntryNotFound);
        if (await IsDateLockedAsync(entry.EntryDate, cancellationToken)) return Result.Failure<LedgerEntryResponse>(AccountingErrors.FiscalPeriodLocked);
        if (!request.IsPosted || entry.Status != AccountingRecordStatus.Draft)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.PostedRecordImmutable);
        if (entry.Lines.Count < 2 || entry.Lines.Sum(x => x.Debit) <= 0 || entry.Lines.Sum(x => x.Debit) != entry.Lines.Sum(x => x.Credit))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);

        entry.Status = AccountingRecordStatus.Posted;
        if (!string.IsNullOrWhiteSpace(request.Notes)) entry.Description = $"{entry.Description} — {request.Notes.Trim()}";
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLedgerEntry(entry));
    }

    public async Task<Result<LedgerEntryResponse>> CreateOpeningBalanceAsync(CreateOpeningBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var period = await dbcontext.AccountingSettings.FirstOrDefaultAsync(x => x.Id == request.FiscalPeriodId, cancellationToken);
        if (period is null || period.IsClosed || request.Lines.Count < 2)
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.InvalidRequest);

        var entryNumber = $"OPEN-{period.Id}";
        if (await dbcontext.LedgerEntries.AnyAsync(x => x.EntryNumber == entryNumber, cancellationToken))
            return Result.Failure<LedgerEntryResponse>(AccountingErrors.DuplicateNumber);

        return await SaveLedgerEntryAsync(null, new SaveLedgerEntryRequest(
            entryNumber,
            period.StartsAt,
            $"الرصيد الافتتاحي للفترة {period.FiscalYearName}" + (string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : $" — {request.Notes.Trim()}"),
            AccountingRecordStatus.Posted,
            request.Lines), cancellationToken);
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
        if (await IsDateLockedAsync(request.ReceiptDate ?? DateTime.UtcNow.AddHours(3), cancellationToken))
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.FiscalPeriodLocked);
        var receiptNumber = string.IsNullOrWhiteSpace(request.ReceiptNumber) ? await GenerateNumberAsync("RCV", dbcontext.ReceiptVouchers, cancellationToken) : request.ReceiptNumber.Trim();
        if (await dbcontext.ReceiptVouchers.AnyAsync(x => x.ReceiptNumber == receiptNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.DuplicateNumber);

        ReceiptVoucher receipt;
        if (id.HasValue)
        {
            receipt = await dbcontext.ReceiptVouchers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (receipt is null)
                return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.ReceiptNotFound);
            if (IsImmutable(receipt.Status)) return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.PostedRecordImmutable);
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
        if (await IsDateLockedAsync(receipt.ReceiptDate, cancellationToken)) return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.FiscalPeriodLocked);
        if (!IsValidRecordTransition(receipt.Status, request.Status) || RequiresDecisionReason(request.Status) && string.IsNullOrWhiteSpace(request.Notes))
            return Result.Failure<ReceiptVoucherResponse>(AccountingErrors.InvalidRequest);

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
        if (await IsDateLockedAsync(request.ExpenseDate ?? DateTime.UtcNow.AddHours(3), cancellationToken)) return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.FiscalPeriodLocked);
        var number = string.IsNullOrWhiteSpace(request.ExpenseNumber) ? await GenerateNumberAsync("EXP", dbcontext.ExpenseVouchers, cancellationToken) : request.ExpenseNumber.Trim();
        if (await dbcontext.ExpenseVouchers.AnyAsync(x => x.ExpenseNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.DuplicateNumber);

        ExpenseVoucher expense;
        if (id.HasValue)
        {
            expense = await dbcontext.ExpenseVouchers.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (expense is null)
                return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.ExpenseNotFound);
            if (IsImmutable(expense.Status)) return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.PostedRecordImmutable);
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
        if (await IsDateLockedAsync(expense.ExpenseDate, cancellationToken)) return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.FiscalPeriodLocked);
        if (!IsValidRecordTransition(expense.Status, request.Status) || RequiresDecisionReason(request.Status) && string.IsNullOrWhiteSpace(request.Notes))
            return Result.Failure<ExpenseVoucherResponse>(AccountingErrors.InvalidRequest);

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
        if (await IsDateLockedAsync(request.PayrollMonth, cancellationToken)) return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.FiscalPeriodLocked);

        SalaryDisbursement salary;
        if (id.HasValue)
        {
            salary = await dbcontext.SalaryDisbursements.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (salary is null)
                return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.ExpenseNotFound);
            if (IsImmutable(salary.Status)) return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.PostedRecordImmutable);
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
            return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.SalaryDisbursementNotFound);
        if (!IsValidRecordTransition(salary.Status, request.Status) || RequiresDecisionReason(request.Status) && string.IsNullOrWhiteSpace(request.Notes))
            return Result.Failure<SalaryDisbursementResponse>(AccountingErrors.InvalidRequest);

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
        if (!IsValidRecordTransition(item.Status, request.Status) || RequiresDecisionReason(request.Status) && string.IsNullOrWhiteSpace(request.DecisionNotes))
            return Result.Failure<FinancialReviewItemResponse>(AccountingErrors.InvalidRequest);
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
        if (request.FiscalYear < 2000 || string.IsNullOrWhiteSpace(request.Name) || request.PlannedIncome < 0 || request.PlannedExpenses < 0 || request.Status != BudgetStatus.Draft)
            return Result.Failure<FinanceBudgetResponse>(AccountingErrors.InvalidRequest);

        FinanceBudget budget;
        if (id.HasValue)
        {
            budget = await dbcontext.FinanceBudgets.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (budget is null)
                return Result.Failure<FinanceBudgetResponse>(AccountingErrors.BudgetNotFound);
            if (budget.Status != BudgetStatus.Draft)
                return Result.Failure<FinanceBudgetResponse>(AccountingErrors.PostedRecordImmutable);
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
        var actualIncome = await dbcontext.ReceiptVouchers.Where(x => x.ReceiptDate.Year == budget.FiscalYear && (x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved)).SumAsync(x => x.Amount, cancellationToken);
        var actualExpenses = await dbcontext.ExpenseVouchers.Where(x => x.ExpenseDate.Year == budget.FiscalYear && (x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved || x.Status == AccountingRecordStatus.Closed)).SumAsync(x => x.Amount, cancellationToken);
        return Result.Success(MapBudget(budget, actualIncome, actualExpenses));
    }

    public async Task<Result<FinanceBudgetResponse>> DecideBudgetAsync(int id, DecideBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var budget = await dbcontext.FinanceBudgets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (budget is null)
            return Result.Failure<FinanceBudgetResponse>(AccountingErrors.BudgetNotFound);
        if (budget.Status != BudgetStatus.Draft || request.Status is BudgetStatus.Draft || budget.Status == request.Status)
            return Result.Failure<FinanceBudgetResponse>(AccountingErrors.InvalidRequest);

        budget.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        var actualIncome = await dbcontext.ReceiptVouchers.Where(x => x.ReceiptDate.Year == budget.FiscalYear && (x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved)).SumAsync(x => x.Amount, cancellationToken);
        var actualExpenses = await dbcontext.ExpenseVouchers.Where(x => x.ExpenseDate.Year == budget.FiscalYear && (x.Status == AccountingRecordStatus.Posted || x.Status == AccountingRecordStatus.Approved || x.Status == AccountingRecordStatus.Closed)).SumAsync(x => x.Amount, cancellationToken);
        return Result.Success(MapBudget(budget, actualIncome, actualExpenses));
    }

    public async Task<Result<IEnumerable<BankReconciliationResponse>>> GetBankReconciliationsAsync(int? bankAccountId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BankReconciliations.AsNoTracking().Include(x => x.FinanceBankAccount).AsQueryable();
        if (bankAccountId.HasValue) query = query.Where(x => x.FinanceBankAccountId == bankAccountId.Value);
        var items = await query.OrderByDescending(x => x.ReconciliationDate).Select(x => MapReconciliation(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<BankReconciliationResponse>>(items);
    }

    public async Task<Result<BankReconciliationResponse>> SaveBankReconciliationAsync(int? id, SaveBankReconciliationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IsApproved) return Result.Failure<BankReconciliationResponse>(AccountingErrors.InvalidRequest);
        if (request.FinanceBankAccountId <= 0 || !await dbcontext.FinanceBankAccounts.AnyAsync(x => x.Id == request.FinanceBankAccountId, cancellationToken)) return Result.Failure<BankReconciliationResponse>(AccountingErrors.BankAccountNotFound);
        BankReconciliation item;
        if (id.HasValue) item = await dbcontext.BankReconciliations.Include(x => x.FinanceBankAccount).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
        else { item = new BankReconciliation(); dbcontext.BankReconciliations.Add(item); }
        if (item is null) return Result.Failure<BankReconciliationResponse>(AccountingErrors.InvalidRequest);
        if (item.IsApproved) return Result.Failure<BankReconciliationResponse>(AccountingErrors.PostedRecordImmutable);
        item.FinanceBankAccountId = request.FinanceBankAccountId;
        item.ReconciliationDate = request.ReconciliationDate.Date;
        item.StatementBalance = request.StatementBalance;
        item.BookBalance = request.BookBalance;
        item.IsApproved = request.IsApproved;
        item.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(item).Reference(x => x.FinanceBankAccount).LoadAsync(cancellationToken);
        return Result.Success(MapReconciliation(item));
    }

    public async Task<Result<BankReconciliationResponse>> ApproveBankReconciliationAsync(int id, ApproveBankReconciliationRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbcontext.BankReconciliations.Include(x => x.FinanceBankAccount).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return Result.Failure<BankReconciliationResponse>(AccountingErrors.InvalidRequest);
        if (item.IsApproved) return Result.Failure<BankReconciliationResponse>(AccountingErrors.PostedRecordImmutable);
        item.IsApproved = true;
        item.Notes = string.IsNullOrWhiteSpace(request.Notes) ? item.Notes : request.Notes.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReconciliation(item));
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

    private Task<bool> IsDateLockedAsync(DateTime date, CancellationToken cancellationToken) =>
        dbcontext.AccountingSettings.AnyAsync(x => x.IsClosed && x.StartsAt <= date && x.EndsAt >= date, cancellationToken);

    private async Task<bool> HasDraftRecordsInPeriodAsync(DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken) =>
        await dbcontext.LedgerEntries.AnyAsync(x => x.Status == AccountingRecordStatus.Draft && x.EntryDate >= startsAt && x.EntryDate <= endsAt, cancellationToken) ||
        await dbcontext.ReceiptVouchers.AnyAsync(x => x.Status == AccountingRecordStatus.Draft && x.ReceiptDate >= startsAt && x.ReceiptDate <= endsAt, cancellationToken) ||
        await dbcontext.ExpenseVouchers.AnyAsync(x => x.Status == AccountingRecordStatus.Draft && x.ExpenseDate >= startsAt && x.ExpenseDate <= endsAt, cancellationToken);

    private static bool IsImmutable(AccountingRecordStatus status) => status is AccountingRecordStatus.Posted or AccountingRecordStatus.Approved or AccountingRecordStatus.Closed or AccountingRecordStatus.Canceled or AccountingRecordStatus.Archived;

    private static bool RequiresDecisionReason(AccountingRecordStatus status) => status is AccountingRecordStatus.Rejected or AccountingRecordStatus.Canceled;

    private static bool IsValidRecordTransition(AccountingRecordStatus from, AccountingRecordStatus to) =>
        from switch
        {
            AccountingRecordStatus.Draft => to is AccountingRecordStatus.Approved or AccountingRecordStatus.Posted or AccountingRecordStatus.Rejected or AccountingRecordStatus.Canceled,
            AccountingRecordStatus.Approved => to is AccountingRecordStatus.Posted or AccountingRecordStatus.Canceled,
            AccountingRecordStatus.Rejected => to is AccountingRecordStatus.Draft or AccountingRecordStatus.Canceled,
            AccountingRecordStatus.Posted => to == AccountingRecordStatus.Closed,
            _ => false
        };

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
    private static BankReconciliationResponse MapReconciliation(BankReconciliation item) => new(item.Id, item.FinanceBankAccountId, item.FinanceBankAccount?.BankName ?? string.Empty, item.FinanceBankAccount?.AccountName ?? string.Empty, item.ReconciliationDate, item.StatementBalance, item.BookBalance, item.StatementBalance - item.BookBalance, item.IsApproved, item.Notes);
}
