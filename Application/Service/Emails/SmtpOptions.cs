namespace Application.Service.Emails;

public class SmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string SenderName { get; set; } = "Management System";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
