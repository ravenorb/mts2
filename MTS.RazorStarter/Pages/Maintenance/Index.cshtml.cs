using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Pages.Maintenance;

public class IndexModel : PageModel
{
    public IReadOnlyList<MaintenanceTaskCard> Tasks { get; private set; } = [];

    public void OnGet()
    {
        Tasks =
        [
            new() { Title = "Laser chiller daily check", ResponsibleParty = "Operator", Frequency = "Daily", Status = "Open" },
            new() { Title = "Compressor oil inspection", ResponsibleParty = "Maintenance", Frequency = "Weekly", Status = "Due soon" },
            new() { Title = "Robot torch cleaning", ResponsibleParty = "Operator", Frequency = "Per shift", Status = "Open" }
        ];
    }
}
