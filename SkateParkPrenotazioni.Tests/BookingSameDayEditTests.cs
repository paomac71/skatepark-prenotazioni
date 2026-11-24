using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Tests;

public class BookingSameDayEditTests
{
    private static readonly DateTime FixedNow = new(2026, 11, 24, 8, 0, 0);

    [Fact]
    public async Task GetUserActiveBookingOnDayAsync_ReturnsExistingBooking()
    {
        await using var db = CreateDb();
        var day = DateOnly.FromDateTime(FixedNow.Date.AddDays(1));
        await SeedInstructorAndDurationAsync(db);
        db.Bookings.Add(new Booking
        {
            UserId = "u1",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = day.ToDateTime(new TimeOnly(10, 0)),
            Participants = 2
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var found = await sut.GetUserActiveBookingOnDayAsync("u1", 1, day);

        Assert.NotNull(found);
        Assert.Equal(2, found.Participants);
    }

    [Fact]
    public async Task CreateAsync_UpdatesExistingSameDayBooking_InsteadOfDuplicating()
    {
        await using var db = CreateDb();
        var day = DateOnly.FromDateTime(FixedNow.Date.AddDays(1));
        await SeedInstructorAndDurationAsync(db);
        db.InstructorAvailabilities.Add(new InstructorAvailability
        {
            InstructorId = 1,
            DayOfWeek = day.DayOfWeek,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsActive = true
        });
        db.Bookings.Add(new Booking
        {
            UserId = "u1",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = day.ToDateTime(new TimeOnly(10, 0)),
            Participants = 2
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var (ok, error) = await sut.CreateAsync("u1", new BookingFormViewModel
        {
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = day.ToDateTime(new TimeOnly(11, 0)),
            Participants = 3
        });

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(1, await db.Bookings.CountAsync(b => !b.IsCancelled && b.UserId == "u1"));
        var booking = await db.Bookings.SingleAsync(b => b.UserId == "u1");
        Assert.Equal(3, booking.Participants);
        Assert.Equal(day.ToDateTime(new TimeOnly(11, 0)), booking.StartAt);
    }

    private static async Task SeedInstructorAndDurationAsync(ApplicationDbContext db)
    {
        db.Instructors.Add(new Instructor { Name = "Giulia", Bio = "Test", MaxParticipants = 6, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();
    }

    private static BookingService CreateSut(ApplicationDbContext db)
    {
        var clock = new TestAppClock(FixedNow);
        var capacity = new BookingCapacityService(db, clock);
        var availability = new AvailabilityService(db, capacity, clock);
        return new BookingService(db, capacity, availability, NullLogger<BookingService>.Instance);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
