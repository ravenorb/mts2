namespace MTS.RazorStarter.Models;

public class FramePartViewModel
{
    public string ItemNo { get; set; } = "";
    public string Title { get; set; } = "";

    public List<RevisionVM> Revisions { get; set; } = new();

    public class RevisionVM
    {
        public string RevisionCode { get; set; } = "";
        public string? DrawingPath { get; set; }
        public List<CutSheetVM> CutSheets { get; set; } = new();
    }

    public class CutSheetVM
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
    }
}