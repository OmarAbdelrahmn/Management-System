using Application.Contracts.DocumentationArchive;
using Application.Service.DocumentationArchive;
using Domain.Entities;

namespace Express_Service.Services;

public class DocumentationArchiveUiService(IDocumentationArchiveService service)
{
    public async Task<DocumentationArchiveDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<ArchiveDocumentResponse>> GetArchiveDocumentsAsync(ArchiveDocumentCategory? category = null, ArchiveDocumentStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetArchiveDocumentsAsync(category, status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveArchiveDocumentAsync(int? id, SaveArchiveDocumentRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveArchiveDocumentAsync(id, request, cancellationToken), "تم حفظ ملف الأرشيف.");

    public async Task<List<CorrespondenceRecordResponse>> GetCorrespondenceAsync(CorrespondenceDirection? direction = null, CorrespondenceStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetCorrespondenceAsync(direction, status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveCorrespondenceAsync(int? id, SaveCorrespondenceRecordRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveCorrespondenceAsync(id, request, cancellationToken), "تم حفظ سجل البريد.");

    public async Task<(bool Success, string Message)> UpdateCorrespondenceStatusAsync(int id, CorrespondenceStatus status, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.UpdateCorrespondenceStatusAsync(id, new UpdateCorrespondenceStatusRequest(status, notes), cancellationToken), "تم تحديث حالة البريد.");

    public async Task<(bool Success, string Message)> RequestCorrespondenceRemovalAsync(int id, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.RequestCorrespondenceRemovalAsync(id, new RequestCorrespondenceRemovalRequest(notes), cancellationToken), "تم إرسال طلب حذف البريد للاعتماد.");

    public async Task<(bool Success, string Message)> DecideCorrespondenceRemovalAsync(int id, bool approved, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.DecideCorrespondenceRemovalAsync(id, new DecideCorrespondenceRemovalRequest(approved, notes), cancellationToken), approved ? "تم اعتماد حذف البريد." : "تم رفض طلب حذف البريد.");

    public async Task<List<CorrespondenceOperationResponse>> GetOperationsAsync(CorrespondenceOperationStatus? status = null, int? correspondenceRecordId = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetOperationsAsync(status, correspondenceRecordId, cancellationToken));

    public async Task<(bool Success, string Message)> SaveOperationAsync(int? id, SaveCorrespondenceOperationRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveOperationAsync(id, request, cancellationToken), "تم حفظ معاملة البريد.");

    public async Task<(bool Success, string Message)> CompleteOperationAsync(int id, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.CompleteOperationAsync(id, new CompleteCorrespondenceOperationRequest(DateTime.UtcNow.AddHours(3), notes), cancellationToken), "تم إكمال معاملة البريد.");

    private static (bool Success, string Message) ToUi(Application.Abstraction.Result result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
