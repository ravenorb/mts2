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

    public List<FramePartListItemVm> FrameParts { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        FrameParts = await _framePartService.GetAllAsync(ct);
    }
}
