using System.ComponentModel.DataAnnotations;

namespace SkateParkPrenotazioni.ViewModels;

public class AvailabilityWindowViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Giorno")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Display(Name = "Inizio")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);

    [Required]
    [Display(Name = "Fine")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; } = new(12, 0, 0);

    [Display(Name = "Attiva")]
    public bool IsActive { get; set; } = true;
}
