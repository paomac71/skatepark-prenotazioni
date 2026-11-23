using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorListItemViewModel>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InstructorManageItemViewModel>> GetAllForManageAsync(CancellationToken cancellationToken = default);

    Task<InstructorEditViewModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool Ok, string? Error)> UpdateAsync(InstructorEditViewModel model, CancellationToken cancellationToken = default);

    Task<InstructorListItemViewModel?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);
}
