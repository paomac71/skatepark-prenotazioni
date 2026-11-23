using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Services;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Controllers;

[Authorize]
public class BookingsController(IBookingService bookingService) : Controller
{
    private readonly IBookingService _bookingService = bookingService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetUserBookingsAsync(UserId(), cancellationToken);
        return View(bookings);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _bookingService.PrepareFormAsync(new BookingFormViewModel(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model, CancellationToken cancellationToken)
    {
        NormalizePrivateLesson(model);

        if (!ModelState.IsValid)
        {
            return View(await _bookingService.PrepareFormAsync(model, cancellationToken));
        }

        var (ok, error) = await _bookingService.CreateAsync(UserId(), model, cancellationToken);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(await _bookingService.PrepareFormAsync(model, cancellationToken));
        }

        TempData["Success"] = "Prenotazione creata correttamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _bookingService.GetEditFormAsync(UserId(), id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        NormalizePrivateLesson(model);

        if (!ModelState.IsValid)
        {
            return View(await _bookingService.PrepareFormAsync(model, cancellationToken));
        }

        var (ok, error, notFound) = await _bookingService.UpdateAsync(UserId(), id, model, cancellationToken);
        if (notFound)
        {
            return NotFound();
        }

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(await _bookingService.PrepareFormAsync(model, cancellationToken));
        }

        TempData["Success"] = "Prenotazione aggiornata.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var (ok, notFound) = await _bookingService.CancelAsync(UserId(), id, cancellationToken);
        if (notFound || !ok)
        {
            return NotFound();
        }

        TempData["Success"] = "Prenotazione annullata.";
        return RedirectToAction(nameof(Index));
    }

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Utente non autenticato.");

    private void NormalizePrivateLesson(BookingFormViewModel model)
    {
        if (model.LessonType == Models.LessonType.Privata)
        {
            model.Participants = 1;
            ModelState.Remove(nameof(model.Participants));
        }
    }
}
