using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class RecurrenceService : IRecurrenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecurrenceService> _logger;

        public RecurrenceService(ApplicationDbContext context, ILogger<RecurrenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Event>> GenerateOccurrencesAsync(Event templateEvent, RecurrencePattern pattern, DateTime? untilDate = null)
        {
            // Yield once to keep the method truly asynchronous and avoid CS1998
            await Task.Yield();

            var occurrences = new List<Event>();
            var currentDate = templateEvent.StartTime.Date;
            var duration = templateEvent.EndTime - templateEvent.StartTime;
            var endDate = untilDate ?? pattern.EndDate ?? currentDate.AddYears(1); // Default 1 year ahead
            var count = 0;
            var maxOccurrences = pattern.MaxOccurrences ?? 365; // Safety limit

            try
            {
                while (currentDate <= endDate && count < maxOccurrences)
                {
                    if (IsOccurrenceDate(currentDate, pattern, templateEvent.StartTime.Date))
                    {
                        var occurrence = new Event
                        {
                            Title = templateEvent.Title,
                            Description = templateEvent.Description,
                            ResourceId = templateEvent.ResourceId,
                            StartTime = currentDate.Add(templateEvent.StartTime.TimeOfDay),
                            EndTime = currentDate.Add(templateEvent.StartTime.TimeOfDay).Add(duration),
                            ParentEventId = templateEvent.Id
                        };

                        occurrences.Add(occurrence);
                        count++;
                    }

                    currentDate = GetNextDate(currentDate, pattern);
                }

                _logger.LogInformation("Generated {Count} occurrences for event {EventId}", occurrences.Count, templateEvent.Id);
                return occurrences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating occurrences for event {EventId}", templateEvent.Id);
                throw;
            }
        }

        public async Task<List<Event>> GetOccurrencesAsync(int eventId, DateTime startDate, DateTime endDate)
        {
            var templateEvent = await _context.Events
                .AsNoTracking()
                .Include(e => e.RecurrencePattern)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (templateEvent?.RecurrencePattern == null)
            {
                return new List<Event> { templateEvent! };
            }

            // Get existing occurrences from DB
            var existingOccurrences = await _context.Events
                .Where(e => e.ParentEventId == eventId && e.StartTime >= startDate && e.StartTime <= endDate)
                .ToListAsync();

            // If no occurrences exist, generate them on-the-fly
            if (!existingOccurrences.Any())
            {
                return await GenerateOccurrencesAsync(templateEvent, templateEvent.RecurrencePattern, endDate);
            }

            return existingOccurrences;
        }

        public bool IsOccurrenceDate(DateTime date, RecurrencePattern pattern, DateTime startDate)
        {
            if (date < startDate) return false;
            if (pattern.EndDate.HasValue && date > pattern.EndDate.Value) return false;

            var daysDiff = (date - startDate).Days;

            return pattern.Type switch
            {
                RecurrenceType.Daily => daysDiff % pattern.Interval == 0,
                RecurrenceType.Weekly => IsWeeklyOccurrence(date, pattern, startDate),
                RecurrenceType.Monthly => IsMonthlyOccurrence(date, pattern, startDate),
                _ => false
            };
        }

        private bool IsWeeklyOccurrence(DateTime date, RecurrencePattern pattern, DateTime startDate)
        {
            var weeksDiff = (date - startDate).Days / 7;
            if (weeksDiff % pattern.Interval != 0) return false;

            if (string.IsNullOrEmpty(pattern.DaysOfWeek))
            {
                return date.DayOfWeek == startDate.DayOfWeek;
            }

            try
            {
                var daysOfWeek = JsonSerializer.Deserialize<List<int>>(pattern.DaysOfWeek);
                return daysOfWeek?.Contains((int)date.DayOfWeek) ?? false;
            }
            catch
            {
                return date.DayOfWeek == startDate.DayOfWeek;
            }
        }

        private bool IsMonthlyOccurrence(DateTime date, RecurrencePattern pattern, DateTime startDate)
        {
            var monthsDiff = (date.Year - startDate.Year) * 12 + date.Month - startDate.Month;
            if (monthsDiff % pattern.Interval != 0) return false;

            var targetDay = pattern.DayOfMonth ?? startDate.Day;
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var actualDay = Math.Min(targetDay, daysInMonth);

            return date.Day == actualDay;
        }

        private DateTime GetNextDate(DateTime currentDate, RecurrencePattern pattern)
        {
            return pattern.Type switch
            {
                RecurrenceType.Daily => currentDate.AddDays(pattern.Interval),
                RecurrenceType.Weekly => currentDate.AddDays(7 * pattern.Interval),
                RecurrenceType.Monthly => currentDate.AddMonths(pattern.Interval),
                _ => currentDate.AddDays(1)
            };
        }
    }
}
