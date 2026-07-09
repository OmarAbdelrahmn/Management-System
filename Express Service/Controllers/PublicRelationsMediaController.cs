using Application.Contracts.PublicRelationsMedia;
using Application.Service.PublicRelationsMedia;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class PublicRelationsMediaController(IPublicRelationsMediaService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("partners")]
    public async Task<IActionResult> Partners([FromQuery] PublicRelationRecordStatus? status, CancellationToken ct) => ToAction(await service.GetPartnersAsync(status, ct));
    [HttpPost("partners")]
    public async Task<IActionResult> SavePartner([FromBody] SaveMediaPartnerRequest request, CancellationToken ct) => ToAction(await service.SavePartnerAsync(null, request, ct));
    [HttpGet("events")]
    public async Task<IActionResult> Events([FromQuery] PublicRelationRecordStatus? status, CancellationToken ct) => ToAction(await service.GetEventsAsync(status, ct));
    [HttpPost("events")]
    public async Task<IActionResult> SaveEvent([FromBody] SaveMediaEventRequest request, CancellationToken ct) => ToAction(await service.SaveEventAsync(null, request, ct));
    [HttpGet("visits")]
    public async Task<IActionResult> Visits([FromQuery] PublicRelationRecordStatus? status, CancellationToken ct) => ToAction(await service.GetVisitsAsync(status, ct));
    [HttpPost("visits")]
    public async Task<IActionResult> SaveVisit([FromBody] SaveMediaVisitRequest request, CancellationToken ct) => ToAction(await service.SaveVisitAsync(null, request, ct));
    [HttpGet("website-users")]
    public async Task<IActionResult> WebsiteUsers([FromQuery] WebsiteUserStatus? status, CancellationToken ct) => ToAction(await service.GetWebsiteUsersAsync(status, ct));
    [HttpPost("website-users")]
    public async Task<IActionResult> SaveWebsiteUser([FromBody] SaveWebsiteUserAccountRequest request, CancellationToken ct) => ToAction(await service.SaveWebsiteUserAsync(null, request, ct));
    [HttpPost("website-users/{id:int}/login")]
    public async Task<IActionResult> RecordWebsiteLogin(int id, [FromBody] RecordWebsiteLoginRequest request, CancellationToken ct) => ToAction(await service.RecordWebsiteLoginAsync(id, request, ct));
    [HttpGet("templates")]
    public async Task<IActionResult> Templates([FromQuery] CommunicationChannelType? channel, CancellationToken ct) => ToAction(await service.GetTemplatesAsync(channel, ct));
    [HttpPost("templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] SaveCommunicationTemplateRequest request, CancellationToken ct) => ToAction(await service.SaveTemplateAsync(null, request, ct));
    [HttpGet("lists")]
    public async Task<IActionResult> Lists([FromQuery] CommunicationChannelType? channel, CancellationToken ct) => ToAction(await service.GetListsAsync(channel, ct));
    [HttpPost("lists")]
    public async Task<IActionResult> SaveList([FromBody] SaveCommunicationListRequest request, CancellationToken ct) => ToAction(await service.SaveListAsync(null, request, ct));
    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns([FromQuery] CommunicationChannelType? channel, [FromQuery] CommunicationMessageStatus? status, CancellationToken ct) => ToAction(await service.GetCampaignsAsync(channel, status, ct));
    [HttpPost("campaigns")]
    public async Task<IActionResult> SaveCampaign([FromBody] SaveCommunicationCampaignRequest request, CancellationToken ct) => ToAction(await service.SaveCampaignAsync(null, request, ct));
    [HttpPost("campaigns/{id:int}/send")]
    public async Task<IActionResult> SendCampaign(int id, [FromBody] SendCommunicationCampaignRequest request, CancellationToken ct) => ToAction(await service.SendCampaignAsync(id, request, ct));
    [HttpGet("push-subscribers")]
    public async Task<IActionResult> PushSubscribers([FromQuery] bool? isActive, CancellationToken ct) => ToAction(await service.GetPushSubscribersAsync(isActive, ct));
    [HttpPost("push-subscribers")]
    public async Task<IActionResult> SavePushSubscriber([FromBody] SavePushSubscriberRequest request, CancellationToken ct) => ToAction(await service.SavePushSubscriberAsync(null, request, ct));
    [HttpGet("design")]
    public async Task<IActionResult> Design([FromQuery] bool? isActive, CancellationToken ct) => ToAction(await service.GetDesignSettingsAsync(isActive, ct));
    [HttpPost("design")]
    public async Task<IActionResult> SaveDesign([FromBody] SaveWebsiteDesignSettingRequest request, CancellationToken ct) => ToAction(await service.SaveDesignSettingAsync(null, request, ct));
    [HttpGet("navigation")]
    public async Task<IActionResult> Navigation([FromQuery] string? placement, CancellationToken ct) => ToAction(await service.GetNavigationItemsAsync(placement, ct));
    [HttpPost("navigation")]
    public async Task<IActionResult> SaveNavigation([FromBody] SaveWebsiteNavigationItemRequest request, CancellationToken ct) => ToAction(await service.SaveNavigationItemAsync(null, request, ct));
    [HttpGet("content")]
    public async Task<IActionResult> Content([FromQuery] WebsiteContentType? type, [FromQuery] WebsiteContentStatus? status, CancellationToken ct) => ToAction(await service.GetContentItemsAsync(type, status, ct));
    [HttpPost("content")]
    public async Task<IActionResult> SaveContent([FromBody] SaveWebsiteContentItemRequest request, CancellationToken ct) => ToAction(await service.SaveContentItemAsync(null, request, ct));
    [HttpGet("forms")]
    public async Task<IActionResult> Forms([FromQuery] bool? isActive, CancellationToken ct) => ToAction(await service.GetFormsAsync(isActive, ct));
    [HttpPost("forms")]
    public async Task<IActionResult> SaveForm([FromBody] SaveWebsiteFormRequest request, CancellationToken ct) => ToAction(await service.SaveFormAsync(null, request, ct));
    [HttpGet("form-submissions")]
    public async Task<IActionResult> FormSubmissions([FromQuery] int? formId, CancellationToken ct) => ToAction(await service.GetFormSubmissionsAsync(formId, ct));
    [HttpPost("form-submissions")]
    public async Task<IActionResult> SaveFormSubmission([FromBody] SaveWebsiteFormSubmissionRequest request, CancellationToken ct) => ToAction(await service.SaveFormSubmissionAsync(request, ct));
    [HttpGet("contact-requests")]
    public async Task<IActionResult> ContactRequests([FromQuery] WebsiteContentStatus? status, CancellationToken ct) => ToAction(await service.GetContactRequestsAsync(status, ct));
    [HttpPost("contact-requests")]
    public async Task<IActionResult> SaveContactRequest([FromBody] SaveWebsiteContactRequestRequest request, CancellationToken ct) => ToAction(await service.SaveContactRequestAsync(null, request, ct));
    [HttpPost("contact-requests/{id:int}/status")]
    public async Task<IActionResult> UpdateContactStatus(int id, [FromBody] UpdateWebsiteContactStatusRequest request, CancellationToken ct) => ToAction(await service.UpdateContactStatusAsync(id, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
