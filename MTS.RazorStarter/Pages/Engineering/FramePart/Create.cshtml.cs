using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MTS.RazorStarter.Models;
using MTS.RazorStarter.Services;

namespace MTS.RazorStarter.Pages.Engineering.FramePart;

public class CreateModel : PageModel
{
    private readonly FramePartService _framePartService;

    public CreateModel(FramePartService framePartService)
    {
        _framePartService = framePartService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _framePartService.CreateAsync(new CreateFramePartDto
            {
                ItemNo = Input.ItemNo,
                Title = Input.Title,
                UnitOfMeasure = Input.UnitOfMeasure
            }, ct);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        return Redirect($"/Engineering/FramePart/{Input.ItemNo.Trim()}");
    }

    public class InputModel
    {
        [Required]
        public string ItemNo { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string UnitOfMeasure { get; set; } = "EA";
    }
}
