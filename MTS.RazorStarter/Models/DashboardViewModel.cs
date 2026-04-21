namespace MTS.RazorStarter.Models;

public class DashboardViewModel
{
    public List<DashboardItem> Items { get; set; } = new();
}

public class DashboardItem
{
    public string ItemNo { get; set; } = "";
    public string Title { get; set; } = "";
    public List<string> Revisions { get; set; } = new();
}