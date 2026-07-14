using Microsoft.AspNetCore.Authorization;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequirePermission("system.tech-enablement.system_register")]
public class LogsController(IWebHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    [HttpGet("files")]
    public IActionResult Files()
    {
        var directory = GetLogDirectory();
        if (!Directory.Exists(directory))
            return Ok(Array.Empty<object>());

        var files = Directory.GetFiles(directory, "*.log")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new
            {
                file.Name,
                file.Length,
                LastModifiedAt = file.LastWriteTimeUtc
            });

        return Ok(files);
    }

    [HttpGet("latest")]
    public IActionResult Latest([FromQuery] string? fileName, [FromQuery] int? lines)
    {
        var directory = GetLogDirectory();
        if (!Directory.Exists(directory))
            return NotFound(new { Message = "Log directory was not found." });

        var targetFile = ResolveLogFile(directory, fileName);
        if (targetFile is null)
            return NotFound(new { Message = "Log file was not found." });

        var maxLines = configuration.GetValue("LogViewer:MaxLines", 1000);
        var requestedLines = Math.Clamp(lines ?? 300, 1, maxLines);
        var content = System.IO.File.ReadLines(targetFile.FullName).TakeLast(requestedLines);

        return Content(string.Join(Environment.NewLine, content), "text/plain");
    }

    private string GetLogDirectory()
    {
        var configuredDirectory = configuration["LogViewer:Directory"] ?? "Logs";
        return Path.IsPathRooted(configuredDirectory)
            ? configuredDirectory
            : Path.Combine(environment.ContentRootPath, configuredDirectory);
    }

    private static FileInfo? ResolveLogFile(string directory, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || Path.GetFileName(fileName) != fileName)
                return null;

            var explicitFile = new FileInfo(Path.Combine(directory, fileName));
            return explicitFile.Exists && explicitFile.Extension.Equals(".log", StringComparison.OrdinalIgnoreCase)
                ? explicitFile
                : null;
        }

        return Directory.GetFiles(directory, "*.log")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();
    }
}
