using Application.Contracts.PublicRelationsMedia;
using Application.Service.PublicRelationsMedia;
using Domain.Entities;

namespace Express_Service.Services;

public class PublicRelationsMediaUiService(IPublicRelationsMediaService service)
{
    public async Task<PublicRelationsMediaDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<MediaPartnerResponse>> GetPartnersAsync(PublicRelationRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetPartnersAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SavePartnerAsync(int? id, SaveMediaPartnerRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SavePartnerAsync(id, request, cancellationToken), "تم حفظ الشريك الإعلامي.");
    public async Task<List<MediaEventResponse>> GetEventsAsync(PublicRelationRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetEventsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveEventAsync(int? id, SaveMediaEventRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveEventAsync(id, request, cancellationToken), "تم حفظ الفعالية.");
    public async Task<List<MediaVisitResponse>> GetVisitsAsync(PublicRelationRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetVisitsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveVisitAsync(int? id, SaveMediaVisitRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveVisitAsync(id, request, cancellationToken), "تم حفظ الزيارة.");
    public async Task<List<WebsiteUserAccountResponse>> GetWebsiteUsersAsync(WebsiteUserStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetWebsiteUsersAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveWebsiteUserAsync(int? id, SaveWebsiteUserAccountRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveWebsiteUserAsync(id, request, cancellationToken), "تم حفظ مستخدم الموقع.");
    public async Task<(bool Success, string Message)> RecordWebsiteLoginAsync(int id, CancellationToken cancellationToken = default) => ToUi(await service.RecordWebsiteLoginAsync(id, new RecordWebsiteLoginRequest(DateTime.UtcNow.AddHours(3)), cancellationToken), "تم تسجيل الدخول.");
    public async Task<List<CommunicationTemplateResponse>> GetTemplatesAsync(CommunicationChannelType? channel = null, CancellationToken cancellationToken = default) => ToList(await service.GetTemplatesAsync(channel, cancellationToken));
    public async Task<(bool Success, string Message)> SaveTemplateAsync(int? id, SaveCommunicationTemplateRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveTemplateAsync(id, request, cancellationToken), "تم حفظ القالب.");
    public async Task<List<CommunicationListResponse>> GetListsAsync(CommunicationChannelType? channel = null, CancellationToken cancellationToken = default) => ToList(await service.GetListsAsync(channel, cancellationToken));
    public async Task<(bool Success, string Message)> SaveListAsync(int? id, SaveCommunicationListRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveListAsync(id, request, cancellationToken), "تم حفظ القائمة.");
    public async Task<List<CommunicationCampaignResponse>> GetCampaignsAsync(CommunicationChannelType? channel = null, CommunicationMessageStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetCampaignsAsync(channel, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveCampaignAsync(int? id, SaveCommunicationCampaignRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveCampaignAsync(id, request, cancellationToken), "تم حفظ الحملة.");
    public async Task<(bool Success, string Message)> SendCampaignAsync(int id, bool markSent = true, CancellationToken cancellationToken = default) => ToUi(await service.SendCampaignAsync(id, new SendCommunicationCampaignRequest(markSent), cancellationToken), markSent ? "تم إرسال الحملة." : "تم تسجيل فشل الحملة.");
    public async Task<List<PushSubscriberResponse>> GetPushSubscribersAsync(bool? isActive = null, CancellationToken cancellationToken = default) => ToList(await service.GetPushSubscribersAsync(isActive, cancellationToken));
    public async Task<(bool Success, string Message)> SavePushSubscriberAsync(int? id, SavePushSubscriberRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SavePushSubscriberAsync(id, request, cancellationToken), "تم حفظ مشترك التنبيه.");
    public async Task<List<WebsiteDesignSettingResponse>> GetDesignSettingsAsync(bool? isActive = null, CancellationToken cancellationToken = default) => ToList(await service.GetDesignSettingsAsync(isActive, cancellationToken));
    public async Task<(bool Success, string Message)> SaveDesignSettingAsync(int? id, SaveWebsiteDesignSettingRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveDesignSettingAsync(id, request, cancellationToken), "تم حفظ التصميم.");
    public async Task<List<WebsiteNavigationItemResponse>> GetNavigationItemsAsync(string? placement = null, CancellationToken cancellationToken = default) => ToList(await service.GetNavigationItemsAsync(placement, cancellationToken));
    public async Task<(bool Success, string Message)> SaveNavigationItemAsync(int? id, SaveWebsiteNavigationItemRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveNavigationItemAsync(id, request, cancellationToken), "تم حفظ عنصر التنقل.");
    public async Task<List<WebsiteContentItemResponse>> GetContentItemsAsync(WebsiteContentType? type = null, WebsiteContentStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetContentItemsAsync(type, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveContentItemAsync(int? id, SaveWebsiteContentItemRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveContentItemAsync(id, request, cancellationToken), "تم حفظ المحتوى.");
    public async Task<List<WebsiteFormResponse>> GetFormsAsync(bool? isActive = null, CancellationToken cancellationToken = default) => ToList(await service.GetFormsAsync(isActive, cancellationToken));
    public async Task<(bool Success, string Message)> SaveFormAsync(int? id, SaveWebsiteFormRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveFormAsync(id, request, cancellationToken), "تم حفظ النموذج.");
    public async Task<List<WebsiteFormSubmissionResponse>> GetFormSubmissionsAsync(int? formId = null, CancellationToken cancellationToken = default) => ToList(await service.GetFormSubmissionsAsync(formId, cancellationToken));
    public async Task<(bool Success, string Message)> SaveFormSubmissionAsync(SaveWebsiteFormSubmissionRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveFormSubmissionAsync(request, cancellationToken), "تم حفظ إدخال النموذج.");
    public async Task<List<WebsiteContactRequestResponse>> GetContactRequestsAsync(WebsiteContentStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetContactRequestsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveContactRequestAsync(int? id, SaveWebsiteContactRequestRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveContactRequestAsync(id, request, cancellationToken), "تم حفظ طلب التواصل.");
    public async Task<(bool Success, string Message)> UpdateContactStatusAsync(int id, WebsiteContentStatus status, CancellationToken cancellationToken = default) => ToUi(await service.UpdateContactStatusAsync(id, new UpdateWebsiteContactStatusRequest(status), cancellationToken), "تم تحديث طلب التواصل.");

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
