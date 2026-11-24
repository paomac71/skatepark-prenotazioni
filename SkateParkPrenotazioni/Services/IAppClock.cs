namespace SkateParkPrenotazioni.Services;

/// <summary>
/// Orologio applicativo nel fuso configurato (Weather:Timezone, default Europe/Rome).
/// </summary>
public interface IAppClock
{
    DateTime Now { get; }

    DateOnly Today { get; }
}
