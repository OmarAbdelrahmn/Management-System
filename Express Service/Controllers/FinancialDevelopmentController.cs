using Application.Contracts.FinancialDevelopment;
using Application.Service.FinancialDevelopment;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class FinancialDevelopmentController(IFinancialDevelopmentService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));

    [HttpGet("supporters")]
    public async Task<IActionResult> Supporters([FromQuery] FinancialSupporterStatus? status, [FromQuery] string? search, CancellationToken ct) => ToAction(await service.GetSupportersAsync(status, search, ct));

    [HttpPost("supporters")]
    public async Task<IActionResult> SaveSupporter([FromBody] SaveFinancialSupporterRequest request, CancellationToken ct) => ToAction(await service.SaveSupporterAsync(null, request, ct));

    [HttpPut("supporters/{id:int}")]
    public async Task<IActionResult> UpdateSupporter(int id, [FromBody] SaveFinancialSupporterRequest request, CancellationToken ct) => ToAction(await service.SaveSupporterAsync(id, request, ct));

    [HttpGet("opportunities")]
    public async Task<IActionResult> Opportunities([FromQuery] FundraisingOpportunityType? type, [FromQuery] FundraisingOpportunityStatus? status, CancellationToken ct) => ToAction(await service.GetOpportunitiesAsync(type, status, ct));

    [HttpPost("opportunities")]
    public async Task<IActionResult> SaveOpportunity([FromBody] SaveFundraisingOpportunityRequest request, CancellationToken ct) => ToAction(await service.SaveOpportunityAsync(null, request, ct));

    [HttpPut("opportunities/{id:int}")]
    public async Task<IActionResult> UpdateOpportunity(int id, [FromBody] SaveFundraisingOpportunityRequest request, CancellationToken ct) => ToAction(await service.SaveOpportunityAsync(id, request, ct));

    [HttpGet("contributions")]
    public async Task<IActionResult> Contributions([FromQuery] DonationContributionStatus? status, [FromQuery] int? supporterId, [FromQuery] int? opportunityId, CancellationToken ct) => ToAction(await service.GetContributionsAsync(status, supporterId, opportunityId, ct));

    [HttpPost("contributions")]
    public async Task<IActionResult> SaveContribution([FromBody] SaveDonationContributionRequest request, CancellationToken ct) => ToAction(await service.SaveContributionAsync(null, request, ct));

    [HttpPut("contributions/{id:int}")]
    public async Task<IActionResult> UpdateContribution(int id, [FromBody] SaveDonationContributionRequest request, CancellationToken ct) => ToAction(await service.SaveContributionAsync(id, request, ct));

    [HttpGet("reports/donations")]
    public async Task<IActionResult> DonationReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) => ToAction(await service.GetDonationReportAsync(from, to, ct));

    [HttpGet("digital-campaigns")]
    public async Task<IActionResult> DigitalCampaigns([FromQuery] DigitalMarketingCampaignStatus? status, CancellationToken ct) => ToAction(await service.GetDigitalCampaignsAsync(status, ct));

    [HttpPost("digital-campaigns")]
    public async Task<IActionResult> SaveDigitalCampaign([FromBody] SaveDigitalMarketingCampaignRequest request, CancellationToken ct) => ToAction(await service.SaveDigitalCampaignAsync(null, request, ct));

    [HttpPut("digital-campaigns/{id:int}")]
    public async Task<IActionResult> UpdateDigitalCampaign(int id, [FromBody] SaveDigitalMarketingCampaignRequest request, CancellationToken ct) => ToAction(await service.SaveDigitalCampaignAsync(id, request, ct));

    [HttpGet("abandoned-carts")]
    public async Task<IActionResult> AbandonedCarts([FromQuery] AbandonedDonationCartStatus? status, CancellationToken ct) => ToAction(await service.GetAbandonedCartsAsync(status, ct));

    [HttpPost("abandoned-carts")]
    public async Task<IActionResult> SaveAbandonedCart([FromBody] SaveAbandonedDonationCartRequest request, CancellationToken ct) => ToAction(await service.SaveAbandonedCartAsync(null, request, ct));

    [HttpPut("abandoned-carts/{id:int}")]
    public async Task<IActionResult> UpdateAbandonedCart(int id, [FromBody] SaveAbandonedDonationCartRequest request, CancellationToken ct) => ToAction(await service.SaveAbandonedCartAsync(id, request, ct));

    [HttpGet("endowments")]
    public async Task<IActionResult> Endowments([FromQuery] EndowmentAssetStatus? status, CancellationToken ct) => ToAction(await service.GetEndowmentsAsync(status, ct));

    [HttpPost("endowments")]
    public async Task<IActionResult> SaveEndowment([FromBody] SaveEndowmentAssetRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentAsync(null, request, ct));

    [HttpPut("endowments/{id:int}")]
    public async Task<IActionResult> UpdateEndowment(int id, [FromBody] SaveEndowmentAssetRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentAsync(id, request, ct));

    [HttpGet("endowment-contracts")]
    public async Task<IActionResult> EndowmentContracts([FromQuery] int? endowmentAssetId, CancellationToken ct) => ToAction(await service.GetEndowmentContractsAsync(endowmentAssetId, ct));

    [HttpPost("endowment-contracts")]
    public async Task<IActionResult> SaveEndowmentContract([FromBody] SaveEndowmentContractRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentContractAsync(null, request, ct));

    [HttpPut("endowment-contracts/{id:int}")]
    public async Task<IActionResult> UpdateEndowmentContract(int id, [FromBody] SaveEndowmentContractRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentContractAsync(id, request, ct));

    [HttpGet("endowment-invoices")]
    public async Task<IActionResult> EndowmentInvoices([FromQuery] EndowmentInvoiceStatus? status, [FromQuery] bool dueSoonOnly, CancellationToken ct) => ToAction(await service.GetEndowmentInvoicesAsync(status, dueSoonOnly, ct));

    [HttpPost("endowment-invoices")]
    public async Task<IActionResult> SaveEndowmentInvoice([FromBody] SaveEndowmentInvoiceRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentInvoiceAsync(null, request, ct));

    [HttpPut("endowment-invoices/{id:int}")]
    public async Task<IActionResult> UpdateEndowmentInvoice(int id, [FromBody] SaveEndowmentInvoiceRequest request, CancellationToken ct) => ToAction(await service.SaveEndowmentInvoiceAsync(id, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
