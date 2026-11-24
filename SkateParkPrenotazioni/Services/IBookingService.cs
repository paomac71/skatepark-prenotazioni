using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public interface IBookingService
{
    Task<IReadOnlyList<BookingListItemViewModel>> GetUserBookingsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<BookingFormViewModel> PrepareFormAsync(
        BookingFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<(bool Ok, string? Error)> CreateAsync(
        string userId,
        BookingFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<BookingFormViewModel?> GetEditFormAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default);

    Task<(bool Ok, string? Error, bool NotFound)> UpdateAsync(
        string userId,
        int id,
        BookingFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<(bool Ok, bool NotFound)> CancelAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default);

    Task<int?> GetDurationMinutesAsync(int lessonDurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restituisce excludeBookingId solo se appartiene all'utente; altrimenti null.
    /// </summary>
    Task<int?> ResolveOwnedExcludeBookingIdAsync(
        string userId,
        int? excludeBookingId,
        CancellationToken cancellationToken = default);

    Task<UserDayBookingViewModel?> GetUserActiveBookingOnDayAsync(
        string userId,
        int instructorId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, int>> GetUserActiveBookingIdsByDateAsync(
        string userId,
        int instructorId,
        int year,
        int month,
        CancellationToken cancellationToken = default);
}
