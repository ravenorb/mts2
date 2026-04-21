namespace MTS.RazorStarter.Models;

public class FramePartViewModel
{
    public string ItemNo { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<RevisionVM> Revisions { get; set; } = new();

    public class RevisionVM
    {
        public string RevisionCode { get; set; } = "";
        public bool IsCurrent { get; set; }
        public string ReleaseState { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public string? DrawingPath { get; set; }
        public List<CutSheetVM> CutSheets { get; set; } = new();
    }

    public class CutSheetVM
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Type { get; set; } = "Laser";
    }
}
