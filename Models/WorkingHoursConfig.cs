namespace UniversalReservationMVC.Models;

public class WorkingHoursConfig
{
    public Dictionary<string, DayHours> Hours { get; set; } = new();
}

public class DayHours
{
    public string? Open { get; set; }
    public string? Close { get; set; }
    public bool IsClosed { get; set; } = false;
}
