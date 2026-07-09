using Application.Abstraction;
using Application.Contracts.Emails;
using Application.Contracts.Messaging;
using Application.Service.Emails;
using Application.Service.Messaging;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;

namespace ManagementSystem.Tests;

public class MessagingServiceTests
{
    [Fact]
    public async Task CreateNotificationAsync_InternalChannelMarksRecipientsSentAndReadCanBeTracked()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, recipient) = await SeedUsersAsync(dbcontext);
        var service = CreateService(dbcontext, creator.Id);

        var created = await service.CreateNotificationAsync(new CreateNotificationRequest(
            "تنبيه داخلي",
            "نص التنبيه",
            MessageChannel.Internal,
            [recipient.Id],
            null));

        Assert.True(created.IsSuccess);
        var notificationRecipient = Assert.Single(created.Value.Recipients);
        Assert.Equal("Sent", notificationRecipient.DeliveryStatus);
        Assert.Equal(1, notificationRecipient.DeliveryAttempts);
        Assert.NotNull(notificationRecipient.DeliveredAt);

        var read = await service.MarkNotificationRecipientReadAsync(notificationRecipient.Id);

        Assert.True(read.IsSuccess);
        Assert.True(read.Value.IsRead);
        Assert.NotNull(read.Value.ReadAt);
    }

    [Fact]
    public async Task DeliveryFailureAndRetry_UpdateRecipientAndQueueFollowUpAttempt()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, recipient) = await SeedUsersAsync(dbcontext);
        var service = CreateService(dbcontext, creator.Id);
        var created = await service.CreateNotificationAsync(new CreateNotificationRequest(
            "رسالة بريدية",
            "نص الرسالة",
            MessageChannel.Email,
            [recipient.Id],
            null));
        var notificationRecipient = Assert.Single(created.Value.Recipients);

        var failed = await service.RecordNotificationDeliveryAsync(
            notificationRecipient.Id,
            new UpdateNotificationDeliveryRequest(ChannelDeliveryStatus.Failed, "provider-1", "تعذر الإرسال"));
        var retried = await service.RetryNotificationRecipientAsync(notificationRecipient.Id);

        Assert.True(failed.IsSuccess);
        Assert.Equal("Failed", failed.Value.DeliveryStatus);
        Assert.Equal("تعذر الإرسال", failed.Value.LastDeliveryError);
        Assert.True(retried.IsSuccess);
        Assert.Equal("Pending", retried.Value.DeliveryStatus);
        Assert.Equal(2, retried.Value.DeliveryAttempts);
        Assert.Null(retried.Value.LastDeliveryError);
        Assert.Equal(2, dbcontext.EmailOutbox.Count());
        Assert.True(dbcontext.ChannelDeliveryLogs.Count(x => x.Channel == MessageChannel.Email) >= 3);
    }

    [Fact]
    public async Task CancelNotificationAsync_CancelsPendingRecipients()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, recipient) = await SeedUsersAsync(dbcontext);
        var service = CreateService(dbcontext, creator.Id);
        var created = await service.CreateNotificationAsync(new CreateNotificationRequest(
            "رسالة جوال",
            "نص الرسالة",
            MessageChannel.Sms,
            [recipient.Id],
            null));

        var cancelled = await service.CancelNotificationAsync(created.Value.Id, new CancelNotificationRequest("طلب إداري"));
        var reloaded = (await service.GetNotificationsAsync(NotificationStatus.Cancelled)).Value.Single();

        Assert.True(cancelled.IsSuccess);
        Assert.Equal("Cancelled", reloaded.Status);
        Assert.Equal("Cancelled", reloaded.Recipients.Single().DeliveryStatus);
        Assert.Equal("طلب إداري", reloaded.Recipients.Single().LastDeliveryError);
    }

    [Fact]
    public async Task InternalMail_CanBeReadArchivedAndRestoredByRecipient()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, recipient) = await SeedUsersAsync(dbcontext);
        var senderService = CreateService(dbcontext, creator.Id);
        var recipientService = CreateService(dbcontext, recipient.Id);
        await senderService.CreateMailAsync(new CreateInternalMailRequest("رسالة واردة", "نص الرسالة", [recipient.Id], true));
        var inbox = (await recipientService.GetInboxAsync(recipient.Id)).Value.Single();
        var recipientRow = inbox.Recipients.Single(x => x.RecipientUserId == recipient.Id);

        var read = await recipientService.MarkMailReadAsync(recipientRow.Id);
        var archived = await recipientService.ArchiveMailRecipientAsync(recipientRow.Id, true);
        var inboxAfterArchive = (await recipientService.GetInboxAsync(recipient.Id)).Value.ToList();
        var archivedMessages = (await recipientService.GetArchivedAsync(recipient.Id)).Value.ToList();
        var restored = await recipientService.ArchiveMailRecipientAsync(recipientRow.Id, false);
        var inboxAfterRestore = (await recipientService.GetInboxAsync(recipient.Id)).Value.ToList();

        Assert.True(read.IsSuccess);
        Assert.True(archived.IsSuccess);
        Assert.Empty(inboxAfterArchive);
        Assert.Single(archivedMessages);
        Assert.True(restored.IsSuccess);
        Assert.Single(inboxAfterRestore);
    }

    [Fact]
    public async Task DraftMail_CanBeUpdatedSentAndCancelledBySender()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, recipient) = await SeedUsersAsync(dbcontext);
        var service = CreateService(dbcontext, creator.Id);
        var draft = await service.CreateMailAsync(new CreateInternalMailRequest("مسودة", "نص", [], false));

        var updated = await service.UpdateDraftAsync(draft.Value.Id, new UpdateInternalMailDraftRequest("مسودة معدلة", "نص معدل", [recipient.Id]));
        var sent = await service.SendDraftAsync(draft.Value.Id, new SendDraftRequest([recipient.Id]));
        var sentMessages = (await service.GetSentAsync(creator.Id)).Value.ToList();
        var secondDraft = await service.CreateMailAsync(new CreateInternalMailRequest("مسودة حذف", "نص", [], false));
        var cancelled = await service.CancelDraftAsync(secondDraft.Value.Id);
        var remainingDrafts = (await service.GetDraftsAsync(creator.Id)).Value.ToList();

        Assert.True(updated.IsSuccess);
        Assert.Equal("مسودة معدلة", updated.Value.Subject);
        Assert.True(sent.IsSuccess);
        Assert.Single(sentMessages);
        Assert.True(cancelled.IsSuccess);
        Assert.Empty(remainingDrafts);
    }

    private static MessagingService CreateService(Domain.ApplicationDbcontext dbcontext, string userId) =>
        new(dbcontext, new TestCurrentUserContext(userId), new StubEmailService());

    private static async Task<(ApplicationUser Creator, ApplicationUser Recipient)> SeedUsersAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var creator = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "creator@example.com", Email = "creator@example.com", FullName = "منشئ الإشعار", IsActive = true };
        var recipient = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "recipient@example.com", Email = "recipient@example.com", PhoneNumber = "0500000000", FullName = "مستلم الإشعار", IsActive = true };
        dbcontext.Users.AddRange(creator, recipient);
        await dbcontext.SaveChangesAsync();
        return (creator, recipient);
    }

    private sealed class TestCurrentUserContext(string userId) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = ["Admin"];
    }

    private sealed class StubEmailService : IEmailService
    {
        public Task<Result<IEnumerable<EmailOutboxResponse>>> GetOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success<IEnumerable<EmailOutboxResponse>>([]));

        public Task<Result<SendPendingEmailsResponse>> SendPendingAsync(int maxMessages = 50, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(new SendPendingEmailsResponse(0, 0, 0)));

        public Task<Result<EmailOutboxResponse>> SendOutboxMessageAsync(int outboxMessageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Failure<EmailOutboxResponse>(Application.Abstraction.Errors.EmailErrors.OutboxMessageNotFound));

        public Result<SmtpConfigurationResponse> GetSmtpConfigurationStatus() =>
            Result.Success(new SmtpConfigurationResponse(string.Empty, 0, false, string.Empty, false, false));
    }
}
