using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    public class SeatController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SeatController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetSeatMap(int resourceId)
        {
            var seats = await _db.Seats.Where(s => s.ResourceId == resourceId).ToListAsync();
            ViewBag.ResourceId = resourceId;
            return View(seats);
        }
    }
}
