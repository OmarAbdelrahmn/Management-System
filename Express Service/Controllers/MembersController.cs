using Application.Contracts.Members;
using Application.Service.Members;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        var result = await memberService.GetMembershipTypesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("types")]
    public async Task<IActionResult> CreateType([FromBody] UpsertMembershipTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.CreateMembershipTypeAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("types/{id:int}")]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpsertMembershipTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.UpdateMembershipTypeAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] MemberStatus? status,
        [FromQuery] int? membershipTypeId,
        [FromQuery] bool? feesPaid,
        [FromQuery] bool? isSupporter,
        CancellationToken cancellationToken)
    {
        var result = await memberService.SearchAsync(new MemberSearchRequest(search, status, membershipTypeId, feesPaid, isSupporter), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await memberService.GetAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.CancelAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await memberService.RestoreAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("due")]
    public async Task<IActionResult> Due(CancellationToken cancellationToken)
    {
        var result = await memberService.GetDueMembersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("payments")]
    public async Task<IActionResult> Payments([FromQuery] int? memberId, CancellationToken cancellationToken)
    {
        var result = await memberService.GetPaymentsAsync(memberId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] RecordMemberPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.RecordPaymentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("cards")]
    public async Task<IActionResult> Cards(CancellationToken cancellationToken)
    {
        var result = await memberService.GetCardsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/cards")]
    public async Task<IActionResult> IssueCard(int id, [FromBody] IssueMemberCardRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.IssueCardAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("reports/shares")]
    public async Task<IActionResult> ReportShares(CancellationToken cancellationToken)
    {
        var result = await memberService.GetReportSharesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("reports/share")]
    public async Task<IActionResult> ShareReport([FromBody] ShareMemberReportRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.ShareReportAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
