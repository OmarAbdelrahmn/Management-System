namespace Application.Service.Emails;

public interface IEmailBackgroundJob
{
    Task SendPendingEmailsAsync();
}
