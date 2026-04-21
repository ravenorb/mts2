namespace MTS.RazorStarter.Models;

public class StationDashboardViewModel
{
    public List<StationItem> Items { get; set; } = new();
}

public class StationItem
{
    public string ItemNo { get; set; } = "";
    public string Title { get; set; } = "";
}