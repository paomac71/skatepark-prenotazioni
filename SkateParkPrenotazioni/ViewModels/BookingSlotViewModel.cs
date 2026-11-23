namespace SkateParkPrenotazioni.ViewModels;

public class BookingSlotViewModel
{
    public DateTime StartAt { get; set; }
    public string Label { get; set; } = string.Empty;
    public int RemainingCapacity { get; set; }
}

public class BookingDayAvailabilityViewModel
{
    public string Date { get; set; } = string.Empty;
    public bool HasSlots { get; set; }
}

public class BookingCalendarViewModel
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public BookingFormViewModel Form { get; set; } = new();
    public int Year { get; set; }
    public int Month { get; set; }
    public IReadOnlyList<string> AvailableDates { get; set; } = Array.Empty<string>();
}
