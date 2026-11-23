using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
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

    public async Task<InstructorListItemViewModel?> GetActiveByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Instructors
            .AsNoTracking()
            .Where(i => i.Id == id && i.IsActive)
            .Select(i => new InstructorListItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Bio = i.Bio,
                MaxParticipants = i.MaxParticipants
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InstructorManageItemViewModel>> GetAllForManageAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Instructors
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new InstructorManageItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                MaxParticipants = i.MaxParticipants,
                IsActive = i.IsActive,
                AvailabilityWindows = i.Availabilities.Count(a => a.IsActive)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InstructorEditViewModel?> GetEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var instructor = await _db.Instructors
            .AsNoTracking()
            .Include(i => i.Availabilities)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (instructor is null)
        {
            return null;
        }

        return new InstructorEditViewModel
        {
            Id = instructor.Id,
            Name = instructor.Name,
            Bio = instructor.Bio,
            MaxParticipants = instructor.MaxParticipants,
            IsActive = instructor.IsActive,
            Availabilities = instructor.Availabilities
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .Select(a => new AvailabilityWindowViewModel
                {
                    Id = a.Id,
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    IsActive = a.IsActive
                })
                .ToList()
        };
    }

    public async Task<(bool Ok, string? Error)> UpdateAsync(
        InstructorEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        var instructor = await _db.Instructors
            .Include(i => i.Availabilities)
            .FirstOrDefaultAsync(i => i.Id == model.Id, cancellationToken);

        if (instructor is null)
        {
            return (false, "Istruttore non trovato.");
        }

        var windows = model.Availabilities ?? new List<AvailabilityWindowViewModel>();
        foreach (var window in windows)
        {
            if (window.EndTime <= window.StartTime)
            {
                return (false, "Ogni fascia oraria deve avere un orario di fine successivo all'inizio.");
            }
        }

        instructor.Name = model.Name.Trim();
        instructor.Bio = model.Bio.Trim();
        instructor.MaxParticipants = model.MaxParticipants;
        instructor.IsActive = model.IsActive;

        _db.InstructorAvailabilities.RemoveRange(instructor.Availabilities);

        foreach (var window in windows)
        {
            instructor.Availabilities.Add(new InstructorAvailability
            {
                DayOfWeek = window.DayOfWeek,
                StartTime = window.StartTime,
                EndTime = window.EndTime,
                IsActive = window.IsActive
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
