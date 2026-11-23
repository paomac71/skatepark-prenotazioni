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
}
