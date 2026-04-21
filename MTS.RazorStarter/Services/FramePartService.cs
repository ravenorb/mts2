using Microsoft.EntityFrameworkCore;
using Mts.Domain;
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

    public async Task<List<FramePartListItemVm>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Items
            .AsNoTracking()
            .Where(i => i.ItemType == ItemType.FramePart)
            .Select(i => new FramePartListItemVm
            {
                ItemNo = i.ItemNo,
                Title = i.Title,
                LifecycleState = i.LifecycleState.ToString(),
                CurrentRevision = i.Revisions
                    .Where(r => r.IsCurrent)
                    .Select(r => r.RevisionCode)
                    .FirstOrDefault(),
                UpdatedAt = i.UpdatedAt
            })
            .OrderBy(i => i.ItemNo)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(CreateFramePartDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemNo))
        {
            throw new ArgumentException("Item No is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Title is required.", nameof(dto));
        }

        var normalizedItemNo = dto.ItemNo.Trim();
        var normalizedTitle = dto.Title.Trim();
        var normalizedUom = string.IsNullOrWhiteSpace(dto.UnitOfMeasure) ? "EA" : dto.UnitOfMeasure.Trim().ToUpperInvariant();

        var exists = await _db.Items.AnyAsync(x => x.ItemNo == normalizedItemNo, ct);
        if (exists)
        {
            throw new InvalidOperationException($"Item '{normalizedItemNo}' already exists.");
        }

        var now = DateTime.UtcNow;

        var item = new Item
        {
            ItemNo = normalizedItemNo,
            ItemType = ItemType.FramePart,
            Title = normalizedTitle,
            UnitOfMeasure = normalizedUom,
            LifecycleState = LifecycleState.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync(ct);

        _db.ItemRevisions.Add(new ItemRevision
        {
            ItemId = item.Id,
            RevisionCode = "A",
            IsCurrent = true,
            ReleaseState = ReleaseState.Draft,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<FramePartViewModel?> GetAsync(string itemNo, CancellationToken ct = default)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Revisions)
                .ThenInclude(r => r.DocumentLinks)
                    .ThenInclude(l => l.DocumentRevision)
                        .ThenInclude(dr => dr.Document)
            .Include(i => i.Revisions)
                .ThenInclude(r => r.FramePartCutSheetLinksAsFramePart)
                    .ThenInclude(l => l.CutSheetRevision)
                        .ThenInclude(r => r.Item)
            .Include(i => i.Revisions)
                .ThenInclude(r => r.BomChildren)
                    .ThenInclude(b => b.ChildRevision)
                        .ThenInclude(cr => cr.Item)
            .FirstOrDefaultAsync(x => x.ItemNo == itemNo, ct);

        if (item == null)
        {
            return null;
        }

        var revisions = item.Revisions
            .OrderByDescending(r => r.IsCurrent)
            .ThenByDescending(r => r.CreatedAt)
            .ToList();

        var currentRevision = revisions.FirstOrDefault();
        if (currentRevision == null)
        {
            return new FramePartViewModel
            {
                ItemNo = item.ItemNo,
                Title = item.Title,
                LifecycleState = item.LifecycleState.ToString(),
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        var primaryDrawing = currentRevision.DocumentLinks
            .Where(l => l.DocumentRole == DocumentRole.FramePartDrawing)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.SortOrder)
            .Select(l => l.DocumentRevision)
            .FirstOrDefault();

        var bomRows = await LoadBomTreeAsync(currentRevision.Id, maxDepth: 2, ct);

        return new FramePartViewModel
        {
            ItemNo = item.ItemNo,
            Title = item.Title,
            LifecycleState = item.LifecycleState.ToString(),
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            CurrentRevisionId = currentRevision.Id,
            CurrentRevisionCode = currentRevision.RevisionCode,
            CurrentRevisionStatus = currentRevision.ReleaseState.ToString(),
            Revisions = revisions.Select(r => r.RevisionCode).ToList(),
            PrimaryDrawingFileName = primaryDrawing?.FileName,
            PrimaryDrawingPath = primaryDrawing?.FilePath,
            HasPrimaryDrawing = !string.IsNullOrWhiteSpace(primaryDrawing?.FilePath),
            DrawingFileName = primaryDrawing?.FileName,
            DrawingPath = primaryDrawing?.FilePath,
            HasDrawing = !string.IsNullOrWhiteSpace(primaryDrawing?.FilePath),
            CutSheets = currentRevision.FramePartCutSheetLinksAsFramePart
                .OrderByDescending(l => l.IsPrimary)
                .Select(l => new CutSheetVm
                {
                    FileName = l.CutSheetRevision.Item.ItemNo,
                    Type = l.LinkType.ToString(),
                    ViewPath = null
                })
                .ToList(),
            BomRows = bomRows
        };
    }

    public Task<ItemRevision?> GetRevisionAsync(string itemNo, string revisionCode, CancellationToken ct = default)
    {
        return _db.ItemRevisions
            .AsNoTracking()
            .Include(r => r.Item)
            .FirstOrDefaultAsync(
                r => r.Item.ItemNo == itemNo && r.RevisionCode == revisionCode,
                ct);
    }

    private async Task<List<BomRowVm>> LoadBomTreeAsync(int rootRevisionId, int maxDepth, CancellationToken ct)
    {
        var allBomRows = new List<ItemBom>();
        var visitedParents = new HashSet<int>();
        var frontier = new HashSet<int> { rootRevisionId };

        for (var depth = 0; depth < maxDepth && frontier.Count > 0; depth++)
        {
            var parentIds = frontier.Except(visitedParents).ToList();
            if (parentIds.Count == 0)
            {
                break;
            }

            foreach (var parentId in parentIds)
            {
                visitedParents.Add(parentId);
            }

            var rows = await _db.ItemBoms
                .AsNoTracking()
                .Where(b => parentIds.Contains(b.ParentRevisionId))
                .Include(b => b.ChildRevision)
                    .ThenInclude(r => r.Item)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.FindNo)
                .ToListAsync(ct);

            allBomRows.AddRange(rows);
            frontier = rows.Select(x => x.ChildRevisionId).ToHashSet();
        }

        var byParent = allBomRows.ToLookup(x => x.ParentRevisionId);

        List<BomRowVm> BuildRows(int parentRevisionId, int depth)
        {
            return byParent[parentRevisionId]
                .Select(row => new BomRowVm
                {
                    Id = row.Id,
                    ParentRevisionId = row.ParentRevisionId,
                    ChildRevisionId = row.ChildRevisionId,
                    FindNo = row.FindNo,
                    ItemNo = row.ChildRevision.Item.ItemNo,
                    Title = row.ChildRevision.Item.Title,
                    RevisionCode = row.ChildRevision.RevisionCode,
                    Qty = row.Qty,
                    BomRole = row.BomRole.ToString(),
                    Notes = row.Notes,
                    SourceType = row.SourceType,
                    HasChildren = byParent.Contains(row.ChildRevisionId),
                    Children = depth + 1 < maxDepth ? BuildRows(row.ChildRevisionId, depth + 1) : new List<BomRowVm>()
                })
                .ToList();
        }

        return BuildRows(rootRevisionId, 0);
    }
}
