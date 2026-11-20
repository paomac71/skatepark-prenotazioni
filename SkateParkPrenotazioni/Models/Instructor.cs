using System.ComponentModel.DataAnnotations;

namespace SkateParkPrenotazioni.Models;

public class Instructor
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    [Display(Name = "Bio")]
    public string Bio { get; set; } = string.Empty;

    [Range(1, 50)]
    [Display(Name = "Capienza massima")]
    public int MaxParticipants { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
