using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Tests;

public class BookingCapacityServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 11, 24, 8, 0, 0);

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenInstructorMissing()
    {
        await using var db = CreateDb();
        var sut = CreateSut(db);

        var (ok, error) = await sut.ValidateAsync(99, FixedNow.AddDays(1), 60, 1, LessonType.Privata);

        Assert.False(ok);
        Assert.Contains("Istruttore", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenPrivateLessonHasMultipleParticipants()
    {
        await using var db = CreateDb();
        db.Instructors.Add(new Instructor { Name = "Luca", Bio = "Test", MaxParticipants = 4, IsActive = true });
        await db.SaveChangesAsync();
        var sut = CreateSut(db);

        var (ok, error) = await sut.ValidateAsync(1, FixedNow.AddDays(1), 60, 2, LessonType.Privata);

        Assert.False(ok);
        Assert.Contains("privata", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenSlotIsFull()
    {
        await using var db = CreateDb();
        var start = FixedNow.Date.AddDays(1).AddHours(10);
        db.Instructors.Add(new Instructor { Name = "Giulia", Bio = "Test", MaxParticipants = 2, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60 min", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();

        db.Bookings.Add(new Booking
        {
            UserId = "user-a",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = start,
            Participants = 2
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var (ok, error) = await sut.ValidateAsync(1, start, 60, 1, LessonType.Gruppo);

        Assert.False(ok);
        Assert.Contains("pieno", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenOverlappingBookingFillsCapacity()
    {
        await using var db = CreateDb();
        var start = FixedNow.Date.AddDays(1).AddHours(10);
        db.Instructors.Add(new Instructor { Name = "Marco", Bio = "Test", MaxParticipants = 1, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60 min", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();

        db.Bookings.Add(new Booking
        {
            UserId = "user-a",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Privata,
            StartAt = start,
            Participants = 1
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var (ok, error) = await sut.ValidateAsync(1, start.AddMinutes(30), 60, 1, LessonType.Privata);

        Assert.False(ok);
        Assert.Contains("pieno", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_Succeeds_WhenCapacityAvailable()
    {
        await using var db = CreateDb();
        var start = FixedNow.Date.AddDays(1).AddHours(10);
        db.Instructors.Add(new Instructor { Name = "Marco", Bio = "Test", MaxParticipants = 4, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60 min", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();

        db.Bookings.Add(new Booking
        {
            UserId = "user-a",
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Gruppo,
            StartAt = start,
            Participants = 2
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var (ok, error) = await sut.ValidateAsync(1, start, 60, 2, LessonType.Gruppo);

        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenSlotIsInThePast()
    {
        await using var db = CreateDb();
        db.Instructors.Add(new Instructor { Name = "Luca", Bio = "Test", MaxParticipants = 1, IsActive = true });
        await db.SaveChangesAsync();
        var sut = CreateSut(db);

        var (ok, error) = await sut.ValidateAsync(1, FixedNow.AddHours(-2), 60, 1, LessonType.Privata);

        Assert.False(ok);
        Assert.Contains("passato", error, StringComparison.OrdinalIgnoreCase);
    }

    private static BookingCapacityService CreateSut(ApplicationDbContext db) =>
        new(db, new TestAppClock(FixedNow));

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
