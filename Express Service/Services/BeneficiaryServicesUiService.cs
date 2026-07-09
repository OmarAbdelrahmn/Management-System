using Application.Contracts.BeneficiaryServices;
using Application.Service.BeneficiaryServices;
using Domain.Entities;

namespace Express_Service.Services;

public class BeneficiaryServicesUiService(IBeneficiaryServicesService service)
{
    public async Task<BeneficiaryServicesDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<AidRequestResponse>> GetAidRequestsAsync(AidRequestStatus? status = null, bool? isExternal = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAidRequestsAsync(status, isExternal, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveAidRequestAsync(int? id, SaveAidRequestRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveAidRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ طلب الإعانة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideAidRequestAsync(int id, AidRequestStatus status, string? socialNotes, string? decisionNotes, CancellationToken cancellationToken = default)
    {
        var result = await service.DecideAidRequestAsync(id, new DecideAidRequestRequest(status, socialNotes, decisionNotes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة طلب الإعانة.") : (false, result.Error.Description);
    }

    public async Task<List<PaymentOrderResponse>> GetPaymentOrdersAsync(PaymentOrderType? type = null, PaymentOrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetPaymentOrdersAsync(type, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SavePaymentOrderAsync(int? id, SavePaymentOrderRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SavePaymentOrderAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ أمر الصرف.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecidePaymentOrderAsync(int id, PaymentOrderStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await service.DecidePaymentOrderAsync(id, new DecidePaymentOrderRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث أمر الصرف.") : (false, result.Error.Description);
    }

    public async Task<List<SponsorResponse>> GetSponsorsAsync(SponsorStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetSponsorsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSponsorAsync(int? id, SaveSponsorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveSponsorAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الكافل.") : (false, result.Error.Description);
    }

    public async Task<List<SponsorshipRequirementResponse>> GetRequirementsAsync(SponsorshipStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetSponsorshipRequirementsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveRequirementAsync(int? id, SaveSponsorshipRequirementRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveSponsorshipRequirementAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ احتياج الكفالة.") : (false, result.Error.Description);
    }

    public async Task<List<SponsorshipRecordResponse>> GetSponsorshipRecordsAsync(SponsorshipStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetSponsorshipRecordsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSponsorshipRecordAsync(int? id, SaveSponsorshipRecordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveSponsorshipRecordAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ سجل الكفالة.") : (false, result.Error.Description);
    }

    public async Task<List<SponsorshipPaymentResponse>> GetSponsorshipPaymentsAsync(SponsorshipPaymentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetSponsorshipPaymentsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSponsorshipPaymentAsync(int? id, SaveSponsorshipPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveSponsorshipPaymentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ دفعة الكفالة.") : (false, result.Error.Description);
    }

    public async Task<List<EntitySupportResponse>> GetEntitySupportsAsync(EntitySupportStatus? status = null, bool? isExternal = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetEntitySupportsAsync(status, isExternal, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveEntitySupportAsync(int? id, SaveEntitySupportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveEntitySupportAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ طلب الجهة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideEntitySupportAsync(int id, EntitySupportStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await service.DecideEntitySupportAsync(id, new DecideEntitySupportRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث طلب الجهة.") : (false, result.Error.Description);
    }

    public async Task<List<CouponRequestResponse>> GetCouponsAsync(CouponStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetCouponsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveCouponAsync(int? id, SaveCouponRequestRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.SaveCouponAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ القسيمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateCouponStatusAsync(int id, CouponStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await service.UpdateCouponStatusAsync(id, new UpdateCouponStatusRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث القسيمة.") : (false, result.Error.Description);
    }
}
