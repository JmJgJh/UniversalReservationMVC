using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Extensions;

namespace UniversalReservationMVC.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IRecurrenceService _recurrenceService;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EventController> _logger;

        public EventController(
            IEventService eventService, 
            IRecurrenceService recurrenceService,
            ApplicationDbContext db,
            ILogger<EventController> logger)
        {
            _eventService = eventService;
            _recurrenceService = recurrenceService;
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _db.Events.AsNoTracking().Include(e => e.Resource).ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _db.Events
                .AsNoTracking()
                .Include(e => e.Resource)
                    .ThenInclude(r => r.Company)
                .Include(e => e.RecurrencePattern)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (ev == null) return NotFound();
            
            // Calculate available seats
            var totalSeats = await _db.Seats
                .Where(s => s.ResourceId == ev.ResourceId)
                .CountAsync();
            
            var reservedSeats = await _db.Reservations
                .Where(r => r.ResourceId == ev.ResourceId
                           && r.Status == ReservationStatus.Confirmed
                           && r.StartTime < ev.EndTime
                           && r.EndTime > ev.StartTime
                           && r.SeatId.HasValue)
                .Select(r => r.SeatId)
                .Distinct()
                .CountAsync();
            
            ViewBag.TotalSeats = totalSeats;
            ViewBag.AvailableSeats = totalSeats - reservedSeats;
            
            // Detect current user's reservation linked to this event or overlapping time window
            var userId = User.GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var userReservation = await _db.Reservations
                    .Where(r => r.UserId == userId
                                && r.ResourceId == ev.ResourceId
                                && r.Status == ReservationStatus.Confirmed
                                && (
                                    (r.EventId.HasValue && r.EventId == ev.Id) ||
                                    (r.StartTime < ev.EndTime && r.EndTime > ev.StartTime)
                                ))
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (userReservation != null)
                {
                    // Show buy button only if no purchased ticket exists for this reservation
                    bool alreadyPurchased = await _db.Tickets.AnyAsync(t => t.ReservationId == userReservation.Id && t.Status == TicketStatus.Purchased);
                    if (!alreadyPurchased)
                    {
                        ViewBag.UserReservationId = userReservation.Id;
                    }
                }
            }
            return View(ev);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Resources = await _db.Resources.ToListAsync();
            return View();
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Event model, 
            int RecurrenceType, 
            int? RecurrenceInterval, 
            List<int>? DaysOfWeek,
            int? DayOfMonth,
            DateTime? RecurrenceEndDate,
            int? MaxOccurrences)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var ev = await _eventService.CreateEventAsync(model);

                    // If recurrence pattern is specified, create it
                    if (RecurrenceType > 0 && ev != null)
                    {
                        var pattern = new RecurrencePattern
                        {
                            EventId = ev.Id,
                            Type = (RecurrenceType)RecurrenceType,
                            Interval = RecurrenceInterval ?? 1,
                            DaysOfWeek = DaysOfWeek != null && DaysOfWeek.Any() 
                                ? System.Text.Json.JsonSerializer.Serialize(DaysOfWeek) 
                                : null,
                            DayOfMonth = DayOfMonth,
                            EndDate = RecurrenceEndDate,
                            MaxOccurrences = MaxOccurrences
                        };

                        _db.RecurrencePatterns.Add(pattern);
                        await _db.SaveChangesAsync();

                        // Generate occurrences (save to DB or generate on-the-fly)
                        var occurrences = await _recurrenceService.GenerateOccurrencesAsync(ev, pattern);
                        _db.Events.AddRange(occurrences);
                        await _db.SaveChangesAsync();

                        _logger.LogInformation("Created recurring event {EventId} with {Count} occurrences", 
                            ev.Id, occurrences.Count);
                    }

                    TempData["SuccessMessage"] = RecurrenceType > 0 
                        ? "Wydarzenie cykliczne zostało utworzone!" 
                        : "Wydarzenie zostało utworzone!";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating event with recurrence");
                    ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia wydarzenia");
                }
            }
            
            ViewBag.Resources = await _db.Resources.ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            ViewBag.Resources = await _db.Resources.ToListAsync();
            return View(ev);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Events.Update(model);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EventExists(id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Resources = await _db.Resources.ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev != null)
            {
                _db.Events.Remove(ev);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EventExists(int id)
        {
            return await _db.Events.AnyAsync(e => e.Id == id);
        }
    }
}
