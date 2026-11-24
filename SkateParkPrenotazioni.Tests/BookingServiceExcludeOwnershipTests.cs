using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SkateParkPrenotazioni.Data;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Tests;

public class BookingServiceExcludeOwnershipTests
{
    [Fact]
    public async Task ResolveOwnedExcludeBookingIdAsync_ReturnsId_WhenOwned()
    {
        await using var db = CreateDb();
        var id = await SeedBookingAsync(db, userId: "owner");

        var sut = CreateSut(db);
        var resolved = await sut.ResolveOwnedExcludeBookingIdAsync("owner", id);

        Assert.Equal(id, resolved);
    }

    [Fact]
    public async Task ResolveOwnedExcludeBookingIdAsync_ReturnsNull_WhenNotOwned()
    {
        await using var db = CreateDb();
        var id = await SeedBookingAsync(db, userId: "owner");

        var sut = CreateSut(db);
        var resolved = await sut.ResolveOwnedExcludeBookingIdAsync("other-user", id);

        Assert.Null(resolved);
    }

    [Fact]
    public async Task ResolveOwnedExcludeBookingIdAsync_ReturnsNull_WhenMissing()
    {
        await using var db = CreateDb();
        var sut = CreateSut(db);

        var resolved = await sut.ResolveOwnedExcludeBookingIdAsync("owner", 999);

        Assert.Null(resolved);
    }

    private static async Task<int> SeedBookingAsync(ApplicationDbContext db, string userId)
    {
        db.Instructors.Add(new Instructor { Name = "Luca", Bio = "Test", MaxParticipants = 2, IsActive = true });
        db.LessonDurations.Add(new LessonDuration { Label = "60", Minutes = 60, IsActive = true });
        await db.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = userId,
            InstructorId = 1,
            LessonDurationId = 1,
            LessonType = LessonType.Privata,
            StartAt = DateTime.Today.AddDays(1).AddHours(10),
            Participants = 1
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking.Id;
    }

    private static BookingService CreateSut(ApplicationDbContext db)
    {
        var clock = new TestAppClock(DateTime.Today.AddHours(8));
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
