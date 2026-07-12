using Application.Contracts.PublicRelationsMedia;
using Application.Service.PublicRelationsMedia;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class PublicRelationsMediaServiceTests
{
    [Fact]
    public async Task RelationsAndWebsiteUsers_SaveCoreRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new PublicRelationsMediaService(dbcontext);

        var partner = await service.SavePartnerAsync(null, new SaveMediaPartnerRequest("صحيفة محلية", "سعيد", "0500000000", null, PublicRelationRecordStatus.Active, null));
        var mediaEvent = await service.SaveEventAsync(null, new SaveMediaEventRequest("لقاء إعلامي", new DateTime(2026, 7, 8), "المقر", null, PublicRelationRecordStatus.Planned));
        var visit = await service.SaveVisitAsync(null, new SaveMediaVisitRequest("زائر إعلامي", "قناة", new DateTime(2026, 7, 9), "تغطية", PublicRelationRecordStatus.Completed));
        var user = await service.SaveWebsiteUserAsync(null, new SaveWebsiteUserAccountRequest("محرر الموقع", "editor", "editor@example.com", "Editor", WebsiteUserStatus.Active));
        var login = await service.RecordWebsiteLoginAsync(user.Value.Id, new RecordWebsiteLoginRequest(new DateTime(2026, 7, 10)));

        Assert.True(partner.IsSuccess);
        Assert.True(mediaEvent.IsSuccess);
        Assert.True(visit.IsSuccess);
        Assert.True(login.IsSuccess);
        Assert.Equal(new DateTime(2026, 7, 10), login.Value.LastLoginAt);
    }

    [Fact]
    public async Task Channels_SaveTemplateListCampaignAndSend()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new PublicRelationsMediaService(dbcontext);

        var template = await service.SaveTemplateAsync(null, new SaveCommunicationTemplateRequest(CommunicationChannelType.Sms, "قالب", "رسالة", true));
        var list = await service.SaveListAsync(null, new SaveCommunicationListRequest(CommunicationChannelType.Sms, "قائمة", "0500000000,0500000001,0500000000", true));
        var campaign = await service.SaveCampaignAsync(null, new SaveCommunicationCampaignRequest(CommunicationChannelType.Sms, template.Value.Id, list.Value.Id, "حملة", "رسالة", CommunicationMessageStatus.Draft));
        var sent = await service.SendCampaignAsync(campaign.Value.Id, new SendCommunicationCampaignRequest(true));
        var resend = await service.SendCampaignAsync(campaign.Value.Id, new SendCommunicationCampaignRequest(true));
        var failedCampaign = await service.SaveCampaignAsync(null, new SaveCommunicationCampaignRequest(CommunicationChannelType.Sms, template.Value.Id, list.Value.Id, "حملة فاشلة", "رسالة", CommunicationMessageStatus.Draft));
        var failed = await service.SendCampaignAsync(failedCampaign.Value.Id, new SendCommunicationCampaignRequest(false));
        var emptyList = await service.SaveListAsync(null, new SaveCommunicationListRequest(CommunicationChannelType.Sms, "قائمة فارغة", " , ", true));
        var emptyCampaign = await service.SaveCampaignAsync(null, new SaveCommunicationCampaignRequest(CommunicationChannelType.Sms, template.Value.Id, emptyList.Value.Id, "حملة بلا مستلمين", "رسالة", CommunicationMessageStatus.Draft));
        var emptySend = await service.SendCampaignAsync(emptyCampaign.Value.Id, new SendCommunicationCampaignRequest(true));

        Assert.True(sent.IsSuccess);
        Assert.Equal("Sent", sent.Value.Status);
        Assert.NotNull(sent.Value.SentAt);
        Assert.Equal(2, sent.Value.RecipientsCount);
        Assert.Equal(2, sent.Value.DeliveredCount);
        Assert.All(sent.Value.Recipients, x => Assert.Equal("Sent", x.Status));
        Assert.True(resend.IsFailure);
        Assert.True(failed.IsSuccess);
        Assert.Equal(2, failed.Value.FailedCount);
        Assert.All(failed.Value.Recipients, x => Assert.NotNull(x.Error));
        Assert.True(emptySend.IsFailure);
    }

    [Fact]
    public async Task WebsiteDesignContentFormsAndContacts_SaveRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new PublicRelationsMediaService(dbcontext);

        var design = await service.SaveDesignSettingAsync(null, new SaveWebsiteDesignSettingRequest("الثيم", "#0f766e", "Tajawal", "Default", null, true));
        var nav = await service.SaveNavigationItemAsync(null, new SaveWebsiteNavigationItemRequest("الرئيسية", "/", "Menu", 1, true));
        var content = await service.SaveContentItemAsync(null, new SaveWebsiteContentItemRequest(WebsiteContentType.News, "خبر", "news", "ملخص", "النص", null, WebsiteContentStatus.Published));
        var form = await service.SaveFormAsync(null, new SaveWebsiteFormRequest("نموذج", "[{\"name\":\"fullName\"}]", true));
        var submission = await service.SaveFormSubmissionAsync(new SaveWebsiteFormSubmissionRequest(form.Value.Id, "مستخدم", "{\"fullName\":\"مستخدم\"}", new DateTime(2026, 7, 8)));
        var contact = await service.SaveContactRequestAsync(null, new SaveWebsiteContactRequestRequest("زائر", null, null, "استفسار", "رسالة", WebsiteContentStatus.Draft));

        Assert.True(design.IsSuccess);
        Assert.True(nav.IsSuccess);
        Assert.Equal("Published", content.Value.Status);
        Assert.True(submission.IsSuccess);
        Assert.True(contact.IsSuccess);
    }

    [Fact]
    public async Task UpdateContentStatusAsync_PublishesWithSlugAndArchives()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new PublicRelationsMediaService(dbcontext);

        var missingSlug = await service.SaveContentItemAsync(null, new SaveWebsiteContentItemRequest(WebsiteContentType.News, "خبر بلا رابط", null, null, "النص", null, WebsiteContentStatus.Draft));
        var invalidPublish = await service.UpdateContentStatusAsync(missingSlug.Value.Id, new UpdateWebsiteContentStatusRequest(WebsiteContentStatus.Published, new DateTime(2026, 7, 8)));
        var content = await service.SaveContentItemAsync(missingSlug.Value.Id, new SaveWebsiteContentItemRequest(WebsiteContentType.News, "خبر", "news-slug", null, "النص", null, WebsiteContentStatus.Draft));
        var published = await service.UpdateContentStatusAsync(content.Value.Id, new UpdateWebsiteContentStatusRequest(WebsiteContentStatus.Published, new DateTime(2026, 7, 8)));
        var archived = await service.UpdateContentStatusAsync(content.Value.Id, new UpdateWebsiteContentStatusRequest(WebsiteContentStatus.Archived, null));
        var dashboard = await service.GetDashboardAsync();

        Assert.False(invalidPublish.IsSuccess);
        Assert.Equal("Published", published.Value.Status);
        Assert.Equal(new DateTime(2026, 7, 8), published.Value.PublishedAt);
        Assert.Equal("Archived", archived.Value.Status);
        Assert.Equal(0, dashboard.Value.PublishedContentCount);
    }
}
