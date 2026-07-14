using Application.Service.Attachments;

namespace Express_Service.Services;

public sealed class PrivateAttachmentStorage(IWebHostEnvironment environment) : IAttachmentStorage
{
    private string Root => Path.Combine(environment.ContentRootPath, "App_Data", "attachments");
    private string TemporaryRoot => Path.Combine(Root, "temporary");

    public async Task<string> SaveTemporaryAsync(Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(TemporaryRoot);
        var path = Path.Combine(TemporaryRoot, $"{Guid.NewGuid():N}.upload");
        await using var output = File.Create(path);
        await content.CopyToAsync(output, cancellationToken);
        return path;
    }

    public Task PromoteAsync(string temporaryPath, string permanentPath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Root);
        var destination = Resolve(permanentPath);
        File.Move(Resolve(temporaryPath), destination, true);
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var path = Resolve(storagePath);
        Stream? stream = File.Exists(path) ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var path = Resolve(storagePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string path)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(Root, Path.GetFileName(path)));
        var allowedRoot = Path.GetFullPath(Root);
        if (!full.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid attachment storage path.");
        return full;
    }
}
