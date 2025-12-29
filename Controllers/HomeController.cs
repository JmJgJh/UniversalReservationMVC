using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.ViewModels;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        [ResponseCache(Duration = 60, VaryByQueryKeys = new string[] { })]  
        public async Task<IActionResult> Index()
        {
            // If user is authenticated, redirect to dashboard
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
            {
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.Now;
            
            var viewModel = new UserDashboardViewModel
            {
                UpcomingReservations = await _db.Reservations
                    .AsNoTracking()
                    .Include(r => r.Resource)
                    .Include(r => r.Event)
                    .Include(r => r.Seat)
                    .Where(r => r.UserId == userId && r.StartTime > now)
                    .OrderBy(r => r.StartTime)
                    .Take(5)
                    .ToListAsync(),
                
                PastReservations = await _db.Reservations
                    .AsNoTracking()
                    .Include(r => r.Resource)
                    .Include(r => r.Event)
                    .Where(r => r.UserId == userId && r.EndTime < now)
                    .OrderByDescending(r => r.EndTime)
                    .Take(5)
                    .ToListAsync(),
                
                UpcomingEvents = await _db.Events
                    .AsNoTracking()
                    .Include(e => e.Resource)
                    .Where(e => e.StartTime > now)
                    .OrderBy(e => e.StartTime)
                    .Take(6)
                    .ToListAsync(),
                
                TotalReservationsCount = await _db.Reservations
                    .Where(r => r.UserId == userId)
                    .CountAsync(),
                
                ActiveReservationsCount = await _db.Reservations
                    .Where(r => r.UserId == userId && r.Status == ReservationStatus.Confirmed)
                    .CountAsync(),
                
                CompletedReservationsCount = await _db.Reservations
                    .Where(r => r.UserId == userId && r.Status == ReservationStatus.Completed)
                    .CountAsync(),
                
                TotalSpent = await _db.Payments
                    .Where(p => p.Reservation != null && p.Reservation.UserId == userId && p.Status == PaymentStatus.Succeeded)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                MyCompanies = await _db.CompanyMembers
                    .Where(cm => cm.UserId == userId && cm.IsActive)
                    .Include(cm => cm.Company)
                    .Select(cm => cm.Company)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Resources()
        {
            var resources = await _db.Resources.AsNoTracking().ToListAsync();
            return View(resources);
        }

        public async Task<IActionResult> Events()
        {
            var events = await _db.Events.AsNoTracking().Include(e => e.Resource).ToListAsync();
            return View(events);
        }

        public IActionResult Privacy() => View();
    }
}
