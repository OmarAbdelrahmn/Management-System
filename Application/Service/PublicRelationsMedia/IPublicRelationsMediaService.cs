using Application.Abstraction;
using Application.Contracts.PublicRelationsMedia;
using Domain.Entities;

namespace Application.Service.PublicRelationsMedia;

public interface IPublicRelationsMediaService
{
    Task<Result<PublicRelationsMediaDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MediaPartnerResponse>>> GetPartnersAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<MediaPartnerResponse>> SavePartnerAsync(int? id, SaveMediaPartnerRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MediaEventResponse>>> GetEventsAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<MediaEventResponse>> SaveEventAsync(int? id, SaveMediaEventRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MediaVisitResponse>>> GetVisitsAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default);
    Task<Result<MediaVisitResponse>> SaveVisitAsync(int? id, SaveMediaVisitRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteUserAccountResponse>>> GetWebsiteUsersAsync(WebsiteUserStatus? status, CancellationToken cancellationToken = default);
    Task<Result<WebsiteUserAccountResponse>> SaveWebsiteUserAsync(int? id, SaveWebsiteUserAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<WebsiteUserAccountResponse>> RecordWebsiteLoginAsync(int id, RecordWebsiteLoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CommunicationTemplateResponse>>> GetTemplatesAsync(CommunicationChannelType? channelType, CancellationToken cancellationToken = default);
    Task<Result<CommunicationTemplateResponse>> SaveTemplateAsync(int? id, SaveCommunicationTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CommunicationListResponse>>> GetListsAsync(CommunicationChannelType? channelType, CancellationToken cancellationToken = default);
    Task<Result<CommunicationListResponse>> SaveListAsync(int? id, SaveCommunicationListRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CommunicationCampaignResponse>>> GetCampaignsAsync(CommunicationChannelType? channelType, CommunicationMessageStatus? status, CancellationToken cancellationToken = default);
    Task<Result<CommunicationCampaignResponse>> SaveCampaignAsync(int? id, SaveCommunicationCampaignRequest request, CancellationToken cancellationToken = default);
    Task<Result<CommunicationCampaignResponse>> SendCampaignAsync(int id, SendCommunicationCampaignRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PushSubscriberResponse>>> GetPushSubscribersAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<Result<PushSubscriberResponse>> SavePushSubscriberAsync(int? id, SavePushSubscriberRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteDesignSettingResponse>>> GetDesignSettingsAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<Result<WebsiteDesignSettingResponse>> SaveDesignSettingAsync(int? id, SaveWebsiteDesignSettingRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteNavigationItemResponse>>> GetNavigationItemsAsync(string? placement, CancellationToken cancellationToken = default);
    Task<Result<WebsiteNavigationItemResponse>> SaveNavigationItemAsync(int? id, SaveWebsiteNavigationItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteContentItemResponse>>> GetContentItemsAsync(WebsiteContentType? contentType, WebsiteContentStatus? status, CancellationToken cancellationToken = default);
    Task<Result<WebsiteContentItemResponse>> SaveContentItemAsync(int? id, SaveWebsiteContentItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteFormResponse>>> GetFormsAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<Result<WebsiteFormResponse>> SaveFormAsync(int? id, SaveWebsiteFormRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteFormSubmissionResponse>>> GetFormSubmissionsAsync(int? formId, CancellationToken cancellationToken = default);
    Task<Result<WebsiteFormSubmissionResponse>> SaveFormSubmissionAsync(SaveWebsiteFormSubmissionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WebsiteContactRequestResponse>>> GetContactRequestsAsync(WebsiteContentStatus? status, CancellationToken cancellationToken = default);
    Task<Result<WebsiteContactRequestResponse>> SaveContactRequestAsync(int? id, SaveWebsiteContactRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<WebsiteContactRequestResponse>> UpdateContactStatusAsync(int id, UpdateWebsiteContactStatusRequest request, CancellationToken cancellationToken = default);
}
