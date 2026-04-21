namespace Mts.Domain;

public class Item
{
    public int Id { get; set; }
    public string ItemNo { get; set; } = string.Empty;
    public ItemType ItemType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = "ea";
    public LifecycleState LifecycleState { get; set; } = LifecycleState.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public FramePart? FramePart { get; set; }
    public FrameAssembly? FrameAssembly { get; set; }
    public CutSheet? CutSheet { get; set; }
    public ComponentPart? ComponentPart { get; set; }

    public ICollection<ItemRevision> Revisions { get; set; } = new List<ItemRevision>();
    public ICollection<CutSheetBomLine> ReferencedByCutSheetBomLines { get; set; } = new List<CutSheetBomLine>();
}

public class FramePart
{
    public int ItemId { get; set; }
    public string? FamilyCode { get; set; }
    public string? MaterialSpec { get; set; }
    public string? DefaultFinish { get; set; }

    public Item Item { get; set; } = null!;
}

public class FrameAssembly
{
    public int ItemId { get; set; }
    public string? FamilyCode { get; set; }
    public string? AssemblyClass { get; set; }

    public Item Item { get; set; } = null!;
}

public class CutSheet
{
    public int ItemId { get; set; }
    public string? MachineType { get; set; }
    public string? MaterialSpec { get; set; }
    public string? Gauge { get; set; }
    public string? SheetSize { get; set; }

    public Item Item { get; set; } = null!;
}

public class ComponentPart
{
    public int ItemId { get; set; }
    public MakeBuyType MakeBuy { get; set; } = MakeBuyType.Make;
    public string? VendorPartNo { get; set; }

    public Item Item { get; set; } = null!;
}