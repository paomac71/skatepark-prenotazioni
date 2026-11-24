using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Tests;

internal sealed class TestAppClock(DateTime now) : IAppClock
{
    public DateTime Now { get; } = now;

    public DateOnly Today => DateOnly.FromDateTime(Now);
}
