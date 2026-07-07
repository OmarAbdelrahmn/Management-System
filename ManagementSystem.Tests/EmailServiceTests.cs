using Application.Service.Emails;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace ManagementSystem.Tests;

public class EmailServiceTests
{
    [Fact]
    public async Task SendPendingAsync_ReturnsConfigurationErrorWhenEmailOrPasswordMissing()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.EmailOutbox.Add(new EmailOutbox
        {
            ToEmail = "member@example.com",
            Subject = "Meeting",
            Body = "Open the app."
        });
        await dbcontext.SaveChangesAsync();

        var service = new EmailService(dbcontext, Options.Create(new SmtpOptions
        {
            Host = "smtp.gmail.com",
            Port = 587,
            Email = "",
            Password = ""
        }));

        var result = await service.SendPendingAsync();

        Assert.True(result.IsFailure);
        Assert.Equal("Email.SmtpNotConfigured", result.Error.Code);
    }
}
