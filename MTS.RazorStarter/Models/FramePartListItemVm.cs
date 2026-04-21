namespace MTS.RazorStarter.Models;

public class FramePartListItemVm
{
    public string ItemNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LifecycleState { get; set; } = string.Empty;
    public string? CurrentRevision { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateFramePartDto
{
    public string ItemNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "EA";
}
