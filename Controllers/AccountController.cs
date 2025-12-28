using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Common;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Błąd logowania.");
            }
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
            if (ModelState.IsValid)
            {
                // Validate account type selection
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

                    // If owner, create company
                    if (model.AccountType == "owner")
                    {
                        try
                        {
                            var company = new Company
                            {
                                Name = model.CompanyName ?? string.Empty,
                                Address = model.CompanyAddress,
                                PhoneNumber = model.CompanyPhone,
                                Email = model.CompanyEmail,
                                Description = model.CompanyDescription,
                                OwnerId = user.Id,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _companyService.CreateCompanyAsync(company);
                            _logger.LogInformation("Company created for user {UserId}", user.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating company for user {UserId}", user.Id);
                            ModelState.AddModelError(string.Empty, "Błąd przy tworzeniu firmy. Spróbuj ponownie.");
                            await _userManager.DeleteAsync(user);
                            return View(model);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // Redirect to appropriate dashboard
                    if (model.AccountType == "owner")
                    {
                        return RedirectToAction("Dashboard", "Company");
                    }
                    
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
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
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
