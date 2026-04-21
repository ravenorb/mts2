using Microsoft.EntityFrameworkCore;
using Mts.Infrastructure;
using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public class DashboardService
{
    private readonly MtsDbContext _db;

    public DashboardService(MtsDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var items = await _db.Items
            .Include(x => x.Revisions)
            .Take(10)
            .ToListAsync(ct);

        return new DashboardViewModel
        {
            Items = items.Select(x => new DashboardItem
            {
                ItemNo = x.ItemNo,
                Title = x.Title,
                Revisions = x.Revisions.Select(r => r.RevisionCode).ToList()
            }).ToList()
        };
    }
}