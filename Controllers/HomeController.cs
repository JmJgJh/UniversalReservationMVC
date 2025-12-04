using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var resources = await _db.Resources.Take(10).ToListAsync();
            return View(resources);
        }

        public async Task<IActionResult> Resources()
        {
            var resources = await _db.Resources.ToListAsync();
            return View(resources);
        }

        public async Task<IActionResult> Events()
        {
            var events = await _db.Events.Include(e => e.Resource).ToListAsync();
            return View(events);
        }

        public IActionResult Privacy() => View();
    }
}
