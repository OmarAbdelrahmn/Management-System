using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.PublicRelationsMedia;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.PublicRelationsMedia;

public class PublicRelationsMediaService(ApplicationDbcontext dbcontext) : IPublicRelationsMediaService
{
    public async Task<Result<PublicRelationsMediaDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return Result.Success(new PublicRelationsMediaDashboardResponse(
            await dbcontext.MediaPartners.CountAsync(cancellationToken),
            await dbcontext.MediaEvents.CountAsync(cancellationToken),
            await dbcontext.MediaVisits.CountAsync(cancellationToken),
            await dbcontext.WebsiteUserAccounts.CountAsync(cancellationToken),
            await dbcontext.CommunicationCampaigns.CountAsync(cancellationToken),
            await dbcontext.WebsiteContentItems.CountAsync(x => x.Status == WebsiteContentStatus.Published, cancellationToken),
            await dbcontext.WebsiteContactRequests.CountAsync(cancellationToken)));
    }

    public async Task<Result<IEnumerable<MediaPartnerResponse>>> GetPartnersAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MediaPartners.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<MediaPartnerResponse>>(await query.OrderBy(x => x.Name).Select(x => MapPartner(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<MediaPartnerResponse>> SavePartnerAsync(int? id, SaveMediaPartnerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return Result.Failure<MediaPartnerResponse>(PublicRelationsMediaErrors.InvalidRequest);
        MediaPartner entity = await FindOrCreateAsync(dbcontext.MediaPartners, id, PublicRelationsMediaErrors.PartnerNotFound, cancellationToken) ?? null!;
        if (entity is null) return Result.Failure<MediaPartnerResponse>(PublicRelationsMediaErrors.PartnerNotFound);
        entity.Name = request.Name.Trim(); entity.ContactPerson = request.ContactPerson?.Trim(); entity.Mobile = request.Mobile?.Trim(); entity.Email = request.Email?.Trim(); entity.Status = request.Status; entity.Notes = request.Notes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPartner(entity));
    }

    public async Task<Result<IEnumerable<MediaEventResponse>>> GetEventsAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MediaEvents.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<MediaEventResponse>>(await query.OrderByDescending(x => x.EventDate).Select(x => MapEvent(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<MediaEventResponse>> SaveEventAsync(int? id, SaveMediaEventRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return Result.Failure<MediaEventResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.MediaEvents, id, PublicRelationsMediaErrors.EventNotFound, cancellationToken);
        if (entity is null) return Result.Failure<MediaEventResponse>(PublicRelationsMediaErrors.EventNotFound);
        entity.Title = request.Title.Trim(); entity.EventDate = request.EventDate; entity.Location = request.Location?.Trim(); entity.Description = request.Description?.Trim(); entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEvent(entity));
    }

    public async Task<Result<IEnumerable<MediaVisitResponse>>> GetVisitsAsync(PublicRelationRecordStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MediaVisits.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<MediaVisitResponse>>(await query.OrderByDescending(x => x.VisitDate).Select(x => MapVisit(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<MediaVisitResponse>> SaveVisitAsync(int? id, SaveMediaVisitRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorName)) return Result.Failure<MediaVisitResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.MediaVisits, id, PublicRelationsMediaErrors.VisitNotFound, cancellationToken);
        if (entity is null) return Result.Failure<MediaVisitResponse>(PublicRelationsMediaErrors.VisitNotFound);
        entity.VisitorName = request.VisitorName.Trim(); entity.Organization = request.Organization?.Trim(); entity.VisitDate = request.VisitDate; entity.Purpose = request.Purpose?.Trim(); entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapVisit(entity));
    }

    public async Task<Result<IEnumerable<WebsiteUserAccountResponse>>> GetWebsiteUsersAsync(WebsiteUserStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteUserAccounts.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<WebsiteUserAccountResponse>>(await query.OrderBy(x => x.Username).Select(x => MapWebsiteUser(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteUserAccountResponse>> SaveWebsiteUserAsync(int? id, SaveWebsiteUserAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Username)) return Result.Failure<WebsiteUserAccountResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var username = request.Username.Trim();
        if (await dbcontext.WebsiteUserAccounts.AnyAsync(x => x.Username == username && (!id.HasValue || x.Id != id.Value), cancellationToken)) return Result.Failure<WebsiteUserAccountResponse>(PublicRelationsMediaErrors.DuplicateUsername);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteUserAccounts, id, PublicRelationsMediaErrors.WebsiteUserNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteUserAccountResponse>(PublicRelationsMediaErrors.WebsiteUserNotFound);
        entity.FullName = request.FullName.Trim(); entity.Username = username; entity.Email = request.Email?.Trim(); entity.RoleName = string.IsNullOrWhiteSpace(request.RoleName) ? "WebsiteEditor" : request.RoleName.Trim(); entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapWebsiteUser(entity));
    }

    public async Task<Result<WebsiteUserAccountResponse>> RecordWebsiteLoginAsync(int id, RecordWebsiteLoginRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.WebsiteUserAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteUserAccountResponse>(PublicRelationsMediaErrors.WebsiteUserNotFound);
        entity.LastLoginAt = request.LoginAt ?? DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapWebsiteUser(entity));
    }

    public async Task<Result<IEnumerable<CommunicationTemplateResponse>>> GetTemplatesAsync(CommunicationChannelType? channelType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CommunicationTemplates.AsNoTracking().AsQueryable();
        if (channelType.HasValue) query = query.Where(x => x.ChannelType == channelType.Value);
        return Result.Success<IEnumerable<CommunicationTemplateResponse>>(await query.OrderBy(x => x.Name).Select(x => MapTemplate(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<CommunicationTemplateResponse>> SaveTemplateAsync(int? id, SaveCommunicationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Body)) return Result.Failure<CommunicationTemplateResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.CommunicationTemplates, id, PublicRelationsMediaErrors.TemplateNotFound, cancellationToken);
        if (entity is null) return Result.Failure<CommunicationTemplateResponse>(PublicRelationsMediaErrors.TemplateNotFound);
        entity.ChannelType = request.ChannelType; entity.Name = request.Name.Trim(); entity.Body = request.Body.Trim(); entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapTemplate(entity));
    }

    public async Task<Result<IEnumerable<CommunicationListResponse>>> GetListsAsync(CommunicationChannelType? channelType, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CommunicationLists.AsNoTracking().AsQueryable();
        if (channelType.HasValue) query = query.Where(x => x.ChannelType == channelType.Value);
        return Result.Success<IEnumerable<CommunicationListResponse>>(await query.OrderBy(x => x.Name).Select(x => MapList(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<CommunicationListResponse>> SaveListAsync(int? id, SaveCommunicationListRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.RecipientsCsv)) return Result.Failure<CommunicationListResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.CommunicationLists, id, PublicRelationsMediaErrors.ListNotFound, cancellationToken);
        if (entity is null) return Result.Failure<CommunicationListResponse>(PublicRelationsMediaErrors.ListNotFound);
        entity.ChannelType = request.ChannelType; entity.Name = request.Name.Trim(); entity.RecipientsCsv = request.RecipientsCsv.Trim(); entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapList(entity));
    }

    public async Task<Result<IEnumerable<CommunicationCampaignResponse>>> GetCampaignsAsync(CommunicationChannelType? channelType, CommunicationMessageStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CommunicationCampaigns.AsNoTracking().AsQueryable();
        if (channelType.HasValue) query = query.Where(x => x.ChannelType == channelType.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<CommunicationCampaignResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapCampaign(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<CommunicationCampaignResponse>> SaveCampaignAsync(int? id, SaveCommunicationCampaignRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.MessageBody)) return Result.Failure<CommunicationCampaignResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.CommunicationCampaigns, id, PublicRelationsMediaErrors.CampaignNotFound, cancellationToken);
        if (entity is null) return Result.Failure<CommunicationCampaignResponse>(PublicRelationsMediaErrors.CampaignNotFound);
        entity.ChannelType = request.ChannelType; entity.CommunicationTemplateId = request.CommunicationTemplateId; entity.CommunicationListId = request.CommunicationListId; entity.Title = request.Title.Trim(); entity.MessageBody = request.MessageBody.Trim(); entity.Status = request.Status; entity.SentAt = request.Status == CommunicationMessageStatus.Sent ? DateTime.UtcNow.AddHours(3) : entity.SentAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCampaign(entity));
    }

    public async Task<Result<CommunicationCampaignResponse>> SendCampaignAsync(int id, SendCommunicationCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.CommunicationCampaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure<CommunicationCampaignResponse>(PublicRelationsMediaErrors.CampaignNotFound);
        entity.Status = request.MarkSent ? CommunicationMessageStatus.Sent : CommunicationMessageStatus.Failed;
        entity.SentAt = request.MarkSent ? DateTime.UtcNow.AddHours(3) : null;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCampaign(entity));
    }

    public async Task<Result<IEnumerable<PushSubscriberResponse>>> GetPushSubscribersAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.PushSubscribers.AsNoTracking().AsQueryable();
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<PushSubscriberResponse>>(await query.OrderBy(x => x.DisplayName).Select(x => MapPushSubscriber(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<PushSubscriberResponse>> SavePushSubscriberAsync(int? id, SavePushSubscriberRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Endpoint)) return Result.Failure<PushSubscriberResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.PushSubscribers, id, PublicRelationsMediaErrors.SubscriberNotFound, cancellationToken);
        if (entity is null) return Result.Failure<PushSubscriberResponse>(PublicRelationsMediaErrors.SubscriberNotFound);
        entity.DisplayName = request.DisplayName.Trim(); entity.Endpoint = request.Endpoint.Trim(); entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPushSubscriber(entity));
    }

    public async Task<Result<IEnumerable<WebsiteDesignSettingResponse>>> GetDesignSettingsAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteDesignSettings.AsNoTracking().AsQueryable();
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<WebsiteDesignSettingResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapDesign(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteDesignSettingResponse>> SaveDesignSettingAsync(int? id, SaveWebsiteDesignSettingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeName) || string.IsNullOrWhiteSpace(request.TemplateName)) return Result.Failure<WebsiteDesignSettingResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteDesignSettings, id, PublicRelationsMediaErrors.DesignNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteDesignSettingResponse>(PublicRelationsMediaErrors.DesignNotFound);
        entity.ThemeName = request.ThemeName.Trim(); entity.PrimaryColor = string.IsNullOrWhiteSpace(request.PrimaryColor) ? "#0f766e" : request.PrimaryColor.Trim(); entity.FontFamily = string.IsNullOrWhiteSpace(request.FontFamily) ? "Tajawal" : request.FontFamily.Trim(); entity.TemplateName = request.TemplateName.Trim(); entity.CustomCss = request.CustomCss?.Trim(); entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDesign(entity));
    }

    public async Task<Result<IEnumerable<WebsiteNavigationItemResponse>>> GetNavigationItemsAsync(string? placement, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteNavigationItems.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(placement)) query = query.Where(x => x.Placement == placement.Trim());
        return Result.Success<IEnumerable<WebsiteNavigationItemResponse>>(await query.OrderBy(x => x.Placement).ThenBy(x => x.SortOrder).Select(x => MapNav(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteNavigationItemResponse>> SaveNavigationItemAsync(int? id, SaveWebsiteNavigationItemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Label) || string.IsNullOrWhiteSpace(request.Url)) return Result.Failure<WebsiteNavigationItemResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteNavigationItems, id, PublicRelationsMediaErrors.NavigationNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteNavigationItemResponse>(PublicRelationsMediaErrors.NavigationNotFound);
        entity.Label = request.Label.Trim(); entity.Url = request.Url.Trim(); entity.Placement = string.IsNullOrWhiteSpace(request.Placement) ? "Menu" : request.Placement.Trim(); entity.SortOrder = request.SortOrder; entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapNav(entity));
    }

    public async Task<Result<IEnumerable<WebsiteContentItemResponse>>> GetContentItemsAsync(WebsiteContentType? contentType, WebsiteContentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteContentItems.AsNoTracking().AsQueryable();
        if (contentType.HasValue) query = query.Where(x => x.ContentType == contentType.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<WebsiteContentItemResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapContent(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteContentItemResponse>> SaveContentItemAsync(int? id, SaveWebsiteContentItemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) return Result.Failure<WebsiteContentItemResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteContentItems, id, PublicRelationsMediaErrors.ContentNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteContentItemResponse>(PublicRelationsMediaErrors.ContentNotFound);
        entity.ContentType = request.ContentType; entity.Title = request.Title.Trim(); entity.Slug = request.Slug?.Trim(); entity.Summary = request.Summary?.Trim(); entity.Body = request.Body.Trim(); entity.MediaUrl = request.MediaUrl?.Trim(); entity.Status = request.Status; entity.PublishedAt = request.Status == WebsiteContentStatus.Published ? DateTime.UtcNow.AddHours(3) : entity.PublishedAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapContent(entity));
    }

    public async Task<Result<IEnumerable<WebsiteFormResponse>>> GetFormsAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteForms.AsNoTracking().Include(x => x.Submissions).AsQueryable();
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return Result.Success<IEnumerable<WebsiteFormResponse>>(await query.OrderBy(x => x.Title).Select(x => MapForm(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteFormResponse>> SaveFormAsync(int? id, SaveWebsiteFormRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.FieldsJson)) return Result.Failure<WebsiteFormResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteForms, id, PublicRelationsMediaErrors.FormNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteFormResponse>(PublicRelationsMediaErrors.FormNotFound);
        entity.Title = request.Title.Trim(); entity.FieldsJson = request.FieldsJson.Trim(); entity.IsActive = request.IsActive;
        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(entity).Collection(x => x.Submissions).LoadAsync(cancellationToken);
        return Result.Success(MapForm(entity));
    }

    public async Task<Result<IEnumerable<WebsiteFormSubmissionResponse>>> GetFormSubmissionsAsync(int? formId, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteFormSubmissions.AsNoTracking().Include(x => x.WebsiteForm).AsQueryable();
        if (formId.HasValue) query = query.Where(x => x.WebsiteFormId == formId.Value);
        return Result.Success<IEnumerable<WebsiteFormSubmissionResponse>>(await query.OrderByDescending(x => x.SubmittedAt).Select(x => MapSubmission(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteFormSubmissionResponse>> SaveFormSubmissionAsync(SaveWebsiteFormSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var form = await dbcontext.WebsiteForms.FirstOrDefaultAsync(x => x.Id == request.WebsiteFormId, cancellationToken);
        if (form is null) return Result.Failure<WebsiteFormSubmissionResponse>(PublicRelationsMediaErrors.FormNotFound);
        if (string.IsNullOrWhiteSpace(request.SubmitterName) || string.IsNullOrWhiteSpace(request.ValuesJson)) return Result.Failure<WebsiteFormSubmissionResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = new WebsiteFormSubmission { WebsiteForm = form, WebsiteFormId = form.Id, SubmitterName = request.SubmitterName.Trim(), ValuesJson = request.ValuesJson.Trim(), SubmittedAt = request.SubmittedAt ?? DateTime.UtcNow.AddHours(3) };
        dbcontext.WebsiteFormSubmissions.Add(entity);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSubmission(entity));
    }

    public async Task<Result<IEnumerable<WebsiteContactRequestResponse>>> GetContactRequestsAsync(WebsiteContentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.WebsiteContactRequests.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<WebsiteContactRequestResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapContact(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<WebsiteContactRequestResponse>> SaveContactRequestAsync(int? id, SaveWebsiteContactRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message)) return Result.Failure<WebsiteContactRequestResponse>(PublicRelationsMediaErrors.InvalidRequest);
        var entity = await FindOrCreateAsync(dbcontext.WebsiteContactRequests, id, PublicRelationsMediaErrors.ContactRequestNotFound, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteContactRequestResponse>(PublicRelationsMediaErrors.ContactRequestNotFound);
        entity.FullName = request.FullName.Trim(); entity.Email = request.Email?.Trim(); entity.Mobile = request.Mobile?.Trim(); entity.Subject = request.Subject.Trim(); entity.Message = request.Message.Trim(); entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapContact(entity));
    }

    public async Task<Result<WebsiteContactRequestResponse>> UpdateContactStatusAsync(int id, UpdateWebsiteContactStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.WebsiteContactRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure<WebsiteContactRequestResponse>(PublicRelationsMediaErrors.ContactRequestNotFound);
        entity.Status = request.Status;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapContact(entity));
    }

    private async Task<T?> FindOrCreateAsync<T>(DbSet<T> set, int? id, Error notFound, CancellationToken cancellationToken) where T : class, new()
    {
        if (!id.HasValue)
        {
            var created = new T();
            set.Add(created);
            return created;
        }

        return await set.FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id.Value, cancellationToken);
    }

    private static MediaPartnerResponse MapPartner(MediaPartner x) => new(x.Id, x.Name, x.ContactPerson, x.Mobile, x.Email, x.Status.ToString(), x.Notes);
    private static MediaEventResponse MapEvent(MediaEvent x) => new(x.Id, x.Title, x.EventDate, x.Location, x.Description, x.Status.ToString());
    private static MediaVisitResponse MapVisit(MediaVisit x) => new(x.Id, x.VisitorName, x.Organization, x.VisitDate, x.Purpose, x.Status.ToString());
    private static WebsiteUserAccountResponse MapWebsiteUser(WebsiteUserAccount x) => new(x.Id, x.FullName, x.Username, x.Email, x.RoleName, x.Status.ToString(), x.LastLoginAt);
    private static CommunicationTemplateResponse MapTemplate(CommunicationTemplate x) => new(x.Id, x.ChannelType.ToString(), x.Name, x.Body, x.IsActive);
    private static CommunicationListResponse MapList(CommunicationList x) => new(x.Id, x.ChannelType.ToString(), x.Name, x.RecipientsCsv, x.IsActive);
    private static CommunicationCampaignResponse MapCampaign(CommunicationCampaign x) => new(x.Id, x.ChannelType.ToString(), x.CommunicationTemplateId, x.CommunicationListId, x.Title, x.MessageBody, x.Status.ToString(), x.SentAt);
    private static PushSubscriberResponse MapPushSubscriber(PushSubscriber x) => new(x.Id, x.DisplayName, x.Endpoint, x.IsActive);
    private static WebsiteDesignSettingResponse MapDesign(WebsiteDesignSetting x) => new(x.Id, x.ThemeName, x.PrimaryColor, x.FontFamily, x.TemplateName, x.CustomCss, x.IsActive);
    private static WebsiteNavigationItemResponse MapNav(WebsiteNavigationItem x) => new(x.Id, x.Label, x.Url, x.Placement, x.SortOrder, x.IsActive);
    private static WebsiteContentItemResponse MapContent(WebsiteContentItem x) => new(x.Id, x.ContentType.ToString(), x.Title, x.Slug, x.Summary, x.Body, x.MediaUrl, x.Status.ToString(), x.PublishedAt);
    private static WebsiteFormResponse MapForm(WebsiteForm x) => new(x.Id, x.Title, x.FieldsJson, x.IsActive, x.Submissions.Count);
    private static WebsiteFormSubmissionResponse MapSubmission(WebsiteFormSubmission x) => new(x.Id, x.WebsiteFormId, x.WebsiteForm?.Title ?? string.Empty, x.SubmitterName, x.ValuesJson, x.SubmittedAt);
    private static WebsiteContactRequestResponse MapContact(WebsiteContactRequest x) => new(x.Id, x.FullName, x.Email, x.Mobile, x.Subject, x.Message, x.Status.ToString(), x.CreatedAt);
}
