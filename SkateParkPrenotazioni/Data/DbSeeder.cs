using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        await db.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
        }

        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail)
            && !string.IsNullOrWhiteSpace(adminPassword)
            && !adminEmail.Contains("YOUR_", StringComparison.Ordinal)
            && !adminPassword.Contains("YOUR_", StringComparison.Ordinal))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }
            else if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }

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
            await db.SaveChangesAsync();
        }

        if (!await db.LessonDurations.AnyAsync())
        {
            db.LessonDurations.AddRange(
                new LessonDuration { Minutes = 30, Label = "30 minuti", IsActive = true },
                new LessonDuration { Minutes = 60, Label = "60 minuti", IsActive = true },
                new LessonDuration { Minutes = 90, Label = "90 minuti", IsActive = true });
            await db.SaveChangesAsync();
        }

        if (!await db.InstructorAvailabilities.AnyAsync())
        {
            var instructors = await db.Instructors.AsNoTracking().OrderBy(i => i.Id).ToListAsync();
            foreach (var instructor in instructors)
            {
                db.InstructorAvailabilities.AddRange(DefaultWindowsFor(instructor));
            }

            await db.SaveChangesAsync();
        }
    }

    private static IEnumerable<InstructorAvailability> DefaultWindowsFor(Instructor instructor)
    {
        // Pattern diversi per dimostrare giorni disponibili per istruttore.
        return instructor.Name switch
        {
            "Luca Bianchi" => WeekdayWindows(instructor.Id, DayOfWeek.Monday, DayOfWeek.Friday, new(9, 0, 0), new(13, 0, 0))
                .Concat(WeekdayWindows(instructor.Id, DayOfWeek.Monday, DayOfWeek.Friday, new(15, 0, 0), new(18, 0, 0))),
            "Giulia Rossi" => WeekdayWindows(instructor.Id, DayOfWeek.Tuesday, DayOfWeek.Saturday, new(10, 0, 0), new(12, 0, 0))
                .Concat(WeekdayWindows(instructor.Id, DayOfWeek.Tuesday, DayOfWeek.Saturday, new(14, 0, 0), new(17, 0, 0))),
            _ => WeekdayWindows(instructor.Id, DayOfWeek.Wednesday, DayOfWeek.Sunday, new(9, 0, 0), new(12, 0, 0))
                .Concat(WeekdayWindows(instructor.Id, DayOfWeek.Wednesday, DayOfWeek.Sunday, new(16, 0, 0), new(19, 0, 0)))
        };
    }

    private static IEnumerable<InstructorAvailability> WeekdayWindows(
        int instructorId,
        DayOfWeek from,
        DayOfWeek to,
        TimeSpan start,
        TimeSpan end)
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            if (!IsInRange(day, from, to))
            {
                continue;
            }

            yield return new InstructorAvailability
            {
                InstructorId = instructorId,
                DayOfWeek = day,
                StartTime = start,
                EndTime = end,
                IsActive = true
            };
        }
    }

    private static bool IsInRange(DayOfWeek day, DayOfWeek from, DayOfWeek to)
    {
        // Intervallo inclusivo su settimana Lun→Dom (0=Dom … 6=Sab).
        var d = (int)day;
        var a = (int)from;
        var b = (int)to;
        if (a <= b)
        {
            return d >= a && d <= b;
        }

        return d >= a || d <= b;
    }
}
