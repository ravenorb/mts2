using Microsoft.EntityFrameworkCore;
using Mts.Domain;
using Mts.Infrastructure;

namespace MTS.RazorStarter.Services;

public class FrameDrawingService
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;
    private readonly MtsDbContext _db;
    private readonly IWebHostEnvironment _hostEnvironment;

    public FrameDrawingService(MtsDbContext db, IWebHostEnvironment hostEnvironment)
    {
        _db = db;
        _hostEnvironment = hostEnvironment;
    }

    public async Task UploadAsync(int itemRevisionId, IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("A PDF file is required.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only PDF files are supported.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File exceeds 20MB limit.");
        }

        var revision = await _db.ItemRevisions
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == itemRevisionId, ct);

        if (revision == null)
        {
            throw new InvalidOperationException("Revision not found.");
        }

        var sanitizedFileName = SanitizeFileName(file.FileName);
        var drawingsRoot = Path.Combine(
            _hostEnvironment.WebRootPath,
            "storage",
            "frame-drawings",
            revision.Item.ItemNo,
            revision.RevisionCode);
        Directory.CreateDirectory(drawingsRoot);

        var fileNameOnly = Path.GetFileNameWithoutExtension(sanitizedFileName);
        var finalFileName = $"{fileNameOnly}.pdf";
        var fullPath = Path.Combine(drawingsRoot, finalFileName);
        var relativePath = Path.Combine("storage", "frame-drawings", revision.Item.ItemNo, revision.RevisionCode, finalFileName)
            .Replace("\\", "/");

        await using (var fileStream = File.Create(fullPath))
        {
            await file.CopyToAsync(fileStream, ct);
        }

        var document = new Document
        {
            DocumentNo = $"FR-{revision.Item.ItemNo}-{revision.RevisionCode}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            DocumentType = DocumentType.Drawing,
            Title = $"{revision.Item.ItemNo} Drawing",
            LifecycleState = LifecycleState.Active,
            CreatedAt = DateTime.UtcNow
        };
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(ct);

        var documentRevision = new DocumentRevision
        {
            DocumentId = document.Id,
            RevisionCode = revision.RevisionCode,
            FileName = finalFileName,
            FilePath = $"/{relativePath}",
            MimeType = "application/pdf",
            FileSizeBytes = file.Length,
            IsCurrent = true,
            ReleaseState = revision.ReleaseState,
            CreatedAt = DateTime.UtcNow
        };
        _db.DocumentRevisions.Add(documentRevision);
        await _db.SaveChangesAsync(ct);

        var existingLinks = await _db.RevisionDocumentLinks
            .Where(x => x.ItemRevisionId == revision.Id && x.DocumentRole == DocumentRole.FramePartDrawing)
            .ToListAsync(ct);

        foreach (var link in existingLinks)
        {
            link.IsPrimary = false;
        }

        var sortOrder = existingLinks.Count == 0 ? 1 : existingLinks.Max(x => x.SortOrder) + 1;
        var revisionLink = new RevisionDocumentLink
        {
            ItemRevisionId = revision.Id,
            DocumentRevisionId = documentRevision.Id,
            DocumentRole = DocumentRole.FramePartDrawing,
            IsPrimary = true,
            SortOrder = sortOrder
        };
        _db.RevisionDocumentLinks.Add(revisionLink);

        await _db.SaveChangesAsync(ct);
    }

    private static string SanitizeFileName(string fileName)
    {
        var input = Path.GetFileName(fileName);
        var sanitized = string.Concat(input.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        return string.IsNullOrWhiteSpace(sanitized) ? "drawing.pdf" : sanitized;
    }
}
