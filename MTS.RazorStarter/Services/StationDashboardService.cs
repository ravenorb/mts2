using Microsoft.EntityFrameworkCore;
using Mts.Infrastructure;
using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public class StationDashboardService
{
    private readonly MtsDbContext _db;

    public StationDashboardService(MtsDbContext db)
    {
        _db = db;
    }

    public async Task<StationDashboardViewModel> GetAsync(CancellationToken ct = default)
    {
        var items = await _db.Items.ToListAsync(ct);

        return new StationDashboardViewModel
        {
            Items = items.Select(x => new StationItem
            {
                ItemNo = x.ItemNo,
                Title = x.Title
            }).ToList()
        };
    }
}