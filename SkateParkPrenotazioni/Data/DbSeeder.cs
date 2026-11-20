using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Instructors.AnyAsync())
        {
            db.Instructors.AddRange(
                new Instructor
                {
                    Name = "Luca Bianchi",
                    Bio = "Specialista freestyle e half-pipe, 12 anni di insegnamento.",
                    MaxParticipants = 1,
                    IsActive = true
                },
                new Instructor
                {
                    Name = "Giulia Rossi",
                    Bio = "Lezioni di gruppo e principianti, focus su sicurezza e tecnica di base.",
                    MaxParticipants = 6,
                    IsActive = true
                },
                new Instructor
                {
                    Name = "Marco Verdi",
                    Bio = "Street e bowl, percorsi intermedi e avanzati.",
                    MaxParticipants = 4,
                    IsActive = true
                });
        }

        if (!await db.LessonDurations.AnyAsync())
        {
            db.LessonDurations.AddRange(
                new LessonDuration { Minutes = 30, Label = "30 minuti", IsActive = true },
                new LessonDuration { Minutes = 60, Label = "60 minuti", IsActive = true },
                new LessonDuration { Minutes = 90, Label = "90 minuti", IsActive = true });
        }

        await db.SaveChangesAsync();
    }
}
