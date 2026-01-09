using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.ViewModels;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        [ResponseCache(Duration = 60, VaryByQueryKeys = new string[] { })]
        public async Task<IActionResult> Index()
        {
            // Je�li u�ytkownik jest zalogowany, przekieruj na dashboard
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var resources = await _db.Resources.AsNoTracking().Take(10).ToListAsync();
            return View(resources);
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return RedirectToAction(nameof(Index));

            var now = DateTime.Now;

            // Pobierz płatności do pamięci przed obliczeniem sumy (SQLite nie obsługuje Sum na decimal)
            var payments = await _db.Payments
                .Where(p => p.Reservation != null && p.Reservation.UserId == userId && p.Status == PaymentStatus.Succeeded)
                .Select(p => p.Amount)
                .ToListAsync();

            var viewModel = new UserDashboardViewModel
            {
                // Statystyki
                TotalReservationsCount = await _db.Reservations.CountAsync(r => r.UserId == userId),
                ActiveReservationsCount = await _db.Reservations.CountAsync(r => r.UserId == userId && r.Status == ReservationStatus.Confirmed),
                CompletedReservationsCount = await _db.Reservations.CountAsync(r => r.UserId == userId && r.Status == ReservationStatus.Completed),
                TotalSpent = payments.Any() ? payments.Sum() : 0,

                // Nadchodz�ce rezerwacje
                UpcomingReservations = await _db.Reservations
                                                .AsNoTracking()
                                                .Include(r => r.Resource)
                                                .Include(r => r.Event)
                                                .Include(r => r.Seat)
                                                .Where(r => r.UserId == userId && r.StartTime > now)
                                                .OrderBy(r => r.StartTime)
                                                .Take(5)
                                                .ToListAsync(),

                // Przesz�e rezerwacje
                PastReservations = await _db.Reservations
                                            .AsNoTracking()
                                            .Include(r => r.Resource)
                                            .Include(r => r.Event)
                                            .Where(r => r.UserId == userId && r.EndTime < now)
                                            .OrderByDescending(r => r.EndTime)
                                            .Take(5)
                                            .ToListAsync(),

                // Nadchodz�ce wydarzenia
                UpcomingEvents = await _db.Events
                                          .AsNoTracking()
                                          .Include(e => e.Resource)
                                          .Where(e => e.StartTime > now)
                                          .OrderBy(e => e.StartTime)
                                          .Take(6)
                                          .ToListAsync(),

                // Firmy u�ytkownika
                MyCompanies = await _db.CompanyMembers
                                       .Where(cm => cm.UserId == userId && cm.IsActive)
                                       .Include(cm => cm.Company)
                                       .Select(cm => cm.Company)
                                       .ToListAsync(),

                // Wybrane miejsca
                SelectedSeats = await _db.Reservations
                                         .Where(r => r.UserId == userId && r.Status == ReservationStatus.Confirmed && r.Seat != null)
                                         .Include(r => r.Resource)
                                         .Include(r => r.Seat)
                                         .Select(r => new SelectedSeatViewModel
                                         {
                                             ResourceName = r.Resource!.Name,
                                             X = r.Seat!.X,
                                             Y = r.Seat!.Y
                                         })
                                         .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Resources()
        {
            return RedirectToAction("Index", "Resource");
        }

        public async Task<IActionResult> Events()
        {
            var events = await _db.Events.AsNoTracking().Include(e => e.Resource).ToListAsync();
            return View(events);
        }

        public IActionResult Privacy() => View();
    }
}
