namespace Application.Service.Attachments;

public sealed class AttachmentOptions
{
    public const long DefaultMaximumSizeBytes = 10 * 1024 * 1024;
    public const int DefaultRetentionDays = 30;

    public long MaximumSizeBytes { get; set; } = DefaultMaximumSizeBytes;
    public int RetentionDays { get; set; } = DefaultRetentionDays;
    public bool RequireMalwareScanner { get; set; } = true;
}
