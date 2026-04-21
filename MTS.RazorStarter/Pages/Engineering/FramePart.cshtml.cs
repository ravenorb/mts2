using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartModel : PageModel
{
    private readonly FramePartService _service;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly BomService _bomService;
    private readonly FrameBomExtractor _frameBomExtractor;

    public FramePartModel(
        FramePartService service,
        IWebHostEnvironment hostEnvironment,
        BomService bomService,
        FrameBomExtractor frameBomExtractor)
    {
        _service = service;
        _hostEnvironment = hostEnvironment;
        _bomService = bomService;
        _frameBomExtractor = frameBomExtractor;
    }

    public FramePartViewModel? FramePart { get; private set; }

    [BindProperty]
    public IFormFile? BomUpload { get; set; }

    [BindProperty]
    public BomEditDto EditRow { get; set; } = new();

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

        if (BomUpload == null || BomUpload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Upload a BOM file first.");
            return await OnGetAsync(itemNo, ct);
        }

        var extractedRows = await ParseUploadAsync(BomUpload, ct);
        await _frameBomExtractor.MergeAsync(framePart.CurrentRevisionId, extractedRows, mergeDuplicateItemNos: true, ct);

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

    private async Task<List<ExtractedBomRow>> ParseUploadAsync(IFormFile file, CancellationToken ct)
    {
        var rows = new List<ExtractedBomRow>();

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var lineNo = 0;
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct) ?? string.Empty;
            lineNo++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (lineNo == 1 && line.Contains("item", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var qty = 1m;
            if (parts.Length >= 4 && !decimal.TryParse(parts[3], out qty))
            {
                qty = 1m;
            }

            rows.Add(new ExtractedBomRow
            {
                FindNo = parts[0],
                ItemNo = parts[1],
                Description = parts.Length >= 3 ? parts[2] : parts[1],
                Qty = qty,
                Notes = parts.Length >= 5 ? parts[4] : null
            });
        }

        return rows;
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
