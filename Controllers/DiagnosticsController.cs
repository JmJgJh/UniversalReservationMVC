using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Extensions;
using Microsoft.AspNetCore.Identity;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Controllers
{
    [Authorize(Roles = "Owner")]
    public class DiagnosticsController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiagnosticsController(
            ICompanyService companyService,
            UserManager<ApplicationUser> userManager)
        {
            _companyService = companyService;
            _userManager = userManager;
        }

        public async Task<IActionResult> CheckCompany()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Content("User ID is null");

            var user = await _userManager.FindByIdAsync(userId);
            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            var roles = await _userManager.GetRolesAsync(user!);

            var result = $@"
User ID: {userId}
User Email: {user?.Email}
User Roles: {string.Join(", ", roles)}

Company Found: {company != null}
Company ID: {company?.Id}
Company Name: '{company?.Name}'
Company OwnerId: {company?.OwnerId}
Company CreatedAt: {company?.CreatedAt}
";

            return Content(result, "text/plain");
        }
    }
}
