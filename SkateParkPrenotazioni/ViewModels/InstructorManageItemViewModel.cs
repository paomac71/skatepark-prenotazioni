namespace SkateParkPrenotazioni.ViewModels;

public class InstructorManageItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public bool IsActive { get; set; }
    public int AvailabilityWindows { get; set; }
}
