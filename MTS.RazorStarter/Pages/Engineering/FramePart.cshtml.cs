using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartModel : PageModel
{
    private readonly FramePartService _service;

    public FramePartModel(FramePartService service)
    {
        _service = service;
    }

    public PartDetailViewModel? PartDetail { get; private set; }

    public async Task<IActionResult> OnGetAsync(string itemNo, CancellationToken ct)
    {
        var part = await _service.GetAsync(itemNo, ct);
        if (part == null)
        {
            return NotFound();
        }

        var revisions = part.Revisions
            .OrderByDescending(r => r.IsCurrent)
            .ThenByDescending(r => r.CreatedAt)
            .ToList();

        var currentRevision = revisions.FirstOrDefault() ?? new FramePartViewModel.RevisionVM();

        PartDetail = new PartDetailViewModel
        {
            ItemNo = part.ItemNo,
            Title = part.Title,
            CurrentRevision = string.IsNullOrWhiteSpace(currentRevision.RevisionCode) ? "N/A" : currentRevision.RevisionCode,
            Status = currentRevision.ReleaseState,
            CreatedDate = part.CreatedAt,
            LastUpdated = part.UpdatedAt,
            Revisions = revisions.Select(r => r.RevisionCode).ToList(),
            DrawingPath = currentRevision.DrawingPath,
            HasDrawing = !string.IsNullOrWhiteSpace(currentRevision.DrawingPath),
            CutSheets = currentRevision.CutSheets
                .Select(cs => new CutSheetRowViewModel
                {
                    FileName = cs.FileName,
                    Type = cs.Type,
                    ViewPath = cs.FilePath
                })
                .ToList()
        };

        return Page();
    }

    public sealed class PartDetailViewModel
    {
        public string ItemNo { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string CurrentRevision { get; init; } = "N/A";
        public string Status { get; init; } = "Draft";
        public DateTime CreatedDate { get; init; }
        public DateTime LastUpdated { get; init; }
        public IReadOnlyList<string> Revisions { get; init; } = Array.Empty<string>();
        public string? DrawingPath { get; init; }
        public bool HasDrawing { get; init; }
        public IReadOnlyList<CutSheetRowViewModel> CutSheets { get; init; } = Array.Empty<CutSheetRowViewModel>();
    }

    public sealed class CutSheetRowViewModel
    {
        public string FileName { get; init; } = string.Empty;
        public string Type { get; init; } = "Laser";
        public string ViewPath { get; init; } = string.Empty;
    }
}
