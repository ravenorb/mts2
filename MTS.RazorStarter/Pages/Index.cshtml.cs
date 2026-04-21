using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages;

public class IndexModel : PageModel
{
    private readonly DashboardService _dashboardService;

    public IndexModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public DashboardViewModel Dashboard { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Dashboard = await _dashboardService.GetDashboardAsync(cancellationToken);
    }
}
