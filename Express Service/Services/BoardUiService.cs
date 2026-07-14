using Application.Contracts.Boards;
using Application.Service.Boards;
using Application.Contracts.TaskManagement;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Express_Service.Services;

public class BoardUiService(IBoardService boardService, ApplicationDbcontext dbcontext)
{
    public async Task<List<BoardResponse>> GetBoardsAsync(CancellationToken cancellationToken = default)
    {
        var result = await boardService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<UserPickerResponse>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        await dbcontext.Users.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.FullName)
            .Select(x => new UserPickerResponse(x.Id, x.FullName, x.Email)).ToListAsync(cancellationToken);

    public async Task<(bool Success, string Message)> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var result = await boardService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء المجلس ودورته.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var result = await boardService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث بيانات المجلس.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SaveMembersAsync(int id, IEnumerable<CreateBoardMemberRequest> members, CancellationToken cancellationToken = default)
    {
        var result = await boardService.SaveMembersAsync(id, new SaveBoardMembersRequest(members), cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ عضويات المجلس والأدوار والرسوم والأوزان.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RenewCycleAsync(int id, RenewBoardCycleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await boardService.RenewCycleAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تجديد دورة المجلس.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CloseExpiredAsync(CancellationToken cancellationToken = default)
    {
        var result = await boardService.CloseExpiredBoardsAsync(DateTime.UtcNow.AddHours(3), cancellationToken);
        return result.IsSuccess ? (true, $"تم إغلاق {result.Value} مجلس منتهي.") : (false, result.Error.Description);
    }
}
