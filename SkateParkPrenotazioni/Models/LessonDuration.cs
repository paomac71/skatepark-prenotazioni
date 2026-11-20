using System.ComponentModel.DataAnnotations;

namespace SkateParkPrenotazioni.Models;

public class LessonDuration
{
    public int Id { get; set; }

    [Range(15, 240)]
    [Display(Name = "Minuti")]
    public int Minutes { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Etichetta")]
    public string Label { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
