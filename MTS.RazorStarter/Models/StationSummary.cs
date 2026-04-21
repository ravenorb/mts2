namespace MTS.RazorStarter.Models;

public class StationSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ActivePallets { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
}
