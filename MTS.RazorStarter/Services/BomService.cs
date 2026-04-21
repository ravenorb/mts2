using Microsoft.EntityFrameworkCore;
using Mts.Domain;
using Mts.Infrastructure;
using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public class BomService
{
    private readonly MtsDbContext _db;

    public BomService(MtsDbContext db)
    {
        _db = db;
    }

    public async Task<List<BomRowVm>> GetBomAsync(int revisionId)
    {
        return await _db.ItemBoms
            .AsNoTracking()
            .Where(x => x.ParentRevisionId == revisionId)
            .Include(x => x.ChildRevision)
                .ThenInclude(x => x.Item)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.FindNo)
            .Select(x => new BomRowVm
            {
                Id = x.Id,
                ParentRevisionId = x.ParentRevisionId,
                ChildRevisionId = x.ChildRevisionId,
                FindNo = x.FindNo,
                ItemNo = x.ChildRevision.Item.ItemNo,
                Title = x.ChildRevision.Item.Title,
                RevisionCode = x.ChildRevision.RevisionCode,
                Qty = x.Qty,
                BomRole = x.BomRole.ToString(),
                Notes = x.Notes,
                SourceType = x.SourceType.ToString()
            })
            .ToListAsync();
    }

    public async Task AddOrUpdateRowAsync(BomEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemNo))
        {
            throw new InvalidOperationException("ItemNo is required.");
        }

        var childRevision = await EnsureRevisionAsync(dto.ItemNo.Trim(), dto.Description?.Trim() ?? dto.ItemNo.Trim());

        ItemBom row;
        if (dto.Id.HasValue)
        {
            row = await _db.ItemBoms.FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                ?? throw new InvalidOperationException($"BOM row {dto.Id.Value} was not found.");
        }
        else
        {
            var nextSort = await _db.ItemBoms
                .Where(x => x.ParentRevisionId == dto.ParentRevisionId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync() ?? 0;

            row = new ItemBom
            {
                ParentRevisionId = dto.ParentRevisionId,
                SortOrder = nextSort + 1
            };
            _db.ItemBoms.Add(row);
        }

        row.ParentRevisionId = dto.ParentRevisionId;
        row.ChildRevisionId = childRevision.Id;
        row.Qty = dto.Qty <= 0 ? 1m : dto.Qty;
        row.FindNo = string.IsNullOrWhiteSpace(dto.FindNo) ? null : dto.FindNo.Trim();
        row.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        row.BomRole = Enum.TryParse<BomRole>(dto.BomRole, true, out var parsedRole)
            ? parsedRole
            : BomRole.Component;
        row.SourceType = BomSourceType.Manual;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRowAsync(int bomId)
    {
        var row = await _db.ItemBoms.FirstOrDefaultAsync(x => x.Id == bomId);
        if (row == null)
        {
            return;
        }

        _db.ItemBoms.Remove(row);
        await _db.SaveChangesAsync();
    }

    private async Task<ItemRevision> EnsureRevisionAsync(string itemNo, string description)
    {
        var item = await _db.Items
            .Include(x => x.Revisions)
            .FirstOrDefaultAsync(x => x.ItemNo == itemNo);

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
            await _db.SaveChangesAsync();

            _db.ComponentParts.Add(new ComponentPart
            {
                ItemId = item.Id,
                MakeBuy = MakeBuyType.Make
            });
            await _db.SaveChangesAsync();
        }

        var revision = await _db.ItemRevisions
            .Where(x => x.ItemId == item.Id)
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

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
        await _db.SaveChangesAsync();
        return revision;
    }
}
