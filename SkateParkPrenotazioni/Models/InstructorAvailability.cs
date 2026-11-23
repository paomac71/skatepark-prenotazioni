using System.ComponentModel.DataAnnotations;

namespace SkateParkPrenotazioni.Models;

/// <summary>
/// Finestra ricorrente settimanale in cui l'istruttore è disponibile per lezioni.
/// </summary>
public class InstructorAvailability
{
    public int Id { get; set; }

    [Required]
    public int InstructorId { get; set; }

    public Instructor? Instructor { get; set; }

    [Required]
    [Display(Name = "Giorno")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Display(Name = "Inizio")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Display(Name = "Fine")]
    public TimeSpan EndTime { get; set; }

    public bool IsActive { get; set; } = true;
}
