namespace Mts.Domain;

public class RevisionDocumentLink
{
    public int Id { get; set; }
    public int ItemRevisionId { get; set; }
    public int DocumentRevisionId { get; set; }
    public DocumentRole DocumentRole { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public ItemRevision ItemRevision { get; set; } = null!;
    public DocumentRevision DocumentRevision { get; set; } = null!;
}

public class FramePartCutSheetLink
{
    public int Id { get; set; }
    public int FramePartRevisionId { get; set; }
    public int CutSheetRevisionId { get; set; }
    public decimal? UsageQty { get; set; }
    public CutSheetLinkType LinkType { get; set; } = CutSheetLinkType.ManufacturedFrom;
    public bool IsPrimary { get; set; } = true;
    public string? Notes { get; set; }

    public ItemRevision FramePartRevision { get; set; } = null!;
    public ItemRevision CutSheetRevision { get; set; } = null!;
}

public class ItemBom
{
    public int Id { get; set; }
    public int ParentRevisionId { get; set; }
    public int ChildRevisionId { get; set; }
    public decimal Qty { get; set; }
    public string? FindNo { get; set; }
    public BomRole BomRole { get; set; } = BomRole.Component;
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public BomSourceType SourceType { get; set; } = BomSourceType.Manual;

    public ItemRevision ParentRevision { get; set; } = null!;
    public ItemRevision ChildRevision { get; set; } = null!;
}

public class CutSheetBomLine
{
    public int Id { get; set; }
    public int CutSheetRevisionId { get; set; }
    public int? ComponentItemId { get; set; }
    public int LineNo { get; set; }
    public string? PartNo { get; set; }
    public string? Description { get; set; }
    public decimal Qty { get; set; } = 1m;
    public string? Notes { get; set; }

    public ItemRevision CutSheetRevision { get; set; } = null!;
    public Item? ComponentItem { get; set; }
}