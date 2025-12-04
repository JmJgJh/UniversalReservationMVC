using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ApplicationDbContext _db;

        public ReservationController(IReservationService reservationService, ApplicationDbContext db)
        {
            _reservationService = reservationService;
            _db = db;
        }

        public async Task<IActionResult> Index(int resourceId)
        {
            var seats = await _reservationService.GetSeatsAsync(resourceId);
            var resource = await _db.Resources.FindAsync(resourceId);
            ViewBag.ResourceId = resourceId;
            ViewBag.ResourceName = resource?.Name ?? "Zasób";
            return View(seats);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int resourceId, int? seatId)
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null) return NotFound();

            var seat = seatId.HasValue ? await _db.Seats.FindAsync(seatId) : null;
            
            var vm = new ReservationCreateViewModel 
            { 
                ResourceId = resourceId, 
                SeatId = seatId, 
                StartTime = DateTime.Now.AddHours(1), 
                EndTime = DateTime.Now.AddHours(2) 
            };
            
            ViewBag.ResourceName = resource.Name;
            ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
            
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var resource = await _db.Resources.FindAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _db.Seats.FindAsync(vm.SeatId) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                return View(vm);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Musisz być zalogowany, aby dokonać rezerwacji.");
                return View(vm);
            }

            var reservation = new Reservation
            {
                ResourceId = vm.ResourceId,
                SeatId = vm.SeatId,
                UserId = userId,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Status = ReservationStatus.Confirmed
            };
            
            try
            {
                await _reservationService.CreateReservation(reservation);
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var resource = await _db.Resources.FindAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _db.Seats.FindAsync(vm.SeatId) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GuestCreate(int resourceId, int? seatId)
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null) return NotFound();

            var seat = seatId.HasValue ? await _db.Seats.FindAsync(seatId) : null;
            
            var vm = new GuestReservationViewModel 
            { 
                ResourceId = resourceId, 
                SeatId = seatId, 
                StartTime = DateTime.Now.AddHours(1), 
                EndTime = DateTime.Now.AddHours(2) 
            };
            
            ViewBag.ResourceName = resource.Name;
            ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestCreate(GuestReservationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var resource = await _db.Resources.FindAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _db.Seats.FindAsync(vm.SeatId) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                return View(vm);
            }

            // Sprawdzenie czy email lub telefon podany
            if (string.IsNullOrWhiteSpace(vm.Email) && string.IsNullOrWhiteSpace(vm.Phone))
            {
                ModelState.AddModelError(string.Empty, "Musisz podać adres e-mail lub numer telefonu.");
                var resource = await _db.Resources.FindAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _db.Seats.FindAsync(vm.SeatId) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                return View(vm);
            }

            var reservation = new Reservation
            {
                ResourceId = vm.ResourceId,
                SeatId = vm.SeatId,
                GuestEmail = vm.Email,
                GuestPhone = vm.Phone,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Status = ReservationStatus.Pending,
                UserId = null
            };
            
            try
            {
                await _reservationService.CreateGuestReservation(reservation);
                
                // Przechowaj informacje do potwierdzenia
                TempData["ReservationConfirmed"] = true;
                TempData["GuestEmail"] = vm.Email;
                TempData["ReservationId"] = reservation.Id;
                
                return RedirectToAction("GuestConfirmation");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var resource = await _db.Resources.FindAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _db.Seats.FindAsync(vm.SeatId) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult GuestConfirmation()
        {
            var email = TempData["GuestEmail"] as string;
            var reservationId = TempData["ReservationId"] as int?;
            
            if (string.IsNullOrEmpty(email) || !reservationId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;
            ViewBag.ReservationId = reservationId;
            
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyReservations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var reservations = await _reservationService.GetReservationsForUser(userId);
            return View(reservations);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                await _reservationService.CancelReservation(id);
                return RedirectToAction(nameof(MyReservations));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(MyReservations));
            }
        }
    }
}
