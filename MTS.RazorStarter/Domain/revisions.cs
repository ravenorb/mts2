namespace Mts.Domain;

public class ItemRevision
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string RevisionCode { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
    public bool IsCurrent { get; set; }
    public ReleaseState ReleaseState { get; set; } = ReleaseState.Draft;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Item Item { get; set; } = null!;

    public ICollection<RevisionDocumentLink> DocumentLinks { get; set; } = new List<RevisionDocumentLink>();

    public ICollection<FramePartCutSheetLink> FramePartCutSheetLinksAsFramePart { get; set; } = new List<FramePartCutSheetLink>();
    public ICollection<FramePartCutSheetLink> FramePartCutSheetLinksAsCutSheet { get; set; } = new List<FramePartCutSheetLink>();

    public ICollection<ItemBom> BomChildren { get; set; } = new List<ItemBom>();
    public ICollection<ItemBom> BomParents { get; set; } = new List<ItemBom>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<CutSheetBomLine> CutSheetBomLines { get; set; } = new List<CutSheetBomLine>();
}