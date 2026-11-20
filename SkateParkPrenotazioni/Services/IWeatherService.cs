using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public interface IWeatherService
{
    Task<WeatherForecastViewModel> GetWeeklyForecastAsync(CancellationToken cancellationToken = default);
}
