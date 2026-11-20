using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.ViewModels;

public class BookingFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Istruttore")]
    public int InstructorId { get; set; }

    [Required]
    [Display(Name = "Durata")]
    public int LessonDurationId { get; set; }

    [Required]
    [Display(Name = "Tipologia")]
    public LessonType LessonType { get; set; } = LessonType.Privata;

    [Required]
    [Display(Name = "Data e ora inizio")]
    public DateTime StartAt { get; set; } = DateTime.Today.AddDays(1).AddHours(10);

    [Range(1, 50)]
    [Display(Name = "Partecipanti")]
    public int Participants { get; set; } = 1;

    [StringLength(500)]
    [Display(Name = "Note")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> Instructors { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Durations { get; set; } = Enumerable.Empty<SelectListItem>();
}
