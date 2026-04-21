namespace Mts.Domain;

public class Document
{
    public int Id { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public LifecycleState LifecycleState { get; set; } = LifecycleState.Draft;
    public DateTime CreatedAt { get; set; }

    public ICollection<DocumentRevision> Revisions { get; set; } = new List<DocumentRevision>();
}

public class DocumentRevision
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string RevisionCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string? ChecksumSha256 { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool IsCurrent { get; set; }
    public ReleaseState ReleaseState { get; set; } = ReleaseState.Draft;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Document Document { get; set; } = null!;
    public ICollection<RevisionDocumentLink> RevisionLinks { get; set; } = new List<RevisionDocumentLink>();
}