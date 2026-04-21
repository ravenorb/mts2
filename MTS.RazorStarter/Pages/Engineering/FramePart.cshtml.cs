using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering;

public class FramePartModel : PageModel
{
    private readonly FramePartService _service;

    public FramePartModel(FramePartService service)
    {
        _service = service;
    }

    public FramePartViewModel? Part { get; set; }

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken ct)
    {
        Part = await _service.GetAsync(id, ct);

        if (Part == null)
            return NotFound();

        return Page();
    }
}