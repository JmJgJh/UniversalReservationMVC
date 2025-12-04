using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using System.Globalization;

namespace UniversalReservationMVC.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CalendarController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Displays monthly calendar view for a specific resource
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int resourceId, int? year, int? month)
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null)
            {
                return NotFound($"Zasób o ID {resourceId} nie został znaleziony.");
            }

            // Default to current month/year if not specified
            var now = DateTime.UtcNow;
            year = year ?? now.Year;
            month = month ?? now.Month;

            // Validate month/year
            if (month < 1 || month > 12)
                month = 1;

            ViewBag.ResourceId = resourceId;
            ViewBag.ResourceName = resource.Name;
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Value);

            // Calculate previous/next month for navigation
            var currentDate = new DateTime(year.Value, month.Value, 1);
            var previousMonth = currentDate.AddMonths(-1);
            var nextMonth = currentDate.AddMonths(1);

            ViewBag.PreviousYear = previousMonth.Year;
            ViewBag.PreviousMonth = previousMonth.Month;
            ViewBag.NextYear = nextMonth.Year;
            ViewBag.NextMonth = nextMonth.Month;

            return View();
        }

        /// <summary>
        /// Returns reservations and events in JSON format for a specific month
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReservations(int resourceId, int year, int month)
        {
            try
            {
                // Get first and last day of the month
                var firstDay = new DateTime(year, month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                // Get all events for this resource in the month
                var events = await _db.Events
                    .Where(e => e.ResourceId == resourceId &&
                                e.StartTime.Date <= lastDay &&
                                e.EndTime.Date >= firstDay.Date)
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        type = "event",
                        date = e.StartTime.Date,
                        startDate = e.StartTime,
                        endDate = e.EndTime,
                        color = "#28a745", // Green for events
                        className = "event-block"
                    })
                    .ToListAsync();

                // Get all reservations for this resource in the month
                var reservations = await _db.Reservations
                    .Where(r => r.ResourceId == resourceId &&
                                r.StartTime.Date <= lastDay &&
                                r.EndTime.Date >= firstDay.Date)
                    .Select(r => new
                    {
                        id = r.Id,
                        title = (r.UserId != null ? "Rezerwacja: " : "Rezerwacja gościa: ") + 
                                (r.User.FirstName + " " + r.User.LastName != null ? 
                                 r.User.FirstName + " " + r.User.LastName : 
                                 (r.GuestEmail != null ? r.GuestEmail : "Gość")),
                        type = "reservation",
                        date = r.StartTime.Date,
                        startDate = r.StartTime,
                        endDate = r.EndTime,
                        status = r.Status.ToString(),
                        color = r.Status == ReservationStatus.Confirmed ? "#007bff" :
                                r.Status == ReservationStatus.Cancelled ? "#dc3545" : "#ffc107",
                        className = "reservation-block " + r.Status.ToString().ToLower(),
                        reservationId = r.Id
                    })
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    events = events,
                    reservations = reservations,
                    daysInMonth = DateTime.DaysInMonth(year, month),
                    firstDayOfWeek = (int)firstDay.DayOfWeek
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
