using Microsoft.EntityFrameworkCore;
using Mts.Domain;

namespace Mts.Infrastructure;

public class MtsDbContext : DbContext
{
    public MtsDbContext(DbContextOptions<MtsDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemRevision> ItemRevisions => Set<ItemRevision>();

    public DbSet<FramePart> FrameParts => Set<FramePart>();
    public DbSet<FrameAssembly> FrameAssemblies => Set<FrameAssembly>();
    public DbSet<CutSheet> CutSheets => Set<CutSheet>();
    public DbSet<ComponentPart> ComponentParts => Set<ComponentPart>();

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRevision> DocumentRevisions => Set<DocumentRevision>();
    public DbSet<RevisionDocumentLink> RevisionDocumentLinks => Set<RevisionDocumentLink>();
    public DbSet<FramePartCutSheetLink> FramePartCutSheetLinks => Set<FramePartCutSheetLink>();
    public DbSet<ItemBom> ItemBoms => Set<ItemBom>();
    public DbSet<CutSheetBomLine> CutSheetBomLines => Set<CutSheetBomLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureItem(modelBuilder);
        ConfigureItemRevision(modelBuilder);
        ConfigureSubtypes(modelBuilder);
        ConfigureDocuments(modelBuilder);
        ConfigureLinks(modelBuilder);
        ConfigureBom(modelBuilder);
        ConfigureCutSheetBom(modelBuilder);
    }

    private static void ConfigureItem(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Item>();

        e.ToTable("item");
        e.HasKey(x => x.Id);

        e.Property(x => x.ItemNo).HasColumnName("item_no").IsRequired();
        e.Property(x => x.ItemType).HasColumnName("item_type").HasConversion<string>().IsRequired();
        e.Property(x => x.Title).HasColumnName("title").IsRequired();
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.UnitOfMeasure).HasColumnName("unit_of_measure").IsRequired();
        e.Property(x => x.LifecycleState).HasColumnName("lifecycle_state").HasConversion<string>().IsRequired();
        e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        e.HasIndex(x => x.ItemNo).IsUnique();
    }

    private static void ConfigureItemRevision(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<ItemRevision>();

        e.ToTable("item_revision");
        e.HasKey(x => x.Id);

        e.Property(x => x.ItemId).HasColumnName("item_id");
        e.Property(x => x.RevisionCode).HasColumnName("revision_code").IsRequired();
        e.Property(x => x.RevisionNote).HasColumnName("revision_note");
        e.Property(x => x.IsCurrent).HasColumnName("is_current").IsRequired();
        e.Property(x => x.ReleaseState).HasColumnName("release_state").HasConversion<string>().IsRequired();
        e.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        e.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        e.Property(x => x.CreatedBy).HasColumnName("created_by");
        e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        e.HasIndex(x => new { x.ItemId, x.RevisionCode }).IsUnique();

        e.HasOne(x => x.Item)
            .WithMany(x => x.Revisions)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSubtypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FramePart>(e =>
        {
            e.ToTable("frame_part");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.ItemId).HasColumnName("item_id");
            e.Property(x => x.FamilyCode).HasColumnName("family_code");
            e.Property(x => x.MaterialSpec).HasColumnName("material_spec");
            e.Property(x => x.DefaultFinish).HasColumnName("default_finish");

            e.HasOne(x => x.Item)
                .WithOne(x => x.FramePart)
                .HasForeignKey<FramePart>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FrameAssembly>(e =>
        {
            e.ToTable("frame_assembly");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.ItemId).HasColumnName("item_id");
            e.Property(x => x.FamilyCode).HasColumnName("family_code");
            e.Property(x => x.AssemblyClass).HasColumnName("assembly_class");

            e.HasOne(x => x.Item)
                .WithOne(x => x.FrameAssembly)
                .HasForeignKey<FrameAssembly>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CutSheet>(e =>
        {
            e.ToTable("cut_sheet");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.ItemId).HasColumnName("item_id");
            e.Property(x => x.MachineType).HasColumnName("machine_type");
            e.Property(x => x.MaterialSpec).HasColumnName("material_spec");
            e.Property(x => x.Gauge).HasColumnName("gauge");
            e.Property(x => x.SheetSize).HasColumnName("sheet_size");

            e.HasOne(x => x.Item)
                .WithOne(x => x.CutSheet)
                .HasForeignKey<CutSheet>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComponentPart>(e =>
        {
            e.ToTable("component_part");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.ItemId).HasColumnName("item_id");
            e.Property(x => x.MakeBuy).HasColumnName("make_buy").HasConversion<string>().IsRequired();
            e.Property(x => x.VendorPartNo).HasColumnName("vendor_part_no");

            e.HasOne(x => x.Item)
                .WithOne(x => x.ComponentPart)
                .HasForeignKey<ComponentPart>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(e =>
        {
            e.ToTable("document");
            e.HasKey(x => x.Id);

            e.Property(x => x.DocumentNo).HasColumnName("document_no").IsRequired();
            e.Property(x => x.DocumentType).HasColumnName("document_type").HasConversion<string>().IsRequired();
            e.Property(x => x.Title).HasColumnName("title").IsRequired();
            e.Property(x => x.LifecycleState).HasColumnName("lifecycle_state").HasConversion<string>().IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            e.HasIndex(x => x.DocumentNo).IsUnique();
        });

        modelBuilder.Entity<DocumentRevision>(e =>
        {
            e.ToTable("document_revision");
            e.HasKey(x => x.Id);

            e.Property(x => x.DocumentId).HasColumnName("document_id");
            e.Property(x => x.RevisionCode).HasColumnName("revision_code").IsRequired();
            e.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
            e.Property(x => x.FilePath).HasColumnName("file_path").IsRequired();
            e.Property(x => x.MimeType).HasColumnName("mime_type");
            e.Property(x => x.ChecksumSha256).HasColumnName("checksum_sha256");
            e.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
            e.Property(x => x.IsCurrent).HasColumnName("is_current").IsRequired();
            e.Property(x => x.ReleaseState).HasColumnName("release_state").HasConversion<string>().IsRequired();
            e.Property(x => x.CreatedBy).HasColumnName("created_by");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            e.HasIndex(x => new { x.DocumentId, x.RevisionCode }).IsUnique();

            e.HasOne(x => x.Document)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLinks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RevisionDocumentLink>(e =>
        {
            e.ToTable("revision_document_link");
            e.HasKey(x => x.Id);

            e.Property(x => x.ItemRevisionId).HasColumnName("item_revision_id");
            e.Property(x => x.DocumentRevisionId).HasColumnName("document_revision_id");
            e.Property(x => x.DocumentRole).HasColumnName("document_role").HasConversion<string>().IsRequired();
            e.Property(x => x.IsPrimary).HasColumnName("is_primary").IsRequired();
            e.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

            e.HasIndex(x => new { x.ItemRevisionId, x.DocumentRevisionId, x.DocumentRole }).IsUnique();

            e.HasOne(x => x.ItemRevision)
                .WithMany(x => x.DocumentLinks)
                .HasForeignKey(x => x.ItemRevisionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.DocumentRevision)
                .WithMany(x => x.RevisionLinks)
                .HasForeignKey(x => x.DocumentRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FramePartCutSheetLink>(e =>
        {
            e.ToTable("frame_part_cut_sheet_link");
            e.HasKey(x => x.Id);

            e.Property(x => x.FramePartRevisionId).HasColumnName("frame_part_revision_id");
            e.Property(x => x.CutSheetRevisionId).HasColumnName("cut_sheet_revision_id");
            e.Property(x => x.UsageQty).HasColumnName("usage_qty").HasColumnType("decimal(18,4)");
            e.Property(x => x.LinkType).HasColumnName("link_type").HasConversion<string>().IsRequired();
            e.Property(x => x.IsPrimary).HasColumnName("is_primary").IsRequired();
            e.Property(x => x.Notes).HasColumnName("notes");

            e.HasIndex(x => new { x.FramePartRevisionId, x.CutSheetRevisionId, x.LinkType }).IsUnique();

            e.HasOne(x => x.FramePartRevision)
                .WithMany(x => x.FramePartCutSheetLinksAsFramePart)
                .HasForeignKey(x => x.FramePartRevisionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CutSheetRevision)
                .WithMany(x => x.FramePartCutSheetLinksAsCutSheet)
                .HasForeignKey(x => x.CutSheetRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemBom>(e =>
        {
            e.ToTable("item_bom");
            e.HasKey(x => x.Id);

            e.Property(x => x.ParentRevisionId).HasColumnName("parent_revision_id");
            e.Property(x => x.ChildRevisionId).HasColumnName("child_revision_id");
            e.Property(x => x.Qty).HasColumnName("qty").HasColumnType("decimal(18,4)");
            e.Property(x => x.FindNo).HasColumnName("find_no");
            e.Property(x => x.BomRole).HasColumnName("bom_role").HasConversion<string>().IsRequired();
            e.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.SourceType).HasColumnName("source_type").HasConversion<string>().IsRequired();

            e.HasOne(x => x.ParentRevision)
                .WithMany(x => x.BomChildren)
                .HasForeignKey(x => x.ParentRevisionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ChildRevision)
                .WithMany(x => x.BomParents)
                .HasForeignKey(x => x.ChildRevisionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCutSheetBom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CutSheetBomLine>(e =>
        {
            e.ToTable("cut_sheet_bom_line");
            e.HasKey(x => x.Id);

            e.Property(x => x.CutSheetRevisionId).HasColumnName("cut_sheet_revision_id");
            e.Property(x => x.ComponentItemId).HasColumnName("component_item_id");
            e.Property(x => x.LineNo).HasColumnName("line_no").IsRequired();
            e.Property(x => x.PartNo).HasColumnName("part_no");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Qty).HasColumnName("qty").HasColumnType("decimal(18,4)");
            e.Property(x => x.Notes).HasColumnName("notes");
            e.HasOne(x => x.CutSheetRevision)
                .WithMany(x => x.CutSheetBomLines)
                .HasForeignKey(x => x.CutSheetRevisionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ComponentItem)
                .WithMany(x => x.ReferencedByCutSheetBomLines)
                .HasForeignKey(x => x.ComponentItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
