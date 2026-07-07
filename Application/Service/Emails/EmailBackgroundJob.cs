namespace Application.Service.Emails;

public class EmailBackgroundJob(IEmailService emailService) : IEmailBackgroundJob
{
    public async Task SendPendingEmailsAsync()
    {
        await emailService.SendPendingAsync();
    }
}
