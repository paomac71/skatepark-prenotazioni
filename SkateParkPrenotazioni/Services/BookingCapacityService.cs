using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Services;

public class BookingCapacityService(ApplicationDbContext db) : IBookingCapacityService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<(bool Ok, string? Error)> ValidateAsync(
        int instructorId,
        DateTime startAt,
        int durationMinutes,
        int participants,
        LessonType lessonType,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var instructor = await _db.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == instructorId && x.IsActive, cancellationToken);

        if (instructor is null)
        {
            return (false, "Istruttore non trovato o non attivo.");
        }

        if (durationMinutes <= 0)
        {
            return (false, "Durata lezione non valida.");
        }

        if (lessonType == LessonType.Privata && participants != 1)
        {
            return (false, "Una lezione privata ammette un solo partecipante.");
        }

        if (participants > instructor.MaxParticipants)
        {
            return (false, $"L'istruttore ammette al massimo {instructor.MaxParticipants} partecipanti.");
        }

        if (startAt < DateTime.Now.AddMinutes(-1))
        {
            return (false, "Non è possibile prenotare uno slot nel passato.");
        }

        var slotEnd = startAt.AddMinutes(durationMinutes);
        var occupied = await GetOccupiedParticipantsAsync(
            instructorId,
            startAt,
            slotEnd,
            excludeBookingId,
            cancellationToken);

        if (occupied + participants > instructor.MaxParticipants)
        {
            var remaining = Math.Max(0, instructor.MaxParticipants - occupied);
            return (false, remaining == 0
                ? "Lo slot selezionato è pieno per questo istruttore."
                : $"Posti rimanenti nello slot: {remaining}.");
        }

        return (true, null);
    }

    public async Task<int> GetOccupiedParticipantsAsync(
        int instructorId,
        DateTime slotStart,
        DateTime slotEnd,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var dayStart = slotStart.Date;
        var dayEnd = dayStart.AddDays(1);

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => !b.IsCancelled
                        && b.InstructorId == instructorId
                        && b.StartAt >= dayStart
                        && b.StartAt < dayEnd)
            .Select(b => new
            {
                b.Id,
                b.StartAt,
                b.Participants,
                Minutes = b.LessonDuration != null ? b.LessonDuration.Minutes : 0
            })
            .ToListAsync(cancellationToken);

        var occupied = 0;
        foreach (var booking in bookings)
        {
            if (excludeBookingId.HasValue && booking.Id == excludeBookingId.Value)
            {
                continue;
            }

            if (booking.Minutes <= 0)
            {
                continue;
            }

            var bookingEnd = booking.StartAt.AddMinutes(booking.Minutes);
            if (booking.StartAt < slotEnd && bookingEnd > slotStart)
            {
                occupied += booking.Participants;
            }
        }

        return occupied;
    }
}
