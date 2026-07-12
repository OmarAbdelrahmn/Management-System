using Application.Contracts.TechEnablement;
using Application.Service.TechEnablement;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class TechEnablementController(ITechEnablementService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("settings")]
    public async Task<IActionResult> Settings([FromQuery] string? category, [FromQuery] TechSettingStatus? status, CancellationToken ct) => ToAction(await service.GetSettingsAsync(category, status, ct));
    [HttpPost("settings")]
    public async Task<IActionResult> SaveSetting([FromBody] SaveTechSystemSettingRequest request, CancellationToken ct) => ToAction(await service.SaveSettingAsync(null, request, ct));
    [HttpPut("settings/{id:int}")]
    public async Task<IActionResult> UpdateSetting(int id, [FromBody] SaveTechSystemSettingRequest request, CancellationToken ct) => ToAction(await service.SaveSettingAsync(id, request, ct));
    [HttpGet("organization")]
    public async Task<IActionResult> Organization([FromQuery] OrganizationAssignmentType? assignmentType, [FromQuery] bool? isActive, CancellationToken ct) => ToAction(await service.GetOrganizationAssignmentsAsync(assignmentType, isActive, ct));
    [HttpPost("organization")]
    public async Task<IActionResult> SaveOrganization([FromBody] SaveOrganizationAssignmentRequest request, CancellationToken ct) => ToAction(await service.SaveOrganizationAssignmentAsync(null, request, ct));
    [HttpPut("organization/{id:int}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] SaveOrganizationAssignmentRequest request, CancellationToken ct) => ToAction(await service.SaveOrganizationAssignmentAsync(id, request, ct));
    [HttpGet("visual-assets")]
    public async Task<IActionResult> VisualAssets([FromQuery] VisualAssetType? assetType, [FromQuery] bool? isActive, CancellationToken ct) => ToAction(await service.GetVisualAssetsAsync(assetType, isActive, ct));
    [HttpPost("visual-assets")]
    public async Task<IActionResult> SaveVisualAsset([FromBody] SaveVisualAssetTemplateRequest request, CancellationToken ct) => ToAction(await service.SaveVisualAssetAsync(null, request, ct));
    [HttpPut("visual-assets/{id:int}")]
    public async Task<IActionResult> UpdateVisualAsset(int id, [FromBody] SaveVisualAssetTemplateRequest request, CancellationToken ct) => ToAction(await service.SaveVisualAssetAsync(id, request, ct));
    [HttpGet("cybersecurity")]
    public async Task<IActionResult> Cybersecurity([FromQuery] CybersecurityReviewStatus? status, CancellationToken ct) => ToAction(await service.GetCybersecurityReviewsAsync(status, ct));
    [HttpPost("cybersecurity")]
    public async Task<IActionResult> SaveCybersecurity([FromBody] SaveCybersecurityReviewRequest request, CancellationToken ct) => ToAction(await service.SaveCybersecurityReviewAsync(null, request, ct));
    [HttpPut("cybersecurity/{id:int}")]
    public async Task<IActionResult> UpdateCybersecurity(int id, [FromBody] SaveCybersecurityReviewRequest request, CancellationToken ct) => ToAction(await service.SaveCybersecurityReviewAsync(id, request, ct));
    [HttpPost("cybersecurity/{id:int}/status")]
    public async Task<IActionResult> UpdateCybersecurityStatus(int id, [FromBody] UpdateCybersecurityReviewStatusRequest request, CancellationToken ct) => ToAction(await service.UpdateCybersecurityStatusAsync(id, request, ct));
    [HttpGet("ncnp")]
    public async Task<IActionResult> Ncnp([FromQuery] NcnpDataStatus? status, CancellationToken ct) => ToAction(await service.GetNcnpDataAsync(status, ct));
    [HttpPost("ncnp")]
    public async Task<IActionResult> SaveNcnp([FromBody] SaveNcnpDataRecordRequest request, CancellationToken ct) => ToAction(await service.SaveNcnpDataAsync(null, request, ct));
    [HttpPut("ncnp/{id:int}")]
    public async Task<IActionResult> UpdateNcnp(int id, [FromBody] SaveNcnpDataRecordRequest request, CancellationToken ct) => ToAction(await service.SaveNcnpDataAsync(id, request, ct));
    [HttpPost("ncnp/{id:int}/status")]
    public async Task<IActionResult> UpdateNcnpStatus(int id, [FromBody] UpdateNcnpDataStatusRequest request, CancellationToken ct)
    {
        var result = await service.UpdateNcnpStatusAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
