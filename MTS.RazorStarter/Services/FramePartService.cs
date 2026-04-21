using Microsoft.EntityFrameworkCore;
using Mts.Infrastructure;
using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public class FramePartService
{
    private readonly MtsDbContext _db;

    public FramePartService(MtsDbContext db)
    {
        _db = db;
    }

    public async Task<FramePartViewModel?> GetAsync(string itemNo, CancellationToken ct = default)
    {
        var item = await _db.Items
            .Include(i => i.Revisions)
                .ThenInclude(r => r.DocumentLinks)
                    .ThenInclude(l => l.DocumentRevision)
                        .ThenInclude(dr => dr.Document)
            .Include(i => i.Revisions)
                .ThenInclude(r => r.FramePartCutSheetLinksAsFramePart)
                    .ThenInclude(l => l.CutSheetRevision)
                        .ThenInclude(r => r.Item)
            .FirstOrDefaultAsync(x => x.ItemNo == itemNo, ct);

        if (item == null)
        {
            return null;
        }

        return new FramePartViewModel
        {
            ItemNo = item.ItemNo,
            Title = item.Title,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            Revisions = item.Revisions
                .OrderByDescending(r => r.IsCurrent)
                .ThenByDescending(r => r.CreatedAt)
                .Select(r => new FramePartViewModel.RevisionVM
                {
                    RevisionCode = r.RevisionCode,
                    IsCurrent = r.IsCurrent,
                    ReleaseState = r.ReleaseState.ToString(),
                    CreatedAt = r.CreatedAt,
                    DrawingPath = r.DocumentLinks
                        .Where(l => l.DocumentRole.ToString().Contains("Drawing"))
                        .OrderByDescending(l => l.IsPrimary)
                        .ThenBy(l => l.SortOrder)
                        .Select(l => l.DocumentRevision.FilePath)
                        .FirstOrDefault(),
                    CutSheets = r.FramePartCutSheetLinksAsFramePart
                        .Select(l => new FramePartViewModel.CutSheetVM
                        {
                            FileName = l.CutSheetRevision.Item.ItemNo,
                            FilePath = string.Empty,
                            Type = "Laser"
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
