using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ElectronicOffice;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.ElectronicOffice;

public class ElectronicOfficeService(ApplicationDbcontext dbcontext) : IElectronicOfficeService
{
    public async Task<Result<ElectronicOfficeDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        Result.Success(new ElectronicOfficeDashboardResponse(
            await dbcontext.OfficeAttendanceEntries.CountAsync(cancellationToken),
            await dbcontext.OfficeReminders.CountAsync(x => x.Status == OfficeRecordStatus.Pending, cancellationToken),
            await dbcontext.OfficeAdministrativeRequests.CountAsync(x => x.Status == OfficeRecordStatus.Pending, cancellationToken),
            await dbcontext.OfficeTransactions.CountAsync(x => x.Status == OfficeTransactionStatus.Pending || x.Status == OfficeTransactionStatus.RequiredFollowUp, cancellationToken),
            await dbcontext.OfficeLogRecords.CountAsync(cancellationToken)));

    public async Task<Result<IEnumerable<OfficeAttendanceResponse>>> GetAttendanceAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OfficeAttendanceEntries.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<OfficeAttendanceResponse>>(await query.OrderByDescending(x => x.AttendanceAt).Select(x => MapAttendance(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OfficeAttendanceResponse>> SaveAttendanceAsync(SaveOfficeAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeName))
            return Result.Failure<OfficeAttendanceResponse>(ElectronicOfficeErrors.InvalidRequest);
        var entity = new OfficeAttendanceEntry { EmployeeName = request.EmployeeName.Trim(), AttendanceType = request.AttendanceType, AttendanceAt = request.AttendanceAt, Status = request.Status, Notes = TrimOrNull(request.Notes) };
        dbcontext.OfficeAttendanceEntries.Add(entity);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAttendance(entity));
    }

    public async Task<Result<IEnumerable<OfficeReminderResponse>>> GetRemindersAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OfficeReminders.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<OfficeReminderResponse>>(await query.OrderBy(x => x.DueAt).Select(x => MapReminder(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OfficeReminderResponse>> SaveReminderAsync(int? id, SaveOfficeReminderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<OfficeReminderResponse>(ElectronicOfficeErrors.InvalidRequest);
        var entity = id.HasValue ? await dbcontext.OfficeReminders.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new OfficeReminder();
        if (entity is null) return Result.Failure<OfficeReminderResponse>(ElectronicOfficeErrors.ReminderNotFound);
        if (!id.HasValue) dbcontext.OfficeReminders.Add(entity);
        entity.OwnerName = request.OwnerName.Trim();
        entity.Title = request.Title.Trim();
        entity.DueAt = request.DueAt;
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReminder(entity));
    }

    public async Task<Result<IEnumerable<OfficeAdministrativeRequestResponse>>> GetRequestsAsync(OfficeRequestType? type = null, OfficeRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OfficeAdministrativeRequests.AsNoTracking().AsQueryable();
        if (type.HasValue) query = query.Where(x => x.RequestType == type.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<OfficeAdministrativeRequestResponse>>(await query.OrderBy(x => x.Status).ThenByDescending(x => x.RequestedAt).Select(x => MapRequest(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OfficeAdministrativeRequestResponse>> SaveRequestAsync(int? id, SaveOfficeAdministrativeRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestNumber) || string.IsNullOrWhiteSpace(request.RequestedBy) || string.IsNullOrWhiteSpace(request.Subject))
            return Result.Failure<OfficeAdministrativeRequestResponse>(ElectronicOfficeErrors.InvalidRequest);
        var number = request.RequestNumber.Trim();
        if (await dbcontext.OfficeAdministrativeRequests.AnyAsync(x => x.RequestNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<OfficeAdministrativeRequestResponse>(ElectronicOfficeErrors.DuplicateRequestNumber);
        var entity = id.HasValue ? await dbcontext.OfficeAdministrativeRequests.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new OfficeAdministrativeRequest();
        if (entity is null) return Result.Failure<OfficeAdministrativeRequestResponse>(ElectronicOfficeErrors.RequestNotFound);
        if (!id.HasValue) dbcontext.OfficeAdministrativeRequests.Add(entity);
        entity.RequestNumber = number;
        entity.RequestType = request.RequestType;
        entity.RequestedBy = request.RequestedBy.Trim();
        entity.Subject = request.Subject.Trim();
        entity.Status = request.Status;
        entity.DecisionNotes = TrimOrNull(request.DecisionNotes);
        entity.RequestedAt = request.RequestedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRequest(entity));
    }

    public async Task<Result> DecideRequestAsync(int id, DecideOfficeRequestRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.OfficeAdministrativeRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure(ElectronicOfficeErrors.RequestNotFound);
        entity.Status = request.Status;
        entity.DecisionNotes = TrimOrNull(request.DecisionNotes) ?? entity.DecisionNotes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<OfficeTransactionResponse>>> GetTransactionsAsync(OfficeTransactionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OfficeTransactions.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<OfficeTransactionResponse>>(await query.OrderBy(x => x.Status).ThenByDescending(x => x.RequestedAt).Select(x => MapTransaction(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OfficeTransactionResponse>> SaveTransactionAsync(int? id, SaveOfficeTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionNumber) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.RequestedBy))
            return Result.Failure<OfficeTransactionResponse>(ElectronicOfficeErrors.InvalidRequest);
        var number = request.TransactionNumber.Trim();
        if (await dbcontext.OfficeTransactions.AnyAsync(x => x.TransactionNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<OfficeTransactionResponse>(ElectronicOfficeErrors.DuplicateTransactionNumber);
        var entity = id.HasValue ? await dbcontext.OfficeTransactions.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new OfficeTransaction();
        if (entity is null) return Result.Failure<OfficeTransactionResponse>(ElectronicOfficeErrors.TransactionNotFound);
        if (!id.HasValue) dbcontext.OfficeTransactions.Add(entity);
        entity.TransactionNumber = number;
        entity.Subject = request.Subject.Trim();
        entity.RequestedBy = request.RequestedBy.Trim();
        entity.CurrentStep = TrimOrNull(request.CurrentStep);
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);
        entity.RequestedAt = request.RequestedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapTransaction(entity));
    }

    public async Task<Result> UpdateTransactionStatusAsync(int id, UpdateOfficeTransactionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.OfficeTransactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure(ElectronicOfficeErrors.TransactionNotFound);
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes) ?? entity.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<OfficeLogRecordResponse>>> GetLogRecordsAsync(OfficeLogType? type = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.OfficeLogRecords.AsNoTracking().AsQueryable();
        if (type.HasValue) query = query.Where(x => x.LogType == type.Value);
        return Result.Success<IEnumerable<OfficeLogRecordResponse>>(await query.OrderByDescending(x => x.RecordDate).Select(x => MapLog(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<OfficeLogRecordResponse>> SaveLogRecordAsync(int? id, SaveOfficeLogRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<OfficeLogRecordResponse>(ElectronicOfficeErrors.InvalidRequest);
        var entity = id.HasValue ? await dbcontext.OfficeLogRecords.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new OfficeLogRecord();
        if (entity is null) return Result.Failure<OfficeLogRecordResponse>(ElectronicOfficeErrors.LogRecordNotFound);
        if (!id.HasValue) dbcontext.OfficeLogRecords.Add(entity);
        entity.LogType = request.LogType;
        entity.Title = request.Title.Trim();
        entity.Reference = TrimOrNull(request.Reference);
        entity.Notes = TrimOrNull(request.Notes);
        entity.RecordDate = request.RecordDate;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapLog(entity));
    }

    private static OfficeAttendanceResponse MapAttendance(OfficeAttendanceEntry x) => new(x.Id, x.EmployeeName, x.AttendanceType.ToString(), x.AttendanceAt, x.Status.ToString(), x.Notes);
    private static OfficeReminderResponse MapReminder(OfficeReminder x) => new(x.Id, x.OwnerName, x.Title, x.DueAt, x.Status.ToString(), x.Notes);
    private static OfficeAdministrativeRequestResponse MapRequest(OfficeAdministrativeRequest x) => new(x.Id, x.RequestNumber, x.RequestType.ToString(), x.RequestedBy, x.Subject, x.Status.ToString(), x.DecisionNotes, x.RequestedAt);
    private static OfficeTransactionResponse MapTransaction(OfficeTransaction x) => new(x.Id, x.TransactionNumber, x.Subject, x.RequestedBy, x.CurrentStep, x.Status.ToString(), x.Notes, x.RequestedAt);
    private static OfficeLogRecordResponse MapLog(OfficeLogRecord x) => new(x.Id, x.LogType.ToString(), x.Title, x.Reference, x.Notes, x.RecordDate);
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
