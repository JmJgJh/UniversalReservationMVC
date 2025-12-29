using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using System.Security.Claims;

namespace UniversalReservationMVC.ViewComponents
{
    public class CompanyNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CompanyNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Content(string.Empty);
            }

            var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Content(string.Empty);
            }

            // Check if user owns a company
            var hasCompany = await _context.Companies
                .AnyAsync(c => c.OwnerId == userId);

            if (hasCompany)
            {
                return View("Default", true);
            }

            return Content(string.Empty);
        }
    }
}
