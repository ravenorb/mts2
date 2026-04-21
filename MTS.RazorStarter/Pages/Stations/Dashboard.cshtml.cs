using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Stations;

public class DashboardModel : PageModel
{
    private readonly StationDashboardService _dashboardService;
    private readonly MtsApiClient _mtsApiClient;

    public DashboardModel(
        StationDashboardService dashboardService,
        MtsApiClient mtsApiClient)
    {
        _dashboardService = dashboardService;
        _mtsApiClient = mtsApiClient;
    }

    public StationDashboardViewModel Dashboard { get; private set; } = new();

    [BindProperty]
    public MoveForm MoveRequest { get; set; } = new();

    public string? ResultMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Dashboard = await _dashboardService.GetAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostMoveAsync(CancellationToken cancellationToken)
    {
        Dashboard = await _dashboardService.GetAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _mtsApiClient.MovePallet(MoveRequest.PalletId, MoveRequest.ToRoute);
            ResultMessage = $"Pallet {MoveRequest.PalletId} moved to '{MoveRequest.ToRoute}'.";
        }
        catch (Exception ex)
        {
            ResultMessage = $"Move failed: {ex.Message}";
        }

        Dashboard = await _dashboardService.GetAsync(cancellationToken);
        return Page();
    }

    public class MoveForm
    {
        [Display(Name = "Station ID")]
        public int StationId { get; set; }

        [Display(Name = "Pallet ID")]
        public int PalletId { get; set; }

        [Required]
        [Display(Name = "Route / destination")]
        public string ToRoute { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}