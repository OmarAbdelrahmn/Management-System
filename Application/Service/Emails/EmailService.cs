using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Emails;
using Domain;
using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Application.Service.Emails;

public class EmailService(ApplicationDbcontext dbcontext, IOptions<SmtpOptions> options) : IEmailService
{
    private readonly SmtpOptions smtpOptions = options.Value;

    public async Task<Result<IEnumerable<EmailOutboxResponse>>> GetOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EmailOutbox.AsNoTracking();
        if (sent is not null)
            query = query.Where(x => x.Sent == sent);

        var messages = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => MapOutbox(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<EmailOutboxResponse>>(messages);
    }

    public async Task<Result<SendPendingEmailsResponse>> SendPendingAsync(int maxMessages = 50, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
            return Result.Failure<SendPendingEmailsResponse>(EmailErrors.SmtpNotConfigured);

        var limit = Math.Clamp(maxMessages, 1, 200);
        var messages = await dbcontext.EmailOutbox
            .Where(x => !x.Sent)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var sent = 0;
        var failed = 0;
        foreach (var message in messages)
        {
            var wasSent = await TrySendMessageAsync(message, cancellationToken);
            if (wasSent)
                sent++;
            else
                failed++;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new SendPendingEmailsResponse(messages.Count, sent, failed));
    }

    public async Task<Result<EmailOutboxResponse>> SendOutboxMessageAsync(int outboxMessageId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
            return Result.Failure<EmailOutboxResponse>(EmailErrors.SmtpNotConfigured);

        var message = await dbcontext.EmailOutbox.FirstOrDefaultAsync(x => x.Id == outboxMessageId, cancellationToken);
        if (message is null)
            return Result.Failure<EmailOutboxResponse>(EmailErrors.OutboxMessageNotFound);

        await TrySendMessageAsync(message, cancellationToken);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapOutbox(message));
    }

    public Result<SmtpConfigurationResponse> GetSmtpConfigurationStatus() =>
        Result.Success(new SmtpConfigurationResponse(
            smtpOptions.Host,
            smtpOptions.Port,
            smtpOptions.UseStartTls,
            smtpOptions.SenderName,
            !string.IsNullOrWhiteSpace(smtpOptions.Email),
            !string.IsNullOrWhiteSpace(smtpOptions.Password)));

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(smtpOptions.Host) &&
        !string.IsNullOrWhiteSpace(smtpOptions.Email) &&
        !string.IsNullOrWhiteSpace(smtpOptions.Password);

    private async Task<bool> TrySendMessageAsync(EmailOutbox outboxMessage, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpOptions.SenderName, smtpOptions.Email));
            message.To.Add(MailboxAddress.Parse(outboxMessage.ToEmail));
            message.Subject = outboxMessage.Subject;
            message.Body = new TextPart("plain") { Text = outboxMessage.Body };

            using var smtpClient = new SmtpClient();
            var secureSocketOptions = smtpOptions.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await smtpClient.ConnectAsync(smtpOptions.Host, smtpOptions.Port, secureSocketOptions, cancellationToken);
            await smtpClient.AuthenticateAsync(smtpOptions.Email, smtpOptions.Password, cancellationToken);
            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            outboxMessage.Sent = true;
            outboxMessage.SentAt = DateTime.UtcNow.AddHours(3);
            outboxMessage.Error = null;
            return true;
        }
        catch (Exception ex)
        {
            outboxMessage.Sent = false;
            outboxMessage.Error = ex.Message;
            return false;
        }
    }

    private static EmailOutboxResponse MapOutbox(EmailOutbox message) =>
        new(
            message.Id,
            message.ToEmail,
            message.Subject,
            message.Sent,
            message.Error,
            message.CreatedAt,
            message.SentAt);
}
