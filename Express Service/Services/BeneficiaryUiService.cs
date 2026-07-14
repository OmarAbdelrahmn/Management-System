using Application.Contracts.Beneficiaries;
using Application.Service.Beneficiaries;
using Domain.Entities;

namespace Express_Service.Services;

public class BeneficiaryUiService(IBeneficiaryService beneficiaries)
{
    public async Task<BeneficiaryDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<BeneficiaryResponse>> SearchAsync(string? search = null, BeneficiaryStatus? status = null, string? category = null, string? city = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.SearchAsync(new BeneficiarySearchRequest(search, status, category, city), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<BeneficiaryResponse?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<(bool Success, string Message, BeneficiaryResponse? Beneficiary)> CreateAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة ملف المستفيد.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message, BeneficiaryResponse? Beneficiary)> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث ملف المستفيد.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message)> ArchiveAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.ArchiveAsync(id, new ArchiveBeneficiaryRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم أرشفة ملف المستفيد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.RestoreAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تمت استعادة ملف المستفيد.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryDependentResponse>> GetDependentsAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetDependentsAsync(beneficiaryProfileId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddDependentAsync(AddBeneficiaryDependentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.AddDependentAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة التابع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateDependentAsync(int id, UpdateBeneficiaryDependentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateDependentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث التابع.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryGuardianResponse>> GetGuardiansAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetGuardiansAsync(beneficiaryProfileId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddGuardianAsync(AddBeneficiaryGuardianRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.AddGuardianAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة الوصي.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateGuardianAsync(int id, UpdateBeneficiaryGuardianRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateGuardianAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث الوصي.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryUpdateRequestResponse>> GetUpdateRequestsAsync(int? beneficiaryProfileId = null, BeneficiaryUpdateRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetUpdateRequestsAsync(beneficiaryProfileId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateUpdateRequestAsync(CreateBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CreateUpdateRequestAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء طلب تحديث البيانات.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideUpdateRequestAsync(int id, bool approved, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.DecideUpdateRequestAsync(id, new DecideBeneficiaryUpdateRequest(approved, notes), cancellationToken);
        return result.IsSuccess ? (true, approved ? "تم اعتماد طلب التحديث." : "تم رفض طلب التحديث.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelUpdateRequestAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CancelUpdateRequestAsync(id, new CancelBeneficiaryUpdateRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء طلب التحديث.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryEntityResponse>> GetEntitiesAsync(string? search = null, BeneficiaryEntityStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetEntitiesAsync(search, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message, BeneficiaryEntityResponse? Entity)> SaveEntityAsync(int? id, UpsertBeneficiaryEntityRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.SaveEntityAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الجهة المستفيدة.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<List<BeneficiaryAccountArtifactResponse>> GetAccountArtifactsAsync(BeneficiaryAccountArtifactType? type = null, BeneficiaryAccountArtifactStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetAccountArtifactsAsync(type, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateAccountArtifactAsync(CreateBeneficiaryAccountArtifactRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CreateAccountArtifactAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء سجل البطاقة أو الباركود.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateAccountArtifactAsync(int id, UpdateBeneficiaryAccountArtifactRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateAccountArtifactAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث سجل البطاقة أو الباركود.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateAccountArtifactStatusAsync(int id, BeneficiaryAccountArtifactStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateAccountArtifactStatusAsync(id, new UpdateBeneficiaryAccountArtifactStatusRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة السجل.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryGuardianOperationResponse>> GetGuardianOperationsAsync(BeneficiaryGuardianOperationType? type = null, BeneficiaryOperationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetGuardianOperationsAsync(type, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateGuardianOperationAsync(CreateBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CreateGuardianOperationAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء عملية الوصي.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideGuardianOperationAsync(int id, bool approved, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.DecideGuardianOperationAsync(id, new DecideBeneficiaryGuardianOperationRequest(approved, notes), cancellationToken);
        return result.IsSuccess ? (true, approved ? "تم اعتماد عملية الوصي." : "تم رفض عملية الوصي.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelGuardianOperationAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CancelGuardianOperationAsync(id, new CancelBeneficiaryGuardianOperationRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء عملية الوصي.") : (false, result.Error.Description);
    }

    public async Task<List<BeneficiaryUpdateBatchResponse>> GetUpdateBatchesAsync(BeneficiaryUpdateBatchKind? kind = null, BeneficiaryOperationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.GetUpdateBatchesAsync(kind, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateUpdateBatchAsync(CreateBeneficiaryUpdateBatchRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.CreateUpdateBatchAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء مهمة تحديث البيانات.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateUpdateBatchAsync(int id, UpdateBeneficiaryUpdateBatchRequest request, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateUpdateBatchAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث مهمة البيانات.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateBatchProgressAsync(int id, BeneficiaryOperationStatus status, int completedProfiles, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await beneficiaries.UpdateBatchProgressAsync(id, new UpdateBeneficiaryBatchProgressRequest(status, completedProfiles, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث تقدم مهمة البيانات.") : (false, result.Error.Description);
    }
}
