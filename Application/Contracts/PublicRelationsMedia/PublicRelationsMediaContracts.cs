using Domain.Entities;

namespace Application.Contracts.PublicRelationsMedia;

public record PublicRelationsMediaDashboardResponse(
    int PartnersCount,
    int EventsCount,
    int VisitsCount,
    int WebsiteUsersCount,
    int CampaignsCount,
    int PublishedContentCount,
    int ContactRequestsCount);

public record MediaPartnerResponse(int Id, string Name, string? ContactPerson, string? Mobile, string? Email, string Status, string? Notes);
public record SaveMediaPartnerRequest(string Name, string? ContactPerson, string? Mobile, string? Email, PublicRelationRecordStatus Status, string? Notes);

public record MediaEventResponse(int Id, string Title, DateTime EventDate, string? Location, string? Description, string Status);
public record SaveMediaEventRequest(string Title, DateTime EventDate, string? Location, string? Description, PublicRelationRecordStatus Status);

public record MediaVisitResponse(int Id, string VisitorName, string? Organization, DateTime VisitDate, string? Purpose, string Status);
public record SaveMediaVisitRequest(string VisitorName, string? Organization, DateTime VisitDate, string? Purpose, PublicRelationRecordStatus Status);

public record WebsiteUserAccountResponse(int Id, string FullName, string Username, string? Email, string RoleName, string Status, DateTime? LastLoginAt);
public record SaveWebsiteUserAccountRequest(string FullName, string Username, string? Email, string RoleName, WebsiteUserStatus Status);
public record RecordWebsiteLoginRequest(DateTime? LoginAt);

public record CommunicationTemplateResponse(int Id, string ChannelType, string Name, string Body, bool IsActive);
public record SaveCommunicationTemplateRequest(CommunicationChannelType ChannelType, string Name, string Body, bool IsActive);

public record CommunicationListResponse(int Id, string ChannelType, string Name, string RecipientsCsv, bool IsActive);
public record SaveCommunicationListRequest(CommunicationChannelType ChannelType, string Name, string RecipientsCsv, bool IsActive);

public record CommunicationCampaignResponse(
    int Id,
    string ChannelType,
    int? CommunicationTemplateId,
    int? CommunicationListId,
    string Title,
    string MessageBody,
    string Status,
    DateTime? SentAt,
    int RecipientsCount,
    int DeliveredCount,
    int FailedCount,
    IReadOnlyList<CommunicationCampaignRecipientResponse> Recipients);

public record CommunicationCampaignRecipientResponse(
    int Id,
    string Recipient,
    string Status,
    DateTime? AttemptedAt,
    DateTime? DeliveredAt,
    string? Error);

public record SaveCommunicationCampaignRequest(CommunicationChannelType ChannelType, int? CommunicationTemplateId, int? CommunicationListId, string Title, string MessageBody, CommunicationMessageStatus Status);
public record SendCommunicationCampaignRequest(bool MarkSent);

public record PushSubscriberResponse(int Id, string DisplayName, string Endpoint, bool IsActive);
public record SavePushSubscriberRequest(string DisplayName, string Endpoint, bool IsActive);

public record WebsiteDesignSettingResponse(int Id, string ThemeName, string PrimaryColor, string FontFamily, string TemplateName, string? CustomCss, bool IsActive);
public record SaveWebsiteDesignSettingRequest(string ThemeName, string PrimaryColor, string FontFamily, string TemplateName, string? CustomCss, bool IsActive);

public record WebsiteNavigationItemResponse(int Id, string Label, string Url, string Placement, int SortOrder, bool IsActive);
public record SaveWebsiteNavigationItemRequest(string Label, string Url, string Placement, int SortOrder, bool IsActive);

public record WebsiteContentItemResponse(int Id, string ContentType, string Title, string? Slug, string? Summary, string Body, string? MediaUrl, string Status, DateTime? PublishedAt);
public record SaveWebsiteContentItemRequest(WebsiteContentType ContentType, string Title, string? Slug, string? Summary, string Body, string? MediaUrl, WebsiteContentStatus Status);
public record UpdateWebsiteContentStatusRequest(WebsiteContentStatus Status, DateTime? PublishedAt);

public record WebsiteFormResponse(int Id, string Title, string FieldsJson, bool IsActive, int SubmissionsCount);
public record SaveWebsiteFormRequest(string Title, string FieldsJson, bool IsActive);

public record WebsiteFormSubmissionResponse(int Id, int WebsiteFormId, string FormTitle, string SubmitterName, string ValuesJson, DateTime SubmittedAt);
public record SaveWebsiteFormSubmissionRequest(int WebsiteFormId, string SubmitterName, string ValuesJson, DateTime? SubmittedAt);

public record WebsiteContactRequestResponse(int Id, string FullName, string? Email, string? Mobile, string Subject, string Message, string Status, DateTime CreatedAt);
public record SaveWebsiteContactRequestRequest(string FullName, string? Email, string? Mobile, string Subject, string Message, WebsiteContentStatus Status);
public record UpdateWebsiteContactStatusRequest(WebsiteContentStatus Status);
