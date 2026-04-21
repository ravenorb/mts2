using Microsoft.EntityFrameworkCore;
using Mts.Domain;
using Mts.Infrastructure;

namespace MTS.RazorStarter.Services;

public class BomMergeService
{
    private readonly MtsDbContext _db;

    public BomMergeService(MtsDbContext db)
    {
        _db = db;
    }

    public async Task MergeRowsAsync(int parentRevisionId, IReadOnlyList<ExtractedBomRow> extractedRows, BomSourceType source, CancellationToken ct = default)
    {
        var normalizedRows = NormalizeRows(extractedRows);
        if (normalizedRows.Count == 0)
        {
            return;
        }

        var existingRows = await _db.ItemBoms
            .Where(x => x.ParentRevisionId == parentRevisionId)
            .Include(x => x.ChildRevision)
                .ThenInclude(x => x.Item)
            .ToListAsync(ct);

        var existingByFindNo = existingRows
            .Where(x => !string.IsNullOrWhiteSpace(x.FindNo))
            .GroupBy(x => x.FindNo!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var existingByItemNo = existingRows
            .GroupBy(x => x.ChildRevision.Item.ItemNo.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var nextSortOrder = existingRows.Count == 0 ? 1 : existingRows.Max(x => x.SortOrder) + 1;

        foreach (var row in normalizedRows)
        {
            var childRevision = await EnsureRevisionAsync(row.ItemNo, row.Description, ct);

            ItemBom? existing = null;
            if (!string.IsNullOrWhiteSpace(row.FindNo) && existingByFindNo.TryGetValue(row.FindNo, out var byFindNo))
            {
                existing = byFindNo;
            }
            else if (existingByItemNo.TryGetValue(row.ItemNo, out var byItemNo))
            {
                existing = byItemNo;
            }

            if (existing != null)
            {
                if (existing.SourceType == BomSourceType.Manual)
                {
                    continue;
                }

                var canUpdate = source switch
                {
                    BomSourceType.Drawing => existing.SourceType == BomSourceType.Drawing,
                    BomSourceType.Csv => existing.SourceType == BomSourceType.Csv || existing.SourceType == BomSourceType.Drawing,
                    _ => false
                };

                if (!canUpdate)
                {
                    continue;
                }

                existing.Qty = row.Qty;
                existing.ChildRevisionId = childRevision.Id;
                existing.FindNo = string.IsNullOrWhiteSpace(existing.FindNo) ? row.FindNo : existing.FindNo;
                existing.Notes = string.IsNullOrWhiteSpace(existing.Notes) ? row.Notes : existing.Notes;
                existing.SourceType = source;
                continue;
            }

            var newRow = new ItemBom
            {
                ParentRevisionId = parentRevisionId,
                ChildRevisionId = childRevision.Id,
                Qty = row.Qty,
                FindNo = row.FindNo,
                BomRole = BomRole.Component,
                SortOrder = nextSortOrder++,
                Notes = row.Notes,
                SourceType = source
            };

            _db.ItemBoms.Add(newRow);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<ItemRevision> EnsureRevisionAsync(string itemNo, string description, CancellationToken ct)
    {
        var item = await _db.Items
            .Include(x => x.Revisions)
            .FirstOrDefaultAsync(x => x.ItemNo == itemNo, ct);

        if (item == null)
        {
            item = new Item
            {
                ItemNo = itemNo,
                Title = description,
                Description = description,
                ItemType = ItemType.ComponentPart,
                LifecycleState = LifecycleState.Active,
                UnitOfMeasure = "EA",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Items.Add(item);
            await _db.SaveChangesAsync(ct);

            _db.ComponentParts.Add(new ComponentPart
            {
                ItemId = item.Id,
                MakeBuy = MakeBuyType.Make
            });
            await _db.SaveChangesAsync(ct);
        }

        var revision = await _db.ItemRevisions
            .Where(x => x.ItemId == item.Id)
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (revision != null)
        {
            return revision;
        }

        revision = new ItemRevision
        {
            ItemId = item.Id,
            RevisionCode = "A",
            IsCurrent = true,
            ReleaseState = ReleaseState.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        _db.ItemRevisions.Add(revision);
        await _db.SaveChangesAsync(ct);

        return revision;
    }

    private static List<ExtractedBomRow> NormalizeRows(IReadOnlyList<ExtractedBomRow> rows)
    {
        return rows
            .Where(x => !string.IsNullOrWhiteSpace(x.ItemNo))
            .Select(x => new ExtractedBomRow
            {
                FindNo = string.IsNullOrWhiteSpace(x.FindNo) ? null : x.FindNo.Trim(),
                ItemNo = x.ItemNo.Trim(),
                Description = string.IsNullOrWhiteSpace(x.Description) ? x.ItemNo.Trim() : x.Description.Trim(),
                Qty = x.Qty <= 0 ? 1m : x.Qty,
                Notes = string.IsNullOrWhiteSpace(x.Notes) ? null : x.Notes.Trim()
            })
            .GroupBy(x => x.ItemNo, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                first.Qty = group.Sum(x => x.Qty <= 0 ? 1m : x.Qty);
                return first;
            })
            .ToList();
    }
}
