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

        if (lessonType == LessonType.Privata && participants != 1)
        {
            return (false, "Una lezione privata ammette un solo partecipante.");
        }

        if (participants > instructor.MaxParticipants)
        {
            return (false, $"L'istruttore ammette al massimo {instructor.MaxParticipants} partecipanti.");
        }

        var query = _db.Bookings.AsNoTracking()
            .Where(b => !b.IsCancelled
                        && b.InstructorId == instructorId
                        && b.StartAt == startAt);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        var occupied = await query.SumAsync(b => b.Participants, cancellationToken);
        if (occupied + participants > instructor.MaxParticipants)
        {
            var remaining = Math.Max(0, instructor.MaxParticipants - occupied);
            return (false, remaining == 0
                ? "Lo slot selezionato è pieno per questo istruttore."
                : $"Posti rimanenti nello slot: {remaining}.");
        }

        return (true, null);
    }
}
