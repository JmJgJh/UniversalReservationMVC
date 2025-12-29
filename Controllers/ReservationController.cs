using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            IEventService eventService,
            IUnitOfWork unitOfWork,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int resourceId)
        {
            var seats = await _reservationService.GetSeatsAsync(resourceId);
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            ViewBag.ResourceId = resourceId;
            ViewBag.ResourceName = resource?.Name ?? "Zasób";
            return View(seats);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int resourceId, int? seatId, int? eventId)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (resource == null) return NotFound();

            var seat = seatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(seatId.Value) : null;
            var seats = await _unitOfWork.Seats.GetByResourceIdAsync(resourceId);
            
            var vm = new ReservationCreateViewModel 
            { 
                ResourceId = resourceId, 
                SeatId = seatId,
                EventId = eventId,
                StartTime = DateTime.Now.AddHours(1), 
                EndTime = DateTime.Now.AddHours(2) 
            };

            // Prefill from event when provided
            if (eventId.HasValue)
            {
                var ev = await _eventService.GetEventByIdAsync(eventId.Value);
                if (ev != null && ev.ResourceId == resourceId)
                {
                    vm.StartTime = ev.StartTime;
                    vm.EndTime = ev.EndTime;
                }
            }
            
            ViewBag.ResourceName = resource.Name;
            ViewBag.SeatLabel = seat != null ? $"Rząd {seat.X}, Miejsce {seat.Y}" : "Wybierz miejsce";
            ViewBag.Seats = seats.ToList();
            ViewBag.ResourcePrice = resource.Price;
            
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                ViewBag.ResourcePrice = resource?.Price ?? 0;
                return View(vm);
            }

            var userId = User.GetCurrentUserId();
            
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
                Status = ReservationStatus.Confirmed,
                EventId = vm.EventId
            };
            
            // If EventId not provided, try to infer event by time window
            if (!reservation.EventId.HasValue)
            {
                var upcomingEvents = await _eventService.GetUpcomingEventsAsync(vm.ResourceId);
                var relatedEvent = upcomingEvents
                    .Where(e => e.StartTime <= vm.StartTime && e.EndTime >= vm.EndTime)
                    .OrderBy(e => e.StartTime)
                    .FirstOrDefault();
                
                if (relatedEvent != null)
                {
                    reservation.EventId = relatedEvent.Id;
                }
            }
            
            try
            {
                await _reservationService.CreateReservationAsync(reservation);
                _logger.LogInformation("User {UserId} created reservation {ReservationId}", userId, reservation.Id);
                TempData["SuccessMessage"] = "Rezerwacja została utworzona pomyślnie.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation for user {UserId}", userId);
                ModelState.AddModelError(string.Empty, ex.Message);
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
                ViewBag.ResourceName = resource?.Name ?? "Zasób";
                ViewBag.SeatLabel = seat?.Label ?? "Nieznane";
                ViewBag.ResourcePrice = resource?.Price ?? 0;
                return View(vm);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.GetCurrentUserId();
            
            if (reservation.UserId != userId && !User.IsAdmin())
            {
                return Forbid();
            }

            var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
            var seat = reservation.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(reservation.SeatId.Value) : null;

            var vm = new ReservationEditViewModel
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                SeatId = reservation.SeatId,
                EventId = reservation.EventId,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                ResourceName = resource?.Name,
                SeatLabel = seat?.Label
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReservationEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
                vm.ResourceName = resource?.Name;
                vm.SeatLabel = seat?.Label;
                return View(vm);
            }

            var reservation = await _reservationService.GetReservationByIdAsync(vm.Id);
            if (reservation == null) return NotFound();

            var userId = User.GetCurrentUserId();
            
            if (reservation.UserId != userId && !User.IsAdmin())
            {
                return Forbid();
            }

            reservation.StartTime = vm.StartTime;
            reservation.EndTime = vm.EndTime;
            reservation.SeatId = vm.SeatId;
            reservation.EventId = vm.EventId;

            try
            {
                await _reservationService.UpdateReservationAsync(reservation);
                _logger.LogInformation("User {UserId} updated reservation {ReservationId}", userId, reservation.Id);
                TempData["SuccessMessage"] = "Rezerwacja została zaktualizowana pomyślnie.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation {ReservationId}", vm.Id);
                ModelState.AddModelError(string.Empty, ex.Message);
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
                vm.ResourceName = resource?.Name;
                vm.SeatLabel = seat?.Label;
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GuestCreate(int resourceId, int? seatId)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (resource == null) return NotFound();

            var seat = seatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(seatId.Value) : null;
            
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
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
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
                await _reservationService.CreateGuestReservationAsync(reservation);
                _logger.LogInformation("Guest reservation {ReservationId} created with email {Email}", 
                    reservation.Id, vm.Email);
                
                TempData["ReservationConfirmed"] = true;
                TempData["GuestEmail"] = vm.Email;
                TempData["ReservationId"] = reservation.Id;
                
                return RedirectToAction("GuestConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guest reservation");
                ModelState.AddModelError(string.Empty, ex.Message);
                var resource = await _unitOfWork.Resources.GetByIdAsync(vm.ResourceId);
                var seat = vm.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(vm.SeatId.Value) : null;
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
            var userId = User.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var reservations = await _reservationService.GetReservationsForUserAsync(userId);
            return View(reservations);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.GetCurrentUserId();
            
            if (reservation.UserId != userId && !User.IsAdmin())
            {
                return Forbid();
            }

            try
            {
                await _reservationService.CancelReservationAsync(id);
                _logger.LogInformation("User {UserId} cancelled reservation {ReservationId}", userId, id);
                TempData["SuccessMessage"] = "Rezerwacja została anulowana.";
                return RedirectToAction(nameof(MyReservations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(MyReservations));
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int resourceId, DateTime start, DateTime end)
        {
            try
            {
                var reservations = await _unitOfWork.Reservations.GetByResourceIdAsync(resourceId, start, end);
                var reservedSeatIds = reservations
                    .Where(r => r.SeatId.HasValue && r.Status != ReservationStatus.Cancelled)
                    .Select(r => r.SeatId!.Value)
                    .Distinct()
                    .ToList();

                return Json(new { reservedSeatIds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for resource {ResourceId}", resourceId);
                return Json(new { reservedSeatIds = new List<int>() });
            }
        }
    }
}
