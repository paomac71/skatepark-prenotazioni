using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Services;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Controllers;

[Authorize]
public class BookingsController(
    IBookingService bookingService,
    IInstructorService instructorService,
    IAvailabilityService availabilityService,
    IAppClock clock) : Controller
{
    private readonly IBookingService _bookingService = bookingService;
    private readonly IInstructorService _instructorService = instructorService;
    private readonly IAvailabilityService _availabilityService = availabilityService;
    private readonly IAppClock _clock = clock;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetUserBookingsAsync(UserId(), cancellationToken);
        return View(bookings);
    }

    public async Task<IActionResult> Create(int? instructorId, CancellationToken cancellationToken)
    {
        if (instructorId is null or <= 0)
        {
            var instructors = await _instructorService.GetActiveAsync(cancellationToken);
            return View("SelectInstructor", instructors);
        }

        var instructor = await _instructorService.GetActiveByIdAsync(instructorId.Value, cancellationToken);
        if (instructor is null)
        {
            return NotFound();
        }

        var form = await _bookingService.PrepareFormAsync(
            new BookingFormViewModel { InstructorId = instructor.Id },
            cancellationToken);

        var today = _clock.Today;
        var model = new BookingCalendarViewModel
        {
            InstructorId = instructor.Id,
            InstructorName = instructor.Name,
            MaxParticipants = instructor.MaxParticipants,
            Form = form,
            Year = today.Year,
            Month = today.Month
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Form")] BookingFormViewModel form,
        CancellationToken cancellationToken)
    {
        NormalizePrivateLesson(form);

        if (!ModelState.IsValid)
        {
            return await RedisplayCreateAsync(form, cancellationToken);
        }

        // Se esiste già una prenotazione lo stesso giorno, CreateAsync la aggiorna.
        var existingBefore = form.StartAt is null
            ? null
            : await _bookingService.GetUserActiveBookingOnDayAsync(
                UserId(),
                form.InstructorId,
                DateOnly.FromDateTime(form.StartAt.Value),
                cancellationToken);

        var (ok, error) = await _bookingService.CreateAsync(UserId(), form, cancellationToken);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return await RedisplayCreateAsync(form, cancellationToken);
        }

        TempData["Success"] = existingBefore is null
            ? "Prenotazione creata correttamente."
            : "Prenotazione esistente aggiornata (stesso giorno).";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _bookingService.GetEditFormAsync(UserId(), id, cancellationToken);
        if (form is null)
        {
            return NotFound();
        }

        var instructor = await _instructorService.GetActiveByIdAsync(form.InstructorId, cancellationToken);
        if (instructor is null)
        {
            // Istruttore disattivato: mantieni comunque i dati del form per messaggio chiaro.
            instructor = new InstructorListItemViewModel
            {
                Id = form.InstructorId,
                Name = "Istruttore non disponibile",
                MaxParticipants = form.Participants
            };
        }

        var model = new BookingCalendarViewModel
        {
            InstructorId = form.InstructorId,
            InstructorName = instructor.Name,
            MaxParticipants = instructor.MaxParticipants,
            Form = form,
            Year = form.StartAt?.Year ?? DateTime.Today.Year,
            Month = form.StartAt?.Month ?? DateTime.Today.Month
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(Prefix = "Form")] BookingFormViewModel form,
        CancellationToken cancellationToken)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        NormalizePrivateLesson(form);

        if (!ModelState.IsValid)
        {
            return await RedisplayEditAsync(form, cancellationToken);
        }

        var (ok, error, notFound) = await _bookingService.UpdateAsync(UserId(), id, form, cancellationToken);
        if (notFound)
        {
            return NotFound();
        }

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return await RedisplayEditAsync(form, cancellationToken);
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

    [HttpGet]
    public async Task<IActionResult> AvailableDays(
        int instructorId,
        int year,
        int month,
        int lessonDurationId,
        int participants = 1,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || month is < 1 or > 12)
        {
            return BadRequest();
        }

        var minutes = await _bookingService.GetDurationMinutesAsync(lessonDurationId, cancellationToken);
        if (minutes is null)
        {
            return BadRequest();
        }

        participants = Math.Max(1, participants);
        var ownedExcludeId = await _bookingService.ResolveOwnedExcludeBookingIdAsync(
            UserId(),
            excludeBookingId,
            cancellationToken);

        var dates = await _availabilityService.GetAvailableDatesAsync(
            instructorId,
            year,
            month,
            minutes.Value,
            participants,
            ownedExcludeId,
            cancellationToken);

        var myBookings = await _bookingService.GetUserActiveBookingIdsByDateAsync(
            UserId(),
            instructorId,
            year,
            month,
            cancellationToken);

        var byDate = new Dictionary<string, BookingDayAvailabilityViewModel>();
        foreach (var d in dates)
        {
            byDate[d] = new BookingDayAvailabilityViewModel
            {
                Date = d,
                HasSlots = true,
                ExistingBookingId = myBookings.TryGetValue(d, out var bid) ? bid : null
            };
        }

        foreach (var (date, bookingId) in myBookings)
        {
            if (byDate.TryGetValue(date, out var day))
            {
                day.ExistingBookingId = bookingId;
                continue;
            }

            // Giorno con prenotazione utente ma senza slot liberi: abilitato per modifica.
            byDate[date] = new BookingDayAvailabilityViewModel
            {
                Date = date,
                HasSlots = false,
                ExistingBookingId = bookingId
            };
        }

        return Json(byDate.Values.OrderBy(d => d.Date));
    }

    [HttpGet]
    public async Task<IActionResult> AvailableSlots(
        int instructorId,
        string date,
        int lessonDurationId,
        int participants = 1,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParse(date, out var day))
        {
            return BadRequest();
        }

        var minutes = await _bookingService.GetDurationMinutesAsync(lessonDurationId, cancellationToken);
        if (minutes is null)
        {
            return BadRequest();
        }

        participants = Math.Max(1, participants);
        var ownedExcludeId = await _bookingService.ResolveOwnedExcludeBookingIdAsync(
            UserId(),
            excludeBookingId,
            cancellationToken);

        var slots = await _availabilityService.GetFreeSlotsAsync(
            instructorId,
            day,
            minutes.Value,
            participants,
            ownedExcludeId,
            cancellationToken);

        return Json(slots.Select(s => new
        {
            startAt = s.StartAt.ToString("yyyy-MM-ddTHH:mm"),
            label = s.Label,
            remainingCapacity = s.RemainingCapacity
        }));
    }

    private async Task<IActionResult> RedisplayCreateAsync(BookingFormViewModel form, CancellationToken cancellationToken)
    {
        form = await _bookingService.PrepareFormAsync(form, cancellationToken);
        var instructor = await _instructorService.GetActiveByIdAsync(form.InstructorId, cancellationToken);
        if (instructor is null)
        {
            var instructors = await _instructorService.GetActiveAsync(cancellationToken);
            return View("SelectInstructor", instructors);
        }

        var model = new BookingCalendarViewModel
        {
            InstructorId = instructor.Id,
            InstructorName = instructor.Name,
            MaxParticipants = instructor.MaxParticipants,
            Form = form,
            Year = form.StartAt?.Year ?? _clock.Today.Year,
            Month = form.StartAt?.Month ?? _clock.Today.Month
        };

        return View(model);
    }

    private async Task<IActionResult> RedisplayEditAsync(BookingFormViewModel form, CancellationToken cancellationToken)
    {
        form = await _bookingService.PrepareFormAsync(form, cancellationToken);
        var instructor = await _instructorService.GetActiveByIdAsync(form.InstructorId, cancellationToken);
        var model = new BookingCalendarViewModel
        {
            InstructorId = form.InstructorId,
            InstructorName = instructor?.Name ?? "Istruttore",
            MaxParticipants = instructor?.MaxParticipants ?? form.Participants,
            Form = form,
            Year = form.StartAt?.Year ?? _clock.Today.Year,
            Month = form.StartAt?.Month ?? _clock.Today.Month
        };
        return View(model);
    }

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Utente non autenticato.");

    private void NormalizePrivateLesson(BookingFormViewModel model)
    {
        if (model.LessonType == Models.LessonType.Privata)
        {
            model.Participants = 1;
            ModelState.Remove("Form.Participants");
            ModelState.Remove(nameof(model.Participants));
        }
    }
}
