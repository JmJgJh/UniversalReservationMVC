using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;
using Microsoft.EntityFrameworkCore;

namespace UniversalReservationMVC.Controllers
{
    public class ResourceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEventService _eventService;
        
        public ResourceController(ApplicationDbContext db, IEventService eventService)
        {
            _db = db;
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var resources = await _db.Resources.AsNoTracking().ToListAsync();
            return View(resources);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) return NotFound();
            
            // Pobierz aktualne wydarzenie dla zasobu
            var currentEvent = await _eventService.GetCurrentEventAsync(id);
            ViewBag.CurrentEvent = currentEvent;
            
            return View(resource);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, int resourceType, string? location, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Nazwa zasobu jest wymagana");
                return View();
            }

            var model = new Resource
            {
                Name = name,
                ResourceType = (ResourceType)resourceType,
                Location = location,
                Description = description
            };

            _db.Resources.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _db.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return View(resource);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, int resourceType, string? location, string? description)
        {
            var resource = await _db.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Nazwa zasobu jest wymagana");
                return View(resource);
            }

            try
            {
                resource.Name = name;
                resource.ResourceType = (ResourceType)resourceType;
                resource.Location = location;
                resource.Description = description;
                
                _db.Resources.Update(resource);
                await _db.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ResourceExists(id)) return NotFound();
                throw;
            }
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _db.Resources.FindAsync(id);
            if (resource != null)
            {
                _db.Resources.Remove(resource);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ResourceExists(int id)
        {
            return await _db.Resources.AnyAsync(e => e.Id == id);
        }
    }
}
