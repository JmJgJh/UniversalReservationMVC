using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ApplicationDbContext _db;

        public TicketController(ITicketService ticketService, ApplicationDbContext db)
        {
            _ticketService = ticketService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Buy(int reservationId)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Resource)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            // Prevent buying a ticket if already purchased
            var purchased = await _db.Tickets.AnyAsync(t => t.ReservationId == reservationId && t.Status == TicketStatus.Purchased);
            if (purchased)
            {
                // Redirect to user's tickets; optionally show message 
                return RedirectToAction(nameof(MyTickets));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ViewBag.ReservationId = reservationId;
            ViewBag.ResourceName = reservation.Resource?.Name ?? "Zasób";
            ViewBag.SeatLabel = reservation.Seat?.Label ?? "Nieznane";
            ViewBag.ReservationStartTime = reservation.StartTime;
            ViewBag.ReservationEndTime = reservation.EndTime;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int reservationId, decimal price)
        {
            try
            {
                // Prevent duplicate purchase
                var purchased = await _db.Tickets.AnyAsync(t => t.ReservationId == reservationId && t.Status == TicketStatus.Purchased);
                if (purchased)
                {
                    ModelState.AddModelError(string.Empty, "Bilet dla tej rezerwacji został już zakupiony.");
                    ViewBag.ReservationId = reservationId;
                    return View();
                }
                var ticket = await _ticketService.BuyTicketAsync(reservationId, price);
                return RedirectToAction(nameof(MyTickets));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ReservationId = reservationId;
                return View();
            }
        }

        public async Task<IActionResult> MyTickets()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _ticketService.GetTicketsForUserAsync(userId);
            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int ticketId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Sprawdź czy to bilet użytkownika
            if (ticket.Reservation?.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                await _ticketService.CancelTicketAsync(ticketId);
                return RedirectToAction(nameof(MyTickets));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(MyTickets));
            }
        }
    }
}
