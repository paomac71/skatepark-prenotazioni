using SkateParkPrenotazioni.Models;

namespace SkateParkPrenotazioni.ViewModels;

public class BookingListItemViewModel
{
    public int Id { get; set; }
    public DateTime StartAt { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public LessonType LessonType { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
    public int Participants { get; set; }
    public bool IsCancelled { get; set; }
}
