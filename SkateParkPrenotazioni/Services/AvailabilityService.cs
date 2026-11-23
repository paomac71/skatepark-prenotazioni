using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public class AvailabilityService(
    ApplicationDbContext db,
    IBookingCapacityService capacity) : IAvailabilityService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IBookingCapacityService _capacity = capacity;

    public async Task<bool> IsWithinAvailabilityAsync(
        int instructorId,
        DateTime startAt,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        if (durationMinutes <= 0)
        {
            return false;
        }

        var endAt = startAt.AddMinutes(durationMinutes);
        var windows = await _db.InstructorAvailabilities
            .AsNoTracking()
            .Where(a => a.InstructorId == instructorId
                        && a.IsActive
                        && a.DayOfWeek == startAt.DayOfWeek)
            .ToListAsync(cancellationToken);

        var startTime = startAt.TimeOfDay;
        var endTime = endAt.TimeOfDay;
        // Lezione che termina a mezzanotte esatta: endTime = 00:00 → confronta con EndTime della finestra.
        if (endAt.Date > startAt.Date)
        {
            return false;
        }

        return windows.Any(w => startTime >= w.StartTime && endTime <= w.EndTime);
    }

    public async Task<IReadOnlyList<string>> GetAvailableDatesAsync(
        int instructorId,
        int year,
        int month,
        int durationMinutes,
        int participants,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        if (durationMinutes <= 0 || participants <= 0)
        {
            return Array.Empty<string>();
        }

        var instructor = await _db.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == instructorId && i.IsActive, cancellationToken);

        if (instructor is null || participants > instructor.MaxParticipants)
        {
            return Array.Empty<string>();
        }

        var activeDays = await _db.InstructorAvailabilities
            .AsNoTracking()
            .Where(a => a.InstructorId == instructorId && a.IsActive)
            .Select(a => a.DayOfWeek)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (activeDays.Count == 0)
        {
            return Array.Empty<string>();
        }

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var available = new List<string>();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            if (date < today)
            {
                continue;
            }

            if (!activeDays.Contains(date.DayOfWeek))
            {
                continue;
            }

            var slots = await GetFreeSlotsAsync(
                instructorId,
                date,
                durationMinutes,
                participants,
                excludeBookingId,
                cancellationToken);

            if (slots.Count > 0)
            {
                available.Add(date.ToString("yyyy-MM-dd"));
            }
        }

        return available;
    }

    public async Task<IReadOnlyList<BookingSlotViewModel>> GetFreeSlotsAsync(
        int instructorId,
        DateOnly date,
        int durationMinutes,
        int participants,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        if (durationMinutes <= 0 || participants <= 0)
        {
            return Array.Empty<BookingSlotViewModel>();
        }

        var instructor = await _db.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == instructorId && i.IsActive, cancellationToken);

        if (instructor is null || participants > instructor.MaxParticipants)
        {
            return Array.Empty<BookingSlotViewModel>();
        }

        var windows = await _db.InstructorAvailabilities
            .AsNoTracking()
            .Where(a => a.InstructorId == instructorId
                        && a.IsActive
                        && a.DayOfWeek == date.DayOfWeek)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        if (windows.Count == 0)
        {
            return Array.Empty<BookingSlotViewModel>();
        }

        var now = DateTime.Now;
        var slots = new List<BookingSlotViewModel>();

        foreach (var window in windows)
        {
            var cursor = date.ToDateTime(TimeOnly.FromTimeSpan(window.StartTime));
            var windowEnd = date.ToDateTime(TimeOnly.FromTimeSpan(window.EndTime));

            while (cursor.AddMinutes(durationMinutes) <= windowEnd)
            {
                if (cursor >= now)
                {
                    var slotEnd = cursor.AddMinutes(durationMinutes);
                    var occupied = await _capacity.GetOccupiedParticipantsAsync(
                        instructorId,
                        cursor,
                        slotEnd,
                        excludeBookingId,
                        cancellationToken);
                    var remaining = instructor.MaxParticipants - occupied;

                    if (remaining >= participants)
                    {
                        slots.Add(new BookingSlotViewModel
                        {
                            StartAt = cursor,
                            Label = cursor.ToString("HH:mm"),
                            RemainingCapacity = remaining
                        });
                    }
                }

                cursor = cursor.AddMinutes(durationMinutes);
            }
        }

        return slots
            .GroupBy(s => s.StartAt)
            .Select(g => g.First())
            .OrderBy(s => s.StartAt)
            .ToList();
    }
}
