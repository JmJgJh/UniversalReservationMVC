using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _db;

        public EventController(IEventService eventService, ApplicationDbContext db)
        {
            _eventService = eventService;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _db.Events.Include(e => e.Resource).ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _db.Events.Include(e => e.Resource).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();
            
            // Detect current user's reservation linked to this event or overlapping time window
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
        public IActionResult Create()
        {
            ViewBag.Resources = _db.Resources.ToList();
            return View();
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (ModelState.IsValid)
            {
                var ev = await _eventService.CreateEventAsync(model);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Resources = _db.Resources.ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            ViewBag.Resources = _db.Resources.ToList();
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
            ViewBag.Resources = _db.Resources.ToList();
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
