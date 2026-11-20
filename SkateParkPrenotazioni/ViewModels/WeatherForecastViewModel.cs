namespace SkateParkPrenotazioni.ViewModels;

public class WeatherForecastViewModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<DailyWeatherItem> Days { get; set; } = Array.Empty<DailyWeatherItem>();
}

public class DailyWeatherItem
{
    public DateOnly Date { get; set; }
    public double TemperatureMaxC { get; set; }
    public double TemperatureMinC { get; set; }
    public double PrecipitationMm { get; set; }
    public int WeatherCode { get; set; }
    public string Description { get; set; } = string.Empty;
}
