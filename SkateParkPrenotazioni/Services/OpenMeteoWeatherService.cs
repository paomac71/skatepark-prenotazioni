using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SkateParkPrenotazioni.Models;
using SkateParkPrenotazioni.ViewModels;

namespace SkateParkPrenotazioni.Services;

public class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherOptions _options;
    private readonly ILogger<OpenMeteoWeatherService> _logger;

    public OpenMeteoWeatherService(
        HttpClient httpClient,
        IOptions<WeatherOptions> options,
        ILogger<OpenMeteoWeatherService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherForecastViewModel> GetWeeklyForecastAsync(CancellationToken cancellationToken = default)
    {
        var result = new WeatherForecastViewModel
        {
            Latitude = _options.Latitude,
            Longitude = _options.Longitude,
            Timezone = _options.Timezone
        };

        try
        {
            var lat = _options.Latitude.ToString(CultureInfo.InvariantCulture);
            var lon = _options.Longitude.ToString(CultureInfo.InvariantCulture);
            var url =
                $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                $"&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum" +
                $"&timezone={Uri.EscapeDataString(_options.Timezone)}&forecast_days=7";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var daily = doc.RootElement.GetProperty("daily");

            var times = daily.GetProperty("time").EnumerateArray().Select(x => x.GetString()!).ToList();
            var codes = daily.GetProperty("weather_code").EnumerateArray().Select(x => x.GetInt32()).ToList();
            var maxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(x => x.GetDouble()).ToList();
            var minTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(x => x.GetDouble()).ToList();
            var precip = daily.GetProperty("precipitation_sum").EnumerateArray().Select(x => x.GetDouble()).ToList();

            var days = new List<DailyWeatherItem>();
            for (var i = 0; i < times.Count; i++)
            {
                var code = codes[i];
                days.Add(new DailyWeatherItem
                {
                    Date = DateOnly.Parse(times[i], CultureInfo.InvariantCulture),
                    WeatherCode = code,
                    TemperatureMaxC = maxTemps[i],
                    TemperatureMinC = minTemps[i],
                    PrecipitationMm = precip[i],
                    Description = DescribeWeatherCode(code)
                });
            }

            result.Days = days;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile recuperare le previsioni Open-Meteo.");
            result.ErrorMessage = "Previsioni non disponibili al momento. Riprova più tardi.";
        }

        return result;
    }

    private static string DescribeWeatherCode(int code) => code switch
    {
        0 => "Sereno",
        1 or 2 => "Prevalentemente sereno",
        3 => "Nuvoloso",
        45 or 48 => "Nebbia",
        51 or 53 or 55 => "Pioviggine",
        61 or 63 or 65 => "Pioggia",
        71 or 73 or 75 => "Neve",
        80 or 81 or 82 => "Rovesci",
        95 or 96 or 99 => "Temporale",
        _ => "Variabile"
    };
}
