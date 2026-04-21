using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Stations;

public class IndexModel : PageModel
{
    private readonly MtsApiClient _mtsApiClient;

    public IndexModel(MtsApiClient mtsApiClient)
    {
        _mtsApiClient = mtsApiClient;
    }

    [BindProperty]
    public MoveForm MoveRequest { get; set; } = new();

    public string? ResultMessage { get; private set; }

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return;

        try
        {
            await _mtsApiClient.MovePallet(
                MoveRequest.PalletId,
                MoveRequest.ToRoute
            );

            ResultMessage = "Move successful";
        }
        catch (Exception ex)
        {
            ResultMessage = $"Error: {ex.Message}";
        }
    }

    public class MoveForm
    {
        [Display(Name = "Station ID")]
        public int StationId { get; set; }

        [Display(Name = "Pallet ID")]
        public int PalletId { get; set; }

        [Display(Name = "Route / destination")]
        public string ToRoute { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}