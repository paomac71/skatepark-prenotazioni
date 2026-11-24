using Microsoft.Extensions.Options;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.Services;

public class AppClock(IOptions<WeatherOptions> weatherOptions) : IAppClock
{
    private readonly TimeZoneInfo _timeZone = ResolveTimeZone(weatherOptions.Value.Timezone);

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

    public DateOnly Today => DateOnly.FromDateTime(Now);

    private static TimeZoneInfo ResolveTimeZone(string? id)
    {
        var timezoneId = string.IsNullOrWhiteSpace(id) ? "Europe/Rome" : id.Trim();
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Windows usa spesso "W. Europe Standard Time".
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
