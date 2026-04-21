using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public class NavigationService
{
    public IReadOnlyList<NavItem> GetPrimaryNav() =>
    [
        new() { Title = "Home", Url = "/", Section = "General" },
        new() { Title = "Stations", Url = "/Stations", Section = "Production" },
        new() { Title = "Engineering", Url = "/Engineering", Section = "Engineering" },
        new() { Title = "Maintenance", Url = "/Maintenance", Section = "Maintenance" },
        new() { Title = "Planning", Url = "/Planning", Section = "Planning" }
    ];
}
