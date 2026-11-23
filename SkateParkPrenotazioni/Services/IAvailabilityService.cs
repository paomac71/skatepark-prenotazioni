using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public interface IAvailabilityService
{
    Task<bool> IsWithinAvailabilityAsync(
        int instructorId,
        DateTime startAt,
        int durationMinutes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAvailableDatesAsync(
        int instructorId,
        int year,
        int month,
        int durationMinutes,
        int participants,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingSlotViewModel>> GetFreeSlotsAsync(
        int instructorId,
        DateOnly date,
        int durationMinutes,
        int participants,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default);
}
