using Domain.Entities;

namespace Application.Contracts.BeneficiaryServices;

public record BeneficiaryServicesDashboardResponse(
    int AidRequestsCount,
    int PendingAidRequestsCount,
    int PaymentOrdersCount,
    int SponsorshipRecordsCount,
    int DueSponsorshipPaymentsCount,
    int EntitySupportRequestsCount,
    int CouponRequestsCount,
    decimal ApprovedAidAmount,
    decimal PaymentOrdersAmount);

public record AidRequestResponse(
    int Id,
    int? BeneficiaryProfileId,
    string? BeneficiaryName,
    int? BeneficiaryEntityId,
    string? EntityName,
    string RequestNumber,
    string AidType,
    decimal Amount,
    string Description,
    string Status,
    bool IsExternal,
    string? SocialResearchNotes,
    string? DecisionNotes,
    DateTime CreatedAt);

public record SaveAidRequestRequest(
    int? BeneficiaryProfileId,
    int? BeneficiaryEntityId,
    string? RequestNumber,
    string AidType,
    decimal Amount,
    string Description,
    bool IsExternal);

public record DecideAidRequestRequest(
    AidRequestStatus Status,
    string? SocialResearchNotes,
    string? DecisionNotes);

public record PaymentOrderResponse(
    int Id,
    int? BeneficiaryAidRequestId,
    int? BeneficiaryProfileId,
    string? BeneficiaryName,
    string OrderNumber,
    string OrderType,
    decimal Amount,
    string ItemDescription,
    string Status,
    DateTime? DueDate,
    string? DecisionNotes,
    DateTime? ClosedAt);

public record SavePaymentOrderRequest(
    int? BeneficiaryAidRequestId,
    int? BeneficiaryProfileId,
    string? OrderNumber,
    PaymentOrderType OrderType,
    decimal Amount,
    string ItemDescription,
    DateTime? DueDate);

public record DecidePaymentOrderRequest(
    PaymentOrderStatus Status,
    string? DecisionNotes);

public record SponsorResponse(
    int Id,
    string FullName,
    string? Mobile,
    string? Email,
    string Status,
    string? Notes);

public record SaveSponsorRequest(
    string FullName,
    string? Mobile,
    string? Email,
    SponsorStatus Status,
    string? Notes);

public record SponsorshipRequirementResponse(
    int Id,
    string Title,
    decimal Amount,
    string Frequency,
    string Status,
    string? Notes);

public record SaveSponsorshipRequirementRequest(
    string Title,
    decimal Amount,
    string Frequency,
    SponsorshipStatus Status,
    string? Notes);

public record SponsorshipRecordResponse(
    int Id,
    int SponsorId,
    string SponsorName,
    int? BeneficiaryProfileId,
    string? BeneficiaryName,
    int? SponsorshipRequirementId,
    string? RequirementTitle,
    DateTime StartsAt,
    DateTime? EndsAt,
    decimal Amount,
    string Status,
    decimal PaidAmount,
    decimal PendingAmount,
    string? Notes);

public record SaveSponsorshipRecordRequest(
    int SponsorId,
    int? BeneficiaryProfileId,
    int? SponsorshipRequirementId,
    DateTime StartsAt,
    DateTime? EndsAt,
    decimal Amount,
    SponsorshipStatus Status,
    string? Notes);

public record SponsorshipPaymentResponse(
    int Id,
    int SponsorshipRecordId,
    string SponsorName,
    string? BeneficiaryName,
    DateTime DueDate,
    decimal Amount,
    DateTime? PaidAt,
    string Status,
    string? Notes);

public record SaveSponsorshipPaymentRequest(
    int SponsorshipRecordId,
    DateTime DueDate,
    decimal Amount,
    SponsorshipPaymentStatus Status,
    string? Notes);

public record EntitySupportResponse(
    int Id,
    int? BeneficiaryEntityId,
    string? EntityName,
    string RequesterName,
    string SupportType,
    decimal Amount,
    bool IsExternal,
    string Status,
    string? DecisionNotes,
    DateTime CreatedAt);

public record SaveEntitySupportRequest(
    int? BeneficiaryEntityId,
    string RequesterName,
    string SupportType,
    decimal Amount,
    bool IsExternal);

public record DecideEntitySupportRequest(
    EntitySupportStatus Status,
    string? DecisionNotes);

public record CouponRequestResponse(
    int Id,
    int? BeneficiaryProfileId,
    string? BeneficiaryName,
    string CouponType,
    decimal Amount,
    string Status,
    DateTime RequiredAt,
    DateTime? IssuedAt,
    DateTime? DeliveredAt,
    string? Notes);

public record SaveCouponRequestRequest(
    int? BeneficiaryProfileId,
    string CouponType,
    decimal Amount,
    DateTime? RequiredAt,
    string? Notes);

public record UpdateCouponStatusRequest(
    CouponStatus Status,
    string? Notes);
