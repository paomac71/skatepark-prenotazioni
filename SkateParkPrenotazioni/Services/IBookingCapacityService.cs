using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Services;

public interface IBookingCapacityService
{
    Task<(bool Ok, string? Error)> ValidateAsync(
        int instructorId,
        DateTime startAt,
        int durationMinutes,
        int participants,
        LessonType lessonType,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);

    Task<int> GetOccupiedParticipantsAsync(
        int instructorId,
        DateTime slotStart,
        DateTime slotEnd,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);
}
