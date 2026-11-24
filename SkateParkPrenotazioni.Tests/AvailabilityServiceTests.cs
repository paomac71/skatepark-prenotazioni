using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Tests;

public class AvailabilityServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 11, 24, 8, 0, 0);

    [Fact]
    public async Task GetFreeSlotsAsync_ReturnsOnlySlotsInsideWindows_WithRemainingCapacity()
    {
        await using var db = CreateDb();
        var tomorrow = DateOnly.FromDateTime(FixedNow.Date.AddDays(1));
        db.Instructors.Add(new Instructor { Name = "Luca", Bio = "Test", MaxParticipants = 2, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();

        db.InstructorAvailabilities.Add(new InstructorAvailability
        {
            InstructorId = 1,
            DayOfWeek = tomorrow.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            IsActive = true
        });
        db.Bookings.Add(new Booking
        {
            UserId = "u1",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = tomorrow.ToDateTime(new TimeOnly(10, 0)),
            Participants = 1
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var slots = await sut.GetFreeSlotsAsync(1, tomorrow, 60, 1);

        Assert.Equal(2, slots.Count);
        Assert.Equal("10:00", slots[0].Label);
        Assert.Equal(1, slots[0].RemainingCapacity);
        Assert.Equal("11:00", slots[1].Label);
        Assert.Equal(2, slots[1].RemainingCapacity);
    }

    [Fact]
    public async Task IsWithinAvailabilityAsync_ReturnsFalse_OutsideWindow()
    {
        await using var db = CreateDb();
        var day = DateOnly.FromDateTime(FixedNow.Date.AddDays(2));
        db.Instructors.Add(new Instructor { Name = "Giulia", Bio = "Test", MaxParticipants = 4, IsActive = true });
        await db.SaveChangesAsync();
        db.InstructorAvailabilities.Add(new InstructorAvailability
        {
            InstructorId = 1,
            DayOfWeek = day.DayOfWeek,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var ok = await sut.IsWithinAvailabilityAsync(
            1,
            day.ToDateTime(new TimeOnly(10, 30)),
            60);

        Assert.False(ok);
    }

    [Fact]
    public async Task GetAvailableDatesAsync_IncludesOnlyDaysWithFreeSlots()
    {
        await using var db = CreateDb();
        var today = DateOnly.FromDateTime(FixedNow);
        db.Instructors.Add(new Instructor { Name = "Marco", Bio = "Test", MaxParticipants = 1, IsActive = true });
        await db.SaveChangesAsync();

        db.InstructorAvailabilities.Add(new InstructorAvailability
        {
            InstructorId = 1,
            DayOfWeek = today.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dates = await sut.GetAvailableDatesAsync(1, today.Year, today.Month, 60, 1);

        Assert.Contains(today.ToString("yyyy-MM-dd"), dates);
        Assert.All(dates, d => Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", d));
    }

    private static AvailabilityService CreateSut(ApplicationDbContext db)
    {
        var clock = new TestAppClock(FixedNow);
        var capacity = new BookingCapacityService(db, clock);
        return new AvailabilityService(db, capacity, clock);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
