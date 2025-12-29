using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface IRecurrenceService
    {
        /// <summary>
        /// Generate occurrences for a recurring event based on its pattern
        /// </summary>
        Task<List<Event>> GenerateOccurrencesAsync(Event templateEvent, RecurrencePattern pattern, DateTime? untilDate = null);

        /// <summary>
        /// Get all occurrences of a recurring event within a date range
        /// </summary>
        Task<List<Event>> GetOccurrencesAsync(int eventId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Check if a date matches the recurrence pattern
        /// </summary>
        bool IsOccurrenceDate(DateTime date, RecurrencePattern pattern, DateTime startDate);
    }
}
