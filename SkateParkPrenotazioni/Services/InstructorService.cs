using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public class InstructorService(ApplicationDbContext db) : IInstructorService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IReadOnlyList<InstructorListItemViewModel>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Instructors
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new InstructorListItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Bio = i.Bio,
                MaxParticipants = i.MaxParticipants
            })
            .ToListAsync(cancellationToken);
    }
}
