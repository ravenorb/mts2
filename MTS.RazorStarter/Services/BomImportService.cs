using Mts.Domain;

namespace MTS.RazorStarter.Services;

public class BomImportService
{
    private readonly BomMergeService _bomMergeService;

    public BomImportService(BomMergeService bomMergeService)
    {
        _bomMergeService = bomMergeService;
    }

    public async Task MergeCsvAsync(int parentRevisionId, IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("Upload a CSV BOM file first.");
        }

        var rows = await ParseCsvAsync(file, ct);
        await _bomMergeService.MergeRowsAsync(parentRevisionId, rows, BomSourceType.Csv, ct);
    }

    private static async Task<List<ExtractedBomRow>> ParseCsvAsync(IFormFile file, CancellationToken ct)
    {
        var rows = new List<ExtractedBomRow>();

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var lineNo = 0;
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct) ?? string.Empty;
            lineNo++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (lineNo == 1 && line.Contains("item", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var qty = 1m;
            if (parts.Length >= 4 && !decimal.TryParse(parts[3], out qty))
            {
                qty = 1m;
            }

            rows.Add(new ExtractedBomRow
            {
                FindNo = parts[0],
                ItemNo = parts[1],
                Description = parts.Length >= 3 ? parts[2] : parts[1],
                Qty = qty,
                Notes = parts.Length >= 5 ? parts[4] : null
            });
        }

        return rows;
    }
}
