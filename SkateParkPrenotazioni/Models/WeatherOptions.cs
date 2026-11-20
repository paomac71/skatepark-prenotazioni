namespace SkateParkPrenotazioni.Models;

public class WeatherOptions
{
    public const string SectionName = "Weather";

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = "Europe/Rome";
}
