namespace SkateParkPrenotazioni.ViewModels;

public class InstructorListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
}