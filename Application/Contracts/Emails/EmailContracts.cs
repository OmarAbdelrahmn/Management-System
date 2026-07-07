namespace Application.Contracts.Emails;

public record EmailOutboxResponse(
    int Id,
    string ToEmail,
    string Subject,
    bool Sent,
    string? Error,
    DateTime CreatedAt,
    DateTime? SentAt);

public record SendPendingEmailsResponse(int Attempted, int Sent, int Failed);

public record SmtpConfigurationResponse(
    string Host,
    int Port,
    bool UseStartTls,
    string SenderName,
    bool EmailConfigured,
    bool PasswordConfigured);
