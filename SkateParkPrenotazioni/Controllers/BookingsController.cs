using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly BookingCapacityService _capacity;

    public BookingsController(ApplicationDbContext db, BookingCapacityService capacity)
    {
        _db = db;
        _capacity = capacity;
    }

    public async Task<IActionResult> Index()
    {
        var userId = UserId();
        var bookings = await _db.Bookings
            .Include(b => b.Instructor)
            .Include(b => b.LessonDuration)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartAt)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new BookingFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model)
    {
        if (model.LessonType == LessonType.Privata)
        {
            model.Participants = 1;
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        var (ok, error) = await _capacity.ValidateAsync(
            model.InstructorId, model.StartAt, model.Participants, model.LessonType);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(await BuildFormAsync(model));
        }

        var booking = new Booking
        {
            UserId = UserId(),
            InstructorId = model.InstructorId,
            LessonDurationId = model.LessonDurationId,
            LessonType = model.LessonType,
            StartAt = model.StartAt,
            Participants = model.Participants,
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Prenotazione creata correttamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var booking = await FindOwnedAsync(id);
        if (booking is null || booking.IsCancelled)
        {
            return NotFound();
        }

        var model = new BookingFormViewModel
        {
            Id = booking.Id,
            InstructorId = booking.InstructorId,
            LessonDurationId = booking.LessonDurationId,
            LessonType = booking.LessonType,
            StartAt = booking.StartAt,
            Participants = booking.Participants,
            Notes = booking.Notes
        };

        return View(await BuildFormAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var booking = await FindOwnedAsync(id);
        if (booking is null || booking.IsCancelled)
        {
            return NotFound();
        }

        if (model.LessonType == LessonType.Privata)
        {
            model.Participants = 1;
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        var (ok, error) = await _capacity.ValidateAsync(
            model.InstructorId, model.StartAt, model.Participants, model.LessonType, excludeBookingId: id);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(await BuildFormAsync(model));
        }

        booking.InstructorId = model.InstructorId;
        booking.LessonDurationId = model.LessonDurationId;
        booking.LessonType = model.LessonType;
        booking.StartAt = model.StartAt;
        booking.Participants = model.Participants;
        booking.Notes = model.Notes;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Prenotazione aggiornata.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await FindOwnedAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (!booking.IsCancelled)
        {
            booking.IsCancelled = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Prenotazione annullata.";
        }

        return RedirectToAction(nameof(Index));
    }

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Utente non autenticato.");

    private Task<Booking?> FindOwnedAsync(int id) =>
        _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId());

    private async Task<BookingFormViewModel> BuildFormAsync(BookingFormViewModel model)
    {
        model.Instructors = await _db.Instructors
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.Name} (max {i.MaxParticipants})",
                Selected = i.Id == model.InstructorId
            })
            .ToListAsync();

        model.Durations = await _db.LessonDurations
            .Where(d => d.IsActive)
            .OrderBy(d => d.Minutes)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Label,
                Selected = d.Id == model.LessonDurationId
            })
            .ToListAsync();

        return model;
    }
}
