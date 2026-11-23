using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorListItemViewModel>> GetActiveAsync(CancellationToken cancellationToken = default);
}
