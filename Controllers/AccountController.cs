using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Common;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace UniversalReservationMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyService _companyService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ICompanyService companyService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _companyService = companyService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginError"] = null; // Pasek błędu ukryty
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Pobierz użytkownika
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        // Zwykły użytkownik -> dashboard
                        if (user.Role == UserRole.User)
                            return RedirectToAction("Dashboard", "Account");

                        // Właściciel -> panel firmy
                        if (user.Role == UserRole.Owner)
                            return RedirectToAction("Dashboard", "Company");
                    }

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                // Błąd logowania
                ViewData["LoginError"] = "Błędne dane logowania";
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            // Przykładowe dane do ViewModel – w realnym projekcie pobierasz z bazy
            var model = new UserDashboardViewModel
            {
                TotalReservationsCount = 5, // np. _reservationService.GetTotalCount(user.Id)
                ActiveReservationsCount = 2,
                CompletedReservationsCount = 3,
                UpcomingReservations = new List<Reservation>
                {
                    new Reservation { Resource = new Resource { Name = "Sala A" }, StartTime = DateTime.Now.AddHours(2) },
                    new Reservation { Resource = new Resource { Name = "Sala B" }, StartTime = DateTime.Now.AddDays(1) }
                }
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.AccountType == "owner" && string.IsNullOrWhiteSpace(model.CompanyName))
            {
                ModelState.AddModelError("CompanyName", "Nazwa firmy jest wymagana dla właścicieli firmy.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.AccountType == "owner" ? UserRole.Owner : UserRole.User,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                string roleName = model.AccountType == "owner" ? AppConstants.Roles.Owner : AppConstants.Roles.User;
                await _userManager.AddToRoleAsync(user, roleName);

                if (model.AccountType == "owner")
                {
                    var company = new Company
                    {
                        Name = model.CompanyName ?? "",
                        OwnerId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _companyService.CreateCompanyAsync(company);
                }

                await _signInManager.SignInAsync(user, false);

                if (user.Role == UserRole.Owner)
                    return RedirectToAction("Dashboard", "Company");

                return RedirectToAction("Dashboard", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
