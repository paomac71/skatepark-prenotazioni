using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Controllers;

public class WeatherController(IWeatherService weatherService) : Controller
{
    private readonly IWeatherService _weatherService = weatherService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var forecast = await _weatherService.GetWeeklyForecastAsync(cancellationToken);
        return View(forecast);
    }
}
