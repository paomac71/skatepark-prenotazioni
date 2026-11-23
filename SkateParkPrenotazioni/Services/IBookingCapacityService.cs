using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Services;

public interface IBookingCapacityService
{
    Task<(bool Ok, string? Error)> ValidateAsync(
        int instructorId,
        DateTime startAt,
        int participants,
        LessonType lessonType,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);
}
