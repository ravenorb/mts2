namespace MTS.RazorStarter.Models;

public class BomEditDto
{
    public int? Id { get; set; }
    public int ParentRevisionId { get; set; }
    public string ItemNo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string? Notes { get; set; }
    public string? FindNo { get; set; }
    public string? BomRole { get; set; }
}
