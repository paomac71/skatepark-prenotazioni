using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Controllers;

public class InstructorsController(IInstructorService instructorService) : Controller
{
    private readonly IInstructorService _instructorService = instructorService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var instructors = await _instructorService.GetActiveAsync(cancellationToken);
        return View(instructors);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Manage(CancellationToken cancellationToken)
    {
        var instructors = await _instructorService.GetAllForManageAsync(cancellationToken);
        return View(instructors);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _instructorService.GetEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id, InstructorEditViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        model.Availabilities ??= new List<AvailabilityWindowViewModel>();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (ok, error) = await _instructorService.UpdateAsync(model, cancellationToken);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Impossibile salvare.");
            return View(model);
        }

        TempData["Success"] = "Scheda istruttore aggiornata.";
        return RedirectToAction(nameof(Manage));
    }
}
