using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class MediaPartnerConfigration : IEntityTypeConfiguration<MediaPartner>
{
    public void Configure(EntityTypeBuilder<MediaPartner> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ContactPerson).HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
    }
}

public class MediaEventConfigration : IEntityTypeConfiguration<MediaEvent>
{
    public void Configure(EntityTypeBuilder<MediaEvent> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Location).HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(2000);
        entity.HasIndex(x => new { x.Status, x.EventDate });
    }
}

public class MediaVisitConfigration : IEntityTypeConfiguration<MediaVisit>
{
    public void Configure(EntityTypeBuilder<MediaVisit> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.VisitorName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Organization).HasMaxLength(200);
        entity.Property(x => x.Purpose).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.VisitDate });
    }
}

public class WebsiteUserAccountConfigration : IEntityTypeConfiguration<WebsiteUserAccount>
{
    public void Configure(EntityTypeBuilder<WebsiteUserAccount> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Username).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.RoleName).IsRequired().HasMaxLength(120);
        entity.HasIndex(x => x.Username).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class CommunicationTemplateConfigration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => new { x.ChannelType, x.IsActive });
    }
}

public class CommunicationListConfigration : IEntityTypeConfiguration<CommunicationList>
{
    public void Configure(EntityTypeBuilder<CommunicationList> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.RecipientsCsv).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => new { x.ChannelType, x.IsActive });
    }
}

public class CommunicationCampaignConfigration : IEntityTypeConfiguration<CommunicationCampaign>
{
    public void Configure(EntityTypeBuilder<CommunicationCampaign> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.MessageBody).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => new { x.ChannelType, x.Status });
        entity.HasOne(x => x.CommunicationTemplate).WithMany().HasForeignKey(x => x.CommunicationTemplateId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.CommunicationList).WithMany().HasForeignKey(x => x.CommunicationListId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CommunicationCampaignRecipientConfigration : IEntityTypeConfiguration<CommunicationCampaignRecipient>
{
    public void Configure(EntityTypeBuilder<CommunicationCampaignRecipient> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Recipient).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Error).HasMaxLength(1000);
        entity.HasIndex(x => new { x.CommunicationCampaignId, x.Status });
        entity.HasIndex(x => x.Recipient);
        entity.HasOne(x => x.CommunicationCampaign).WithMany(x => x.Recipients).HasForeignKey(x => x.CommunicationCampaignId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PushSubscriberConfigration : IEntityTypeConfiguration<PushSubscriber>
{
    public void Configure(EntityTypeBuilder<PushSubscriber> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Endpoint).IsRequired().HasMaxLength(500);
        entity.HasIndex(x => x.IsActive);
    }
}

public class WebsiteDesignSettingConfigration : IEntityTypeConfiguration<WebsiteDesignSetting>
{
    public void Configure(EntityTypeBuilder<WebsiteDesignSetting> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ThemeName).IsRequired().HasMaxLength(120);
        entity.Property(x => x.PrimaryColor).IsRequired().HasMaxLength(30);
        entity.Property(x => x.FontFamily).IsRequired().HasMaxLength(120);
        entity.Property(x => x.TemplateName).IsRequired().HasMaxLength(120);
        entity.Property(x => x.CustomCss).HasMaxLength(4000);
        entity.HasIndex(x => x.IsActive);
    }
}

public class WebsiteNavigationItemConfigration : IEntityTypeConfiguration<WebsiteNavigationItem>
{
    public void Configure(EntityTypeBuilder<WebsiteNavigationItem> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Label).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Url).IsRequired().HasMaxLength(500);
        entity.Property(x => x.Placement).IsRequired().HasMaxLength(80);
        entity.HasIndex(x => new { x.Placement, x.SortOrder });
    }
}

public class WebsiteContentItemConfigration : IEntityTypeConfiguration<WebsiteContentItem>
{
    public void Configure(EntityTypeBuilder<WebsiteContentItem> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Slug).HasMaxLength(200);
        entity.Property(x => x.Summary).HasMaxLength(1000);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(8000);
        entity.Property(x => x.MediaUrl).HasMaxLength(500);
        entity.HasIndex(x => new { x.ContentType, x.Status });
    }
}

public class WebsiteFormConfigration : IEntityTypeConfiguration<WebsiteForm>
{
    public void Configure(EntityTypeBuilder<WebsiteForm> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.FieldsJson).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => x.IsActive);
    }
}

public class WebsiteFormSubmissionConfigration : IEntityTypeConfiguration<WebsiteFormSubmission>
{
    public void Configure(EntityTypeBuilder<WebsiteFormSubmission> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SubmitterName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ValuesJson).IsRequired().HasMaxLength(4000);
        entity.HasOne(x => x.WebsiteForm).WithMany(x => x.Submissions).HasForeignKey(x => x.WebsiteFormId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WebsiteContactRequestConfigration : IEntityTypeConfiguration<WebsiteContactRequest>
{
    public void Configure(EntityTypeBuilder<WebsiteContactRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Message).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => x.Status);
    }
}
