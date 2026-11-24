using SkateParkPrenotazioni.Models;

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

    /// <summary>
    /// Id della prenotazione attiva dell'utente per quel giorno (stesso istruttore), se presente.
    /// </summary>
    public int? ExistingBookingId { get; set; }
}

public class UserDayBookingViewModel
{
    public int Id { get; set; }
    public DateTime StartAt { get; set; }
    public int Participants { get; set; }
    public LessonType LessonType { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
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
