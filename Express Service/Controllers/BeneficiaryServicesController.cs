using Application.Contracts.BeneficiaryServices;
using Application.Service.BeneficiaryServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class BeneficiaryServicesController(IBeneficiaryServicesService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("aid-requests")]
    public async Task<IActionResult> AidRequests([FromQuery] AidRequestStatus? status, [FromQuery] bool? isExternal, CancellationToken cancellationToken)
    {
        var result = await service.GetAidRequestsAsync(status, isExternal, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("aid-requests")]
    public async Task<IActionResult> SaveAidRequest([FromBody] SaveAidRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveAidRequestAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("aid-requests/{id:int}/decision")]
    public async Task<IActionResult> DecideAidRequest(int id, [FromBody] DecideAidRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await service.DecideAidRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("payment-orders")]
    public async Task<IActionResult> PaymentOrders([FromQuery] PaymentOrderType? type, [FromQuery] PaymentOrderStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetPaymentOrdersAsync(type, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("payment-orders")]
    public async Task<IActionResult> SavePaymentOrder([FromBody] SavePaymentOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SavePaymentOrderAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("payment-orders/{id:int}/decision")]
    public async Task<IActionResult> DecidePaymentOrder(int id, [FromBody] DecidePaymentOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await service.DecidePaymentOrderAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sponsors")]
    public async Task<IActionResult> Sponsors([FromQuery] SponsorStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetSponsorsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("sponsors")]
    public async Task<IActionResult> SaveSponsor([FromBody] SaveSponsorRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveSponsorAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sponsorship-requirements")]
    public async Task<IActionResult> SponsorshipRequirements([FromQuery] SponsorshipStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetSponsorshipRequirementsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("sponsorship-requirements")]
    public async Task<IActionResult> SaveSponsorshipRequirement([FromBody] SaveSponsorshipRequirementRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveSponsorshipRequirementAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sponsorship-records")]
    public async Task<IActionResult> SponsorshipRecords([FromQuery] SponsorshipStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetSponsorshipRecordsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("sponsorship-records")]
    public async Task<IActionResult> SaveSponsorshipRecord([FromBody] SaveSponsorshipRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveSponsorshipRecordAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sponsorship-payments")]
    public async Task<IActionResult> SponsorshipPayments([FromQuery] SponsorshipPaymentStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetSponsorshipPaymentsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("sponsorship-payments")]
    public async Task<IActionResult> SaveSponsorshipPayment([FromBody] SaveSponsorshipPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveSponsorshipPaymentAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("entity-supports")]
    public async Task<IActionResult> EntitySupports([FromQuery] EntitySupportStatus? status, [FromQuery] bool? isExternal, CancellationToken cancellationToken)
    {
        var result = await service.GetEntitySupportsAsync(status, isExternal, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("entity-supports")]
    public async Task<IActionResult> SaveEntitySupport([FromBody] SaveEntitySupportRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveEntitySupportAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("entity-supports/{id:int}/decision")]
    public async Task<IActionResult> DecideEntitySupport(int id, [FromBody] DecideEntitySupportRequest request, CancellationToken cancellationToken)
    {
        var result = await service.DecideEntitySupportAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("coupons")]
    public async Task<IActionResult> Coupons([FromQuery] CouponStatus? status, CancellationToken cancellationToken)
    {
        var result = await service.GetCouponsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("coupons")]
    public async Task<IActionResult> SaveCoupon([FromBody] SaveCouponRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveCouponAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("coupons/{id:int}/status")]
    public async Task<IActionResult> UpdateCouponStatus(int id, [FromBody] UpdateCouponStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateCouponStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
