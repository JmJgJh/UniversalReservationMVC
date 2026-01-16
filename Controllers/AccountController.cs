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
        private readonly IReservationService _reservationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ICompanyService companyService,
            IReservationService reservationService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _companyService = companyService;
            _reservationService = reservationService;
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
                // Znajdź użytkownika po emailu, aby uzyskać UserName
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User with email {Email} not found", model.Email);
                    ViewData["LoginError"] = "Błędne dane logowania";
                    return View(model);
                }

                // Użyj UserName zamiast Email do logowania
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName ?? model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);

                    // Zwykły użytkownik -> dashboard
                    if (user.Role == UserRole.User)
                        return RedirectToAction("Dashboard", "Account");

                    // Właściciel -> panel firmy
                    if (user.Role == UserRole.Owner)
                        return RedirectToAction("Dashboard", "Company");

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out", model.Email);
                    ViewData["LoginError"] = "Konto zostało zablokowane";
                }
                else if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User {Email} is not allowed to sign in", model.Email);
                    ViewData["LoginError"] = "Logowanie nie jest możliwe";
                }
                else
                {
                    _logger.LogWarning("Login failed for user {Email}: Invalid password", model.Email);
                    ViewData["LoginError"] = "Błędne dane logowania";
                }
            }
            else
            {
                _logger.LogWarning("Login form validation failed");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            // Pobierz prawdziwe dane z bazy
            var allReservations = await _reservationService.GetReservationsForUserAsync(user.Id);
            var now = DateTime.UtcNow;
            var upcomingReservations = allReservations
                .Where(r => r.StartTime > now)
                .OrderBy(r => r.StartTime)
                .Take(5)
                .ToList();

            var model = new UserDashboardViewModel
            {
                TotalReservationsCount = allReservations.Count(),
                ActiveReservationsCount = allReservations.Count(r => r.Status == ReservationStatus.Confirmed && r.StartTime > now),
                CompletedReservationsCount = allReservations.Count(r => r.EndTime < now),
                UpcomingReservations = upcomingReservations
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
            {
                _logger.LogWarning("Registration validation failed. Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("  - {ErrorMessage}", error.ErrorMessage);
                }
                return View(model);
            }

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
                        Description = model.CompanyDescription,
                        Address = model.CompanyAddress,
                        PhoneNumber = model.CompanyPhone,
                        Email = model.CompanyEmail,
                        OwnerId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _companyService.CreateCompanyAsync(company);
                    _logger.LogInformation("Company '{CompanyName}' created for user {UserId}", company.Name, user.Id);
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil został zaktualizowany pomyślnie.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
