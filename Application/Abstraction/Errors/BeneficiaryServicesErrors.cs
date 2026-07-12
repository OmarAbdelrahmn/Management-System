using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class BeneficiaryServicesErrors
{
    public static readonly Error AidRequestNotFound =
        new("BeneficiaryServices.AidRequestNotFound", "Aid request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error PaymentOrderNotFound =
        new("BeneficiaryServices.PaymentOrderNotFound", "Payment order was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SponsorNotFound =
        new("BeneficiaryServices.SponsorNotFound", "Sponsor was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SponsorshipRequirementNotFound =
        new("BeneficiaryServices.SponsorshipRequirementNotFound", "Sponsorship requirement was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SponsorshipRecordNotFound =
        new("BeneficiaryServices.SponsorshipRecordNotFound", "Sponsorship record was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SponsorshipPaymentNotFound =
        new("BeneficiaryServices.SponsorshipPaymentNotFound", "Sponsorship payment was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SponsorshipRecordNotActive =
        new("BeneficiaryServices.SponsorshipRecordNotActive", "Sponsorship record must be active before generating payments.", StatusCodes.Status400BadRequest);

    public static readonly Error SponsorshipPaymentsAlreadyGenerated =
        new("BeneficiaryServices.SponsorshipPaymentsAlreadyGenerated", "Sponsorship payments have already been generated for this record.", StatusCodes.Status409Conflict);

    public static readonly Error EntitySupportNotFound =
        new("BeneficiaryServices.EntitySupportNotFound", "Entity support request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CouponNotFound =
        new("BeneficiaryServices.CouponNotFound", "Coupon request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidCouponStatusTransition =
        new("BeneficiaryServices.InvalidCouponStatusTransition", "Coupon status transition is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRequest =
        new("BeneficiaryServices.InvalidRequest", "Beneficiary service request is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateAidRequestNumber =
        new("BeneficiaryServices.DuplicateAidRequestNumber", "Aid request number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicatePaymentOrderNumber =
        new("BeneficiaryServices.DuplicatePaymentOrderNumber", "Payment order number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicatePaymentOrderForAidRequest =
        new("BeneficiaryServices.DuplicatePaymentOrderForAidRequest", "A payment order already exists for this aid request.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicatePaymentOrderForEntitySupport =
        new("BeneficiaryServices.DuplicatePaymentOrderForEntitySupport", "A payment order already exists for this entity support request.", StatusCodes.Status409Conflict);

    public static readonly Error AidRequestNotApproved =
        new("BeneficiaryServices.AidRequestNotApproved", "Aid request must be approved before creating a payment order.", StatusCodes.Status400BadRequest);

    public static readonly Error EntitySupportNotApproved =
        new("BeneficiaryServices.EntitySupportNotApproved", "Entity support request must be approved before creating a payment order.", StatusCodes.Status400BadRequest);
}
