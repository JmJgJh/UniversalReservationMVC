using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalReservationMVC.Models
{
    /// <summary>
    /// Defines a recurrence pattern for events
    /// </summary>
    public class RecurrencePattern
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        /// <summary>
        /// Type of recurrence: Daily, Weekly, Monthly
        /// </summary>
        [Required]
        public RecurrenceType Type { get; set; }

        /// <summary>
        /// Interval between occurrences (e.g., every 2 weeks)
        /// </summary>
        [Range(1, 365)]
        public int Interval { get; set; } = 1;

        /// <summary>
        /// For weekly recurrence: which days (JSON array of day numbers, 0=Sunday)
        /// Example: [1,3,5] for Mon, Wed, Fri
        /// </summary>
        public string? DaysOfWeek { get; set; }

        /// <summary>
        /// For monthly recurrence: day of month (1-31) or null for same day as start
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// End date for recurrence (null = no end)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Maximum number of occurrences (null = unlimited)
        /// </summary>
        public int? MaxOccurrences { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum RecurrenceType
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }
}
