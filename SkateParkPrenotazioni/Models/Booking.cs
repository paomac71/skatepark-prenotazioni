using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SkateParkPrenotazioni.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public IdentityUser? User { get; set; }

    [Required]
    [Display(Name = "Istruttore")]
    public int InstructorId { get; set; }

    public Instructor? Instructor { get; set; }

    [Required]
    [Display(Name = "Durata")]
    public int LessonDurationId { get; set; }

    public LessonDuration? LessonDuration { get; set; }

    [Required]
    [Display(Name = "Tipologia")]
    public LessonType LessonType { get; set; } = LessonType.Privata;

    [Required]
    [Display(Name = "Inizio")]
    public DateTime StartAt { get; set; }

    [Range(1, 50)]
    [Display(Name = "Partecipanti")]
    public int Participants { get; set; } = 1;

    [Display(Name = "Annullata")]
    public bool IsCancelled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    [Display(Name = "Note")]
    public string? Notes { get; set; }
}
