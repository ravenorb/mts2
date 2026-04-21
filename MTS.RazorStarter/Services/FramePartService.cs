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

        if (item == null) return null;

        return new FramePartViewModel
        {
            ItemNo = item.ItemNo,
            Title = item.Title,

            Revisions = item.Revisions.Select(r => new FramePartViewModel.RevisionVM
            {
                RevisionCode = r.RevisionCode,

                // DRAWING (from document links)
                DrawingPath = r.DocumentLinks
                    .Where(l => l.DocumentRole.ToString() == "Drawing")
                    .Select(l => l.DocumentRevision.FilePath)
                    .FirstOrDefault(),

                // CUT SHEETS (from link table)
                CutSheets = r.FramePartCutSheetLinksAsFramePart
                    .Select(l => new FramePartViewModel.CutSheetVM
                    {
                        FileName = l.CutSheetRevision.Item.ItemNo,
                        FilePath = "" // add file path later if you store it
                    }).ToList()

            }).ToList()
        };
    }
}