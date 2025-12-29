using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using UniversalReservationMVC.Hubs;
using UniversalReservationMVC.Services;
using System.Text.Json;
using UniversalReservationMVC.Extensions;

namespace UniversalReservationMVC.Controllers
{
    public class SeatController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISeatHoldService _holdService;
        private readonly IHubContext<SeatHub> _hub;
        public SeatController(ApplicationDbContext db, ISeatHoldService holdService, IHubContext<SeatHub> hub)
        {
            _db = db;
            _holdService = holdService;
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> GetSeatMap(int resourceId, int? eventId)
        {
            var seats = await _db.Seats.AsNoTracking().Where(s => s.ResourceId == resourceId).ToListAsync();
            var resource = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == resourceId);
            ViewBag.ResourceId = resourceId;
            ViewBag.ResourceName = resource?.Name;

            if (eventId.HasValue)
            {
                var ev = await _db.Events.FindAsync(eventId.Value);
                if (ev != null && ev.ResourceId == resourceId)
                {
                    ViewBag.EventId = ev.Id;
                    ViewBag.EventStart = ev.StartTime;
                    ViewBag.EventEnd = ev.EndTime;
                }
            }
            return View(seats);
        }

        // JSON: Returns the seat grid for a resource
        [HttpGet]
        public async Task<IActionResult> MapJson(int resourceId)
        {
            var seats = await _db.Seats
                .Where(s => s.ResourceId == resourceId)
                .OrderBy(s => s.Y).ThenBy(s => s.X)
                .Select(s => new {
                    id = s.Id,
                    resourceId = s.ResourceId,
                    x = s.X,
                    y = s.Y,
                    row = s.Row,
                    column = s.Column,
                    label = s.Label
                })
                .ToListAsync();
            return Ok(seats);
        }

        // JSON: Returns occupied seat IDs within a time window for a resource
        [HttpGet]
        public async Task<IActionResult> Availability(int resourceId, DateTime start, DateTime end)
        {
            if (start >= end)
            {
                return BadRequest(new { error = "Zakres czasu jest nieprawidłowy." });
            }

            var occupiedSeatIds = await _db.Reservations
                .Where(r => r.ResourceId == resourceId
                            && r.SeatId != null
                            && r.Status != Models.ReservationStatus.Cancelled
                            && r.StartTime < end && r.EndTime > start)
                .Select(r => r.SeatId!.Value)
                .Distinct()
                .ToListAsync();

            // Include temporarily held seats
            var heldSeatIds = _holdService.GetOccupiedByHold(resourceId, start, end);
            var unionIds = occupiedSeatIds.Union(heldSeatIds).Distinct().ToList();

            return Ok(new { resourceId, start, end, occupiedSeatIds = unionIds });
        }

        // POST: Place a temporary hold on a seat for a given time window
        [HttpPost]
        public async Task<IActionResult> Hold(int resourceId, int seatId, DateTime start, DateTime end, int seconds = 90)
        {
            if (start >= end) return BadRequest(new { error = "Zakres czasu jest nieprawidłowy." });

            // Check seat exists and belongs to resource
            var seat = await _db.Seats.FindAsync(seatId);
            if (seat == null || seat.ResourceId != resourceId)
            {
                return NotFound(new { error = "Miejsce nie znalezione." });
            }

            // Check not already reserved in the window
            var conflict = await _db.Reservations.AnyAsync(r => r.ResourceId == resourceId
                && r.SeatId == seatId
                && r.Status != Models.ReservationStatus.Cancelled
                && r.StartTime < end && r.EndTime > start);
            if (conflict)
            {
                return Conflict(new { error = "To miejsce jest już zajęte w tym czasie." });
            }

            // Holder key: prefer userId, else use session id
            var holderKey = User?.GetCurrentUserId()
                ?? HttpContext.Session.Id;

            var ok = _holdService.TryHold(resourceId, seatId, start, end, holderKey, TimeSpan.FromSeconds(seconds));
            if (!ok)
            {
                return Conflict(new { error = "Nie udało się zablokować miejsca (konflikt lub błąd)." });
            }
            await _hub.Clients.Group(resourceId.ToString()).SendAsync("SeatHoldPlaced", new { resourceId, seatId, start, end });
            return Ok(new { success = true, resourceId, seatId, start, end, expiresAt = DateTime.UtcNow.AddSeconds(seconds) });
        }

        // DELETE: Release a temporary hold
        [HttpDelete]
        public async Task<IActionResult> ReleaseHold(int resourceId, int seatId)
        {
            var holderKey = User?.GetCurrentUserId()
                ?? HttpContext.Session.Id;
            var released = _holdService.Release(resourceId, seatId, holderKey);
            if (released)
            {
                await _hub.Clients.Group(resourceId.ToString()).SendAsync("SeatHoldReleased", new { resourceId, seatId });
            }
            return Ok(new { released });
        }
    }
}
