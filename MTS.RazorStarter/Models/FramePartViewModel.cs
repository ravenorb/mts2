using Mts.Domain;

namespace MTS.RazorStarter.Models;

public class FramePartViewModel
{
    public string ItemNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string LifecycleState { get; set; } = string.Empty;

    public int CurrentRevisionId { get; set; }
    public string CurrentRevisionCode { get; set; } = "N/A";
    public string CurrentRevisionStatus { get; set; } = "Draft";
    public List<string> Revisions { get; set; } = new();

    public string? PrimaryDrawingFileName { get; set; }
    public string? PrimaryDrawingPath { get; set; }
    public bool HasPrimaryDrawing { get; set; }
    public string? DrawingPath { get; set; }
    public string? DrawingFileName { get; set; }
    public bool HasDrawing { get; set; }
    public string? DrawingDownloadUrl { get; set; }
    public string? DrawingOpenUrl { get; set; }

    public List<CutSheetVm> CutSheets { get; set; } = new();
    public List<BomRowVm> BomRows { get; set; } = new();
}

public class CutSheetVm
{
    public string FileName { get; set; } = string.Empty;
    public string Type { get; set; } = "ManufacturedFrom";
    public string? ViewPath { get; set; }
}

public class BomRowVm
{
    public int Id { get; set; }
    public int ParentRevisionId { get; set; }
    public int ChildRevisionId { get; set; }
    public string? FindNo { get; set; }
    public string ItemNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RevisionCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string BomRole { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public BomSourceType SourceType { get; set; } = BomSourceType.Manual;
    public bool HasChildren { get; set; }
    public List<BomRowVm> Children { get; set; } = new();
}
