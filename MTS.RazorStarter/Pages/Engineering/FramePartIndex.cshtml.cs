using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartIndexModel : PageModel
{
    private readonly FramePartService _framePartService;

    public FramePartIndexModel(FramePartService framePartService)
    {
        _framePartService = framePartService;
    }

    private const int DefaultPageSize = 25;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? State { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Dir { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public int PageSize { get; } = DefaultPageSize;
    public int TotalCount { get; private set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public List<FramePartListItemVm> FrameParts { get; private set; } = new();

    public string ToggleDir(string column)
    {
        var isCurrentColumn = string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase);
        var isAsc = string.Equals(Dir, "asc", StringComparison.OrdinalIgnoreCase);
        return isCurrentColumn && isAsc ? "desc" : "asc";
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (Page < 1)
        {
            Page = 1;
        }

        (FrameParts, TotalCount) = await _framePartService.GetAllAsync(
            Search,
            State,
            Sort,
            Dir,
            Page,
            PageSize,
            ct);

        if (Page > TotalPages)
        {
            Page = TotalPages;
            (FrameParts, TotalCount) = await _framePartService.GetAllAsync(
                Search,
                State,
                Sort,
                Dir,
                Page,
                PageSize,
                ct);
        }
    }
}
