using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartModel : PageModel
{
    private readonly FramePartService _service;
    private readonly IWebHostEnvironment _hostEnvironment;

    public FramePartModel(FramePartService service, IWebHostEnvironment hostEnvironment)
    {
        _service = service;
        _hostEnvironment = hostEnvironment;
    }

    public FramePartViewModel? FramePart { get; private set; }

    public async Task<IActionResult> OnGetAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null)
        {
            return NotFound();
        }

        if (framePart.HasPrimaryDrawing && !string.IsNullOrWhiteSpace(framePart.PrimaryDrawingPath))
        {
            var openUrl = BuildOpenDrawingUrl(framePart.PrimaryDrawingPath);
            framePart.DrawingOpenUrl = openUrl;
            framePart.DrawingDownloadUrl = Url.Page("/Engineering/FramePart", "DrawingDownload", new { itemNo });
        }

        FramePart = framePart;
        return Page();
    }

    public async Task<IActionResult> OnGetDrawingDownloadAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null || !framePart.HasPrimaryDrawing || string.IsNullOrWhiteSpace(framePart.PrimaryDrawingPath))
        {
            return NotFound();
        }

        var filePath = ResolveFilePath(framePart.PrimaryDrawingPath);
        if (filePath == null || !System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileName = string.IsNullOrWhiteSpace(framePart.PrimaryDrawingFileName)
            ? Path.GetFileName(filePath)
            : framePart.PrimaryDrawingFileName;

        return PhysicalFile(filePath, "application/pdf", fileName);
    }

    private static string BuildOpenDrawingUrl(string drawingPath)
    {
        if (Uri.TryCreate(drawingPath, UriKind.Absolute, out _))
        {
            return drawingPath;
        }

        var normalized = drawingPath.Replace("\\", "/");
        return normalized.StartsWith('/') ? normalized : $"/{normalized}";
    }

    private string? ResolveFilePath(string drawingPath)
    {
        if (Uri.TryCreate(drawingPath, UriKind.Absolute, out _))
        {
            return null;
        }

        var normalized = drawingPath.Replace("\\", "/").TrimStart('/');

        var webRootCandidate = Path.Combine(_hostEnvironment.WebRootPath ?? string.Empty, normalized.Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(webRootCandidate))
        {
            return webRootCandidate;
        }

        var contentRootCandidate = Path.Combine(_hostEnvironment.ContentRootPath, normalized.Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(contentRootCandidate))
        {
            return contentRootCandidate;
        }

        if (Path.IsPathRooted(drawingPath) && System.IO.File.Exists(drawingPath))
        {
            return drawingPath;
        }

        return null;
    }
}
