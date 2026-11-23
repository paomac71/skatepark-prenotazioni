using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public class BookingService(
    ApplicationDbContext db,
    IBookingCapacityService capacity,
    IAvailabilityService availability,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IBookingCapacityService _capacity = capacity;
    private readonly IAvailabilityService _availability = availability;
    private readonly ILogger<BookingService> _logger = logger;

    public async Task<IReadOnlyList<BookingListItemViewModel>> GetUserBookingsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                StartAt = b.StartAt,
                InstructorName = b.Instructor != null ? b.Instructor.Name : string.Empty,
                LessonType = b.LessonType,
                DurationLabel = b.LessonDuration != null ? b.LessonDuration.Label : string.Empty,
                Participants = b.Participants,
                IsCancelled = b.IsCancelled
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingFormViewModel> PrepareFormAsync(
        BookingFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        model.Instructors = await _db.Instructors
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.Name} (max {i.MaxParticipants})",
                Selected = i.Id == model.InstructorId
            })
            .ToListAsync(cancellationToken);

        model.Durations = await _db.LessonDurations
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Minutes)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Label,
                Selected = d.Id == model.LessonDurationId
            })
            .ToListAsync(cancellationToken);

        if (model.LessonDurationId == 0)
        {
            var firstDuration = model.Durations.FirstOrDefault();
            if (firstDuration is not null && int.TryParse(firstDuration.Value, out var durationId))
            {
                model.LessonDurationId = durationId;
            }
        }

        return model;
    }

    public async Task<int?> GetDurationMinutesAsync(int lessonDurationId, CancellationToken cancellationToken = default)
    {
        return await _db.LessonDurations
            .AsNoTracking()
            .Where(d => d.Id == lessonDurationId && d.IsActive)
            .Select(d => (int?)d.Minutes)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Ok, string? Error)> CreateAsync(
        string userId,
        BookingFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        NormalizeParticipants(model);

        if (model.StartAt is null)
        {
            return (false, "Seleziona un orario libero dal calendario.");
        }

        var startAt = model.StartAt.Value;
        var durationMinutes = await GetDurationMinutesAsync(model.LessonDurationId, cancellationToken);
        if (durationMinutes is null)
        {
            return (false, "Durata lezione non valida.");
        }

        if (!await _availability.IsWithinAvailabilityAsync(
                model.InstructorId,
                startAt,
                durationMinutes.Value,
                cancellationToken))
        {
            return (false, "Lo slot selezionato non rientra nei giorni/orari disponibili dell'istruttore.");
        }

        var (ok, error) = await _capacity.ValidateAsync(
            model.InstructorId,
            startAt,
            durationMinutes.Value,
            model.Participants,
            model.LessonType,
            cancellationToken: cancellationToken);

        if (!ok)
        {
            return (false, error);
        }

        var booking = new Booking
        {
            UserId = userId,
            InstructorId = model.InstructorId,
            LessonDurationId = model.LessonDurationId,
            LessonType = model.LessonType,
            StartAt = startAt,
            Participants = model.Participants,
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Prenotazione {BookingId} creata per utente {UserId}", booking.Id, userId);
        return (true, null);
    }

    public async Task<BookingFormViewModel?> GetEditFormAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var booking = await FindOwnedAsync(userId, id, cancellationToken);
        if (booking is null || booking.IsCancelled)
        {
            return null;
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

        return await PrepareFormAsync(model, cancellationToken);
    }

    public async Task<(bool Ok, string? Error, bool NotFound)> UpdateAsync(
        string userId,
        int id,
        BookingFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var booking = await FindOwnedAsync(userId, id, cancellationToken);
        if (booking is null || booking.IsCancelled)
        {
            return (false, null, true);
        }

        NormalizeParticipants(model);

        if (model.StartAt is null)
        {
            return (false, "Seleziona un orario libero dal calendario.", false);
        }

        var startAt = model.StartAt.Value;
        var durationMinutes = await GetDurationMinutesAsync(model.LessonDurationId, cancellationToken);
        if (durationMinutes is null)
        {
            return (false, "Durata lezione non valida.", false);
        }

        if (!await _availability.IsWithinAvailabilityAsync(
                model.InstructorId,
                startAt,
                durationMinutes.Value,
                cancellationToken))
        {
            return (false, "Lo slot selezionato non rientra nei giorni/orari disponibili dell'istruttore.", false);
        }

        var (ok, error) = await _capacity.ValidateAsync(
            model.InstructorId,
            startAt,
            durationMinutes.Value,
            model.Participants,
            model.LessonType,
            excludeBookingId: id,
            cancellationToken: cancellationToken);

        if (!ok)
        {
            return (false, error, false);
        }

        booking.InstructorId = model.InstructorId;
        booking.LessonDurationId = model.LessonDurationId;
        booking.LessonType = model.LessonType;
        booking.StartAt = startAt;
        booking.Participants = model.Participants;
        booking.Notes = model.Notes;

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Prenotazione {BookingId} aggiornata per utente {UserId}", booking.Id, userId);
        return (true, null, false);
    }

    public async Task<(bool Ok, bool NotFound)> CancelAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var booking = await FindOwnedAsync(userId, id, cancellationToken);
        if (booking is null)
        {
            return (false, true);
        }

        if (!booking.IsCancelled)
        {
            booking.IsCancelled = true;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Prenotazione {BookingId} annullata per utente {UserId}", booking.Id, userId);
        }

        return (true, false);
    }

    private Task<Booking?> FindOwnedAsync(string userId, int id, CancellationToken cancellationToken) =>
        _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

    private static void NormalizeParticipants(BookingFormViewModel model)
    {
        if (model.LessonType == LessonType.Privata)
        {
            model.Participants = 1;
        }
    }
}
