namespace MTS.RazorStarter.Models;

public class PalletMoveRequest
{
    public int StationId { get; set; }
    public int PalletId { get; set; }
    public string ToRoute { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
