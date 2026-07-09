using Application.Abstraction;
using Application.Contracts.BeneficiaryServices;
using Domain.Entities;

namespace Application.Service.BeneficiaryServices;

public interface IBeneficiaryServicesService
{
    Task<Result<BeneficiaryServicesDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AidRequestResponse>>> GetAidRequestsAsync(AidRequestStatus? status, bool? isExternal, CancellationToken cancellationToken = default);
    Task<Result<AidRequestResponse>> SaveAidRequestAsync(int? id, SaveAidRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<AidRequestResponse>> DecideAidRequestAsync(int id, DecideAidRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PaymentOrderResponse>>> GetPaymentOrdersAsync(PaymentOrderType? type, PaymentOrderStatus? status, CancellationToken cancellationToken = default);
    Task<Result<PaymentOrderResponse>> SavePaymentOrderAsync(int? id, SavePaymentOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentOrderResponse>> DecidePaymentOrderAsync(int id, DecidePaymentOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SponsorResponse>>> GetSponsorsAsync(SponsorStatus? status, CancellationToken cancellationToken = default);
    Task<Result<SponsorResponse>> SaveSponsorAsync(int? id, SaveSponsorRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SponsorshipRequirementResponse>>> GetSponsorshipRequirementsAsync(SponsorshipStatus? status, CancellationToken cancellationToken = default);
    Task<Result<SponsorshipRequirementResponse>> SaveSponsorshipRequirementAsync(int? id, SaveSponsorshipRequirementRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SponsorshipRecordResponse>>> GetSponsorshipRecordsAsync(SponsorshipStatus? status, CancellationToken cancellationToken = default);
    Task<Result<SponsorshipRecordResponse>> SaveSponsorshipRecordAsync(int? id, SaveSponsorshipRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SponsorshipPaymentResponse>>> GetSponsorshipPaymentsAsync(SponsorshipPaymentStatus? status, CancellationToken cancellationToken = default);
    Task<Result<SponsorshipPaymentResponse>> SaveSponsorshipPaymentAsync(int? id, SaveSponsorshipPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EntitySupportResponse>>> GetEntitySupportsAsync(EntitySupportStatus? status, bool? isExternal, CancellationToken cancellationToken = default);
    Task<Result<EntitySupportResponse>> SaveEntitySupportAsync(int? id, SaveEntitySupportRequest request, CancellationToken cancellationToken = default);
    Task<Result<EntitySupportResponse>> DecideEntitySupportAsync(int id, DecideEntitySupportRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CouponRequestResponse>>> GetCouponsAsync(CouponStatus? status, CancellationToken cancellationToken = default);
    Task<Result<CouponRequestResponse>> SaveCouponAsync(int? id, SaveCouponRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<CouponRequestResponse>> UpdateCouponStatusAsync(int id, UpdateCouponStatusRequest request, CancellationToken cancellationToken = default);
}
