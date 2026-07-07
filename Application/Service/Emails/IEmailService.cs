using Application.Abstraction;
using Application.Contracts.Emails;

namespace Application.Service.Emails;

public interface IEmailService
{
    Task<Result<IEnumerable<EmailOutboxResponse>>> GetOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default);
    Task<Result<SendPendingEmailsResponse>> SendPendingAsync(int maxMessages = 50, CancellationToken cancellationToken = default);
    Task<Result<EmailOutboxResponse>> SendOutboxMessageAsync(int outboxMessageId, CancellationToken cancellationToken = default);
    Result<SmtpConfigurationResponse> GetSmtpConfigurationStatus();
}
