using Domain.Auditing;

namespace Domain.Entities;

public enum PublicRelationRecordStatus
{
    Planned = 0,
    Active = 1,
    Completed = 2,
    Archived = 3
}

public enum WebsiteUserStatus
{
    Active = 0,
    Suspended = 1,
    Archived = 2
}

public enum CommunicationChannelType
{
    Sms = 0,
    WhatsApp = 1,
    Email = 2,
    Push = 3
}

public enum CommunicationMessageStatus
{
    Draft = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public enum WebsiteContentType
{
    About = 0,
    News = 1,
    Gallery = 2,
    Video = 3,
    Partner = 4,
    Regulation = 5,
    AnnualReport = 6,
    Initiative = 7,
    Testimonial = 8,
    Service = 9,
    Link = 10,
    Popup = 11,
    Contest = 12,
    Page = 13
}

public enum WebsiteContentStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public class MediaPartner : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public PublicRelationRecordStatus Status { get; set; } = PublicRelationRecordStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class MediaEvent : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public PublicRelationRecordStatus Status { get; set; } = PublicRelationRecordStatus.Planned;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class MediaVisit : IAuditable
{
    public int Id { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public DateTime VisitDate { get; set; }
    public string? Purpose { get; set; }
    public PublicRelationRecordStatus Status { get; set; } = PublicRelationRecordStatus.Planned;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class WebsiteUserAccount : IAuditable
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string RoleName { get; set; } = "WebsiteEditor";
    public WebsiteUserStatus Status { get; set; } = WebsiteUserStatus.Active;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class CommunicationTemplate : IAuditable
{
    public int Id { get; set; }
    public CommunicationChannelType ChannelType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class CommunicationList : IAuditable
{
    public int Id { get; set; }
    public CommunicationChannelType ChannelType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RecipientsCsv { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class CommunicationCampaign : IAuditable
{
    public int Id { get; set; }
    public CommunicationChannelType ChannelType { get; set; }
    public int? CommunicationTemplateId { get; set; }
    public int? CommunicationListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public CommunicationMessageStatus Status { get; set; } = CommunicationMessageStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public CommunicationTemplate? CommunicationTemplate { get; set; }
    public CommunicationList? CommunicationList { get; set; }
    public ICollection<CommunicationCampaignRecipient> Recipients { get; set; } = new List<CommunicationCampaignRecipient>();
}

public class CommunicationCampaignRecipient : IAuditable
{
    public int Id { get; set; }
    public int CommunicationCampaignId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public CommunicationMessageStatus Status { get; set; } = CommunicationMessageStatus.Draft;
    public DateTime? AttemptedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public CommunicationCampaign? CommunicationCampaign { get; set; }
}

public class PushSubscriber : IAuditable
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class WebsiteDesignSetting : IAuditable
{
    public int Id { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#0f766e";
    public string FontFamily { get; set; } = "Tajawal";
    public string TemplateName { get; set; } = string.Empty;
    public string? CustomCss { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class WebsiteNavigationItem : IAuditable
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Placement { get; set; } = "Menu";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class WebsiteContentItem : IAuditable
{
    public int Id { get; set; }
    public WebsiteContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public WebsiteContentStatus Status { get; set; } = WebsiteContentStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class WebsiteForm : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FieldsJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<WebsiteFormSubmission> Submissions { get; set; } = new List<WebsiteFormSubmission>();
}

public class WebsiteFormSubmission : IAuditable
{
    public int Id { get; set; }
    public int WebsiteFormId { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public string ValuesJson { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public WebsiteForm? WebsiteForm { get; set; }
}

public class WebsiteContactRequest : IAuditable
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public WebsiteContentStatus Status { get; set; } = WebsiteContentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
