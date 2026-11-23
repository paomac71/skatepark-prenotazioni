using System.ComponentModel.DataAnnotations;

namespace SkateParkPrenotazioni.ViewModels;

public class InstructorEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La bio è obbligatoria.")]
    [StringLength(500)]
    [Display(Name = "Bio")]
    public string Bio { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "La capienza deve essere tra 1 e 50.")]
    [Display(Name = "Capienza massima")]
    public int MaxParticipants { get; set; } = 1;

    [Display(Name = "Attivo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Disponibilità settimanale")]
    public List<AvailabilityWindowViewModel> Availabilities { get; set; } = new();
}
