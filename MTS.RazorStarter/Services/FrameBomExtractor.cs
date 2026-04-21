using Mts.Domain;

namespace MTS.RazorStarter.Services;

public class FrameBomExtractor
{
    private readonly BomMergeService _bomMergeService;

    public FrameBomExtractor(BomMergeService bomMergeService)
    {
        _bomMergeService = bomMergeService;
    }

    public async Task MergeAsync(int parentRevisionId, IReadOnlyList<ExtractedBomRow> extractedRows, CancellationToken ct = default)
    {
        await _bomMergeService.MergeRowsAsync(parentRevisionId, extractedRows, BomSourceType.Drawing, ct);
    }

    public Task<List<ExtractedBomRow>> ExtractFromDrawingAsync(string drawingPath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Phase 1 stub: PDF parser integration (PdfPig) will be added in Phase 2.
        return Task.FromResult(new List<ExtractedBomRow>());
    }
}

public class ExtractedBomRow
{
    public string? FindNo { get; set; }
    public string ItemNo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string? Notes { get; set; }
}
