using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Controllers;

public class WeatherController : Controller
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var forecast = await _weatherService.GetWeeklyForecastAsync(cancellationToken);
        return View(forecast);
    }
}
