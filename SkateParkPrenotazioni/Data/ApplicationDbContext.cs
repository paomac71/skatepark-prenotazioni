using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<LessonDuration> LessonDurations => Set<LessonDuration>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<InstructorAvailability> InstructorAvailabilities => Set<InstructorAvailability>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Instructor>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Bio).HasMaxLength(500).IsRequired();
        });

        builder.Entity<LessonDuration>(entity =>
        {
            entity.Property(x => x.Label).HasMaxLength(50).IsRequired();
        });

        builder.Entity<InstructorAvailability>(entity =>
        {
            entity.HasOne(x => x.Instructor)
                .WithMany(x => x.Availabilities)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.InstructorId, x.DayOfWeek });
        });

        builder.Entity<Booking>(entity =>
        {
            entity.HasOne(x => x.Instructor)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LessonDuration)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.LessonDurationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.InstructorId, x.StartAt });
        });
    }
}
