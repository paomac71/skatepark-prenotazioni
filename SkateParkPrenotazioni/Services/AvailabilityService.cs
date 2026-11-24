using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public class AvailabilityService(
    ApplicationDbContext db,
    IBookingCapacityService capacity,
    IAppClock clock) : IAvailabilityService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IBookingCapacityService _capacity = capacity;
    private readonly IAppClock _clock = clock;

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
        if (endAt.Date > startAt.Date)
        {
            return false;
        }

        var windows = await _db.InstructorAvailabilities
            .AsNoTracking()
            .Where(a => a.InstructorId == instructorId
                        && a.IsActive
                        && a.DayOfWeek == startAt.DayOfWeek)
            .ToListAsync(cancellationToken);

        var startTime = startAt.TimeOfDay;
        var endTime = endAt.TimeOfDay;
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

        var windows = await _db.InstructorAvailabilities
            .AsNoTracking()
            .Where(a => a.InstructorId == instructorId && a.IsActive)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        if (windows.Count == 0)
        {
            return Array.Empty<string>();
        }

        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var rangeStart = monthStart.ToDateTime(TimeOnly.MinValue);
        var rangeEnd = monthEnd.ToDateTime(TimeOnly.MinValue);

        var bookings = await LoadBookingsAsync(instructorId, rangeStart, rangeEnd, cancellationToken);
        var today = _clock.Today;
        var now = _clock.Now;
        var available = new List<string>();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            if (date < today)
            {
                continue;
            }

            var dayWindows = windows.Where(w => w.DayOfWeek == date.DayOfWeek).ToList();
            if (dayWindows.Count == 0)
            {
                continue;
            }

            var slots = BuildFreeSlots(
                instructor,
                date,
                dayWindows,
                bookings,
                durationMinutes,
                participants,
                excludeBookingId,
                now);

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

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);
        var bookings = await LoadBookingsAsync(instructorId, dayStart, dayEnd, cancellationToken);

        return BuildFreeSlots(
            instructor,
            date,
            windows,
            bookings,
            durationMinutes,
            participants,
            excludeBookingId,
            _clock.Now);
    }

    private async Task<List<(int Id, DateTime StartAt, int Participants, int Minutes)>> LoadBookingsAsync(
        int instructorId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken)
    {
        var rows = await _db.Bookings
            .AsNoTracking()
            .Where(b => !b.IsCancelled
                        && b.InstructorId == instructorId
                        && b.StartAt >= rangeStart
                        && b.StartAt < rangeEnd)
            .Select(b => new
            {
                b.Id,
                b.StartAt,
                b.Participants,
                Minutes = b.LessonDuration != null ? b.LessonDuration.Minutes : 0
            })
            .ToListAsync(cancellationToken);

        return rows.Select(b => (b.Id, b.StartAt, b.Participants, b.Minutes)).ToList();
    }

    private List<BookingSlotViewModel> BuildFreeSlots(
        Instructor instructor,
        DateOnly date,
        IReadOnlyList<InstructorAvailability> windows,
        IReadOnlyList<(int Id, DateTime StartAt, int Participants, int Minutes)> bookings,
        int durationMinutes,
        int participants,
        int? excludeBookingId,
        DateTime now)
    {
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
                    var occupied = _capacity.GetOccupiedParticipants(
                        bookings,
                        cursor,
                        slotEnd,
                        excludeBookingId);
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
