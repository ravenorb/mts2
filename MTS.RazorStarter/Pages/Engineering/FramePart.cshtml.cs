using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartModel : PageModel
{
    private const long MaxDrawingFileSizeBytes = 20 * 1024 * 1024;
    private readonly FramePartService _service;
    private readonly FrameDrawingService _frameDrawingService;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly BomService _bomService;
    private readonly FrameBomExtractor _frameBomExtractor;
    private readonly BomImportService _bomImportService;

    public FramePartModel(
        FramePartService service,
        FrameDrawingService frameDrawingService,
        IWebHostEnvironment hostEnvironment,
        BomService bomService,
        FrameBomExtractor frameBomExtractor,
        BomImportService bomImportService)
    {
        _service = service;
        _frameDrawingService = frameDrawingService;
        _hostEnvironment = hostEnvironment;
        _bomService = bomService;
        _frameBomExtractor = frameBomExtractor;
        _bomImportService = bomImportService;
    }

    public FramePartViewModel? FramePart { get; private set; }

    [BindProperty]
    public IFormFile? BomUpload { get; set; }

    [BindProperty]
    public IFormFile? DrawingUpload { get; set; }

    [BindProperty]
    public string? DrawingRevisionCode { get; set; }

    [BindProperty]
    public BomEditDto EditRow { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null)
        {
            return NotFound();
        }

        if (framePart.HasDrawing && !string.IsNullOrWhiteSpace(framePart.DrawingPath))
        {
            var openUrl = BuildOpenDrawingUrl(framePart.DrawingPath);
            framePart.DrawingOpenUrl = openUrl;
            framePart.DrawingDownloadUrl = Url.Page("/Engineering/FramePart", "DrawingDownload", new { itemNo });
        }

        framePart.BomRows = await _bomService.GetBomAsync(framePart.CurrentRevisionId);
        FramePart = framePart;
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateBomAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null)
        {
            return NotFound();
        }

        if (!framePart.HasDrawing || string.IsNullOrWhiteSpace(framePart.DrawingPath))
        {
            ModelState.AddModelError(string.Empty, "Upload a drawing PDF first.");
            return await OnGetAsync(itemNo, ct);
        }

        var extractedRows = await _frameBomExtractor.ExtractFromDrawingAsync(framePart.DrawingPath, ct);
        await _frameBomExtractor.MergeAsync(framePart.CurrentRevisionId, extractedRows, ct);

        return RedirectToPage(new { itemNo });
    }

    public async Task<IActionResult> OnPostImportCsvAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null)
        {
            return NotFound();
        }

        if (BomUpload == null || BomUpload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Upload a CSV BOM file first.");
            return await OnGetAsync(itemNo, ct);
        }

        await _bomImportService.MergeCsvAsync(framePart.CurrentRevisionId, BomUpload, ct);

        return RedirectToPage(new { itemNo });
    }

    public async Task<IActionResult> OnPostSaveBomRowAsync(string itemNo, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(itemNo, ct);
        }

        await _bomService.AddOrUpdateRowAsync(EditRow);
        return RedirectToPage(new { itemNo });
    }

    public async Task<IActionResult> OnPostDeleteBomRowAsync(string itemNo, int bomId)
    {
        await _bomService.DeleteRowAsync(bomId);
        return RedirectToPage(new { itemNo });
    }

    public async Task<IActionResult> OnPostUploadDrawingAsync(string itemNo, CancellationToken ct)
    {
        if (DrawingUpload == null || DrawingUpload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Upload a drawing PDF first.");
            return await OnGetAsync(itemNo, ct);
        }

        if (!string.Equals(Path.GetExtension(DrawingUpload.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Only PDF files are supported.");
            return await OnGetAsync(itemNo, ct);
        }

        if (DrawingUpload.Length > MaxDrawingFileSizeBytes)
        {
            ModelState.AddModelError(string.Empty, "Drawing exceeds 20MB limit.");
            return await OnGetAsync(itemNo, ct);
        }

        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null)
        {
            return NotFound();
        }

        var revisionId = framePart.CurrentRevisionId;
        if (!string.IsNullOrWhiteSpace(DrawingRevisionCode) &&
            !string.Equals(DrawingRevisionCode, framePart.CurrentRevisionCode, StringComparison.OrdinalIgnoreCase))
        {
            var targetRevision = await _service.GetRevisionAsync(itemNo, DrawingRevisionCode, ct);
            if (targetRevision == null)
            {
                ModelState.AddModelError(string.Empty, $"Revision '{DrawingRevisionCode}' was not found for item '{itemNo}'.");
                return await OnGetAsync(itemNo, ct);
            }

            revisionId = targetRevision.Id;
        }

        await _frameDrawingService.UploadAsync(revisionId, DrawingUpload, ct);

        var refreshed = await _service.GetAsync(itemNo, ct);
        if (refreshed?.HasDrawing == true && !string.IsNullOrWhiteSpace(refreshed.DrawingPath))
        {
            var extractedRows = await _frameBomExtractor.ExtractFromDrawingAsync(refreshed.DrawingPath, ct);
            await _frameBomExtractor.MergeAsync(refreshed.CurrentRevisionId, extractedRows, ct);
        }

        return RedirectToPage(new { itemNo });
    }

    public async Task<IActionResult> OnGetDrawingDownloadAsync(string itemNo, CancellationToken ct)
    {
        var framePart = await _service.GetAsync(itemNo, ct);
        if (framePart == null || !framePart.HasDrawing || string.IsNullOrWhiteSpace(framePart.DrawingPath))
        {
            return NotFound();
        }

        var filePath = ResolveFilePath(framePart.DrawingPath);
        if (filePath == null || !System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileName = string.IsNullOrWhiteSpace(framePart.DrawingFileName)
            ? Path.GetFileName(filePath)
            : framePart.DrawingFileName;

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
