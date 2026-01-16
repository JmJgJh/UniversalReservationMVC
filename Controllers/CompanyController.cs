using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Common;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;

namespace UniversalReservationMVC.Controllers
{
    [Authorize(Roles = "Owner")]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ISeatMapService _seatMapService;
        private readonly ICompanyMemberService _companyMemberService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompanyController> _logger;
        private readonly IEmailService _emailService;
        private readonly IReportService _reportService;
        private readonly IAnalyticsService _analyticsService;

        public CompanyController(
            ICompanyService companyService,
            ISeatMapService seatMapService,
            ICompanyMemberService companyMemberService,
            IUnitOfWork unitOfWork,
            ILogger<CompanyController> logger,
            IEmailService emailService,
            IReportService reportService,
            IAnalyticsService analyticsService)
        {
            _companyService = companyService;
            _seatMapService = seatMapService;
            _companyMemberService = companyMemberService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _reportService = reportService;
            _analyticsService = analyticsService;
        }

        // Dashboard - main company panel
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Dashboard access denied: userId is null");
                return Forbid();
            }

            var isOwner = User.IsInRole("Owner");
            _logger.LogInformation("User {UserId} accessing Dashboard. IsInRole(Owner): {IsOwner}", userId, isOwner);

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                _logger.LogWarning("Company not found for owner {UserId}", userId);
                return RedirectToAction("Create");
            }

            return View(company);
        }

        // Create company (if not exists)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }
            var existingCompany = await _companyService.GetCompanyByOwnerAsync(userId);

            if (existingCompany != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }
            var existingCompany = await _companyService.GetCompanyByOwnerAsync(userId);

            if (existingCompany != null)
            {
                ModelState.AddModelError("", "Już masz założoną firmę.");
                return View(company);
            }

            if (ModelState.IsValid)
            {
                company.OwnerId = userId;
                company.CreatedAt = DateTime.UtcNow;
                company.IsActive = true;

                try
                {
                    await _companyService.CreateCompanyAsync(company);
                    _logger.LogInformation("Company created: {CompanyId}", company.Id);
                    return RedirectToAction("Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating company");
                    ModelState.AddModelError("", "Błąd przy tworzeniu firmy.");
                }
            }

            return View(company);
        }

        // Edit company info
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Edit GET: Returning company {CompanyId} with name '{Name}'", 
                company.Id, company.Name);

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string name, string? description, string? address, 
            string? phoneNumber, string? email, string? website, string? logoUrl)
        {
            var userId = User.GetCurrentUserId();
            
            if (userId == null)
            {
                _logger.LogWarning("Edit POST denied: userId is null");
                return Forbid();
            }

            // Pobierz firmę użytkownika z bazy
            var existingCompany = await _companyService.GetCompanyByOwnerAsync(userId);
            if (existingCompany == null)
            {
                _logger.LogWarning("Edit POST: Company not found for owner {UserId}", userId);
                return NotFound("Nie znaleziono firmy dla tego użytkownika.");
            }

            _logger.LogInformation("Edit POST: UserId={UserId}, CompanyId={CompanyId}, InputName='{InputName}'", 
                userId, existingCompany.Id, name ?? "NULL");

            // Waliduj tylko pola które chcemy zaktualizować
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 200)
            {
                _logger.LogWarning("Company edit failed: Invalid Name");
                ModelState.AddModelError("Name", "Nazwa firmy musi zawierać od 2 do 200 znaków");
                return View(existingCompany);
            }

            try
            {
                // Aktualizuj tylko edytowalne pola
                existingCompany.Name = name;
                existingCompany.Description = description;
                existingCompany.Address = address;
                existingCompany.PhoneNumber = phoneNumber;
                existingCompany.Email = email;
                existingCompany.Website = website;
                existingCompany.LogoUrl = logoUrl;
                existingCompany.UpdatedAt = DateTime.UtcNow;

                await _companyService.UpdateCompanyAsync(existingCompany);
                _logger.LogInformation("Company {CompanyId} updated by user {UserId}. Redirecting to Dashboard.", 
                    existingCompany.Id, userId);
                TempData["Success"] = "Dane firmy zostały zaktualizowane.";
                
                // Sprawdź czy użytkownik nadal ma rolę Owner przed przekierowaniem
                if (!User.IsInRole("Owner"))
                {
                    _logger.LogError("User {UserId} lost Owner role after update!", userId);
                    return RedirectToAction("Index", "Home");
                }
                
                return RedirectToAction("Dashboard", "Company");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company");
                ModelState.AddModelError("", "Błąd przy aktualizacji firmy.");
                return View(existingCompany);
            }
        }

        // Manage resources (rooms/workspaces)
        public async Task<IActionResult> Resources()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            ViewBag.Company = company;
            return View(company.Resources ?? new List<Resource>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResource(Resource resource)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }
            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Resource creation failed. Validation errors: {Errors}", errors);
                TempData["Error"] = $"Niepoprawne dane zasobu: {errors}";
                return BadRequest(new { message = "Niepoprawne dane zasobu", errors = errors });
            }

            resource.CompanyId = company.Id;
            resource.CreatedAt = DateTime.UtcNow;
            resource.UpdatedAt = null;

            await _unitOfWork.Resources.AddAsync(resource);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Zasób został utworzony.";
            return RedirectToAction("Resources");
        }

        // Seat Map Builder - interactive grid for resource
        [HttpGet]
        public async Task<IActionResult> SeatMapBuilder(int resourceId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }
            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return Forbid();
            }

            // Check if resource belongs to user's company
            var resource = company.Resources?.FirstOrDefault(r => r.Id == resourceId);
            if (resource == null)
            {
                return NotFound();
            }

            // Create ViewModel without circular references
            var viewModel = new SeatMapBuilderViewModel
            {
                Id = resource.Id,
                Name = resource.Name,
                Location = resource.Location,
                SeatMapWidth = resource.SeatMapWidth ?? 10,
                SeatMapHeight = resource.SeatMapHeight ?? 10,
                Seats = resource.Seats?.Select(s => new SeatDto
                {
                    Id = s.Id,
                    X = s.X,
                    Y = s.Y,
                    Label = s.Label ?? "",
                    Status = s.IsAvailable ? "available" : "unavailable"
                }).ToList() ?? new List<SeatDto>()
            };

            return View(viewModel);
        }

        // Manage company members
        [HttpGet]
        public async Task<IActionResult> Members()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            return View(company);
        }

        // Company settings (branding, colors, logo)
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            var model = new CompanySettingsViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Address = company.Address,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                Website = company.Website,
                LogoUrl = company.LogoUrl,
                PrimaryColor = company.PrimaryColor,
                SecondaryColor = company.SecondaryColor
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(CompanySettingsViewModel model)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            company.Name = model.Name;
            company.Description = model.Description;
            company.Address = model.Address;
            company.PhoneNumber = model.PhoneNumber;
            company.Email = model.Email;
            company.Website = model.Website;
            company.PrimaryColor = model.PrimaryColor;
            company.SecondaryColor = model.SecondaryColor;
            company.UpdatedAt = DateTime.UtcNow;

            // Handle logo upload
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
                var extension = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("LogoFile", "Dozwolone formaty: JPG, PNG, GIF, SVG");
                    return View(model);
                }

                if (model.LogoFile.Length > 2 * 1024 * 1024) // 2MB max
                {
                    ModelState.AddModelError("LogoFile", "Plik jest za duży (maksymalnie 2 MB)");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{company.Id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(fileStream);
                }

                // Delete old logo if exists
                if (!string.IsNullOrEmpty(company.LogoUrl))
                {
                    var oldLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", company.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldLogoPath))
                    {
                        System.IO.File.Delete(oldLogoPath);
                    }
                }

                company.LogoUrl = $"/uploads/logos/{uniqueFileName}";
            }

            await _companyService.UpdateCompanyAsync(company);

            TempData["Success"] = "Ustawienia firmy zostały zaktualizowane.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string email, string role, bool canManageResources, bool canManageReservations)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            var result = await _companyMemberService.AddMemberByEmailAsync(company.Id, email, role, canManageResources, canManageReservations);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction("Members");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int companyId, string userId)
        {
            var ownerId = User.GetCurrentUserId();
            if (ownerId == null)
            {
                return Forbid();
            }
            var company = await _companyService.GetCompanyByOwnerAsync(ownerId);

            if (company == null || company.Id != companyId)
            {
                return Forbid();
            }

            await _companyMemberService.RemoveMemberAsync(companyId, userId);
            TempData["Success"] = "Użytkownik został usunięty z firmy";
            return RedirectToAction("Members");
        }

        // Edit member permissions
        [HttpGet]
        public async Task<IActionResult> EditMemberPermissions(int id)
        {
            var ownerId = User.GetCurrentUserId();
            if (ownerId == null)
            {
                return Forbid();
            }

            var member = await _unitOfWork.CompanyMembers
                .GetByIdWithIncludesAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(ownerId);
            if (company == null || company.Id != member.CompanyId)
            {
                return Forbid();
            }

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMemberPermissions(
            int id,
            string role,
            bool canManageResources,
            bool canViewReservations,
            bool canManageReservations,
            bool canManageEvents,
            bool canViewAnalytics,
            bool canExportReports,
            bool canManageMembers)
        {
            var ownerId = User.GetCurrentUserId();
            if (ownerId == null)
            {
                return Forbid();
            }

            var member = await _unitOfWork.CompanyMembers.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(ownerId);
            if (company == null || company.Id != member.CompanyId)
            {
                return Forbid();
            }

            member.Role = role;
            member.CanManageResources = canManageResources;
            member.CanViewReservations = canViewReservations;
            member.CanManageReservations = canManageReservations;
            member.CanManageEvents = canManageEvents;
            member.CanViewAnalytics = canViewAnalytics;
            member.CanExportReports = canExportReports;
            member.CanManageMembers = canManageMembers;

            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Uprawnienia zostały zaktualizowane";
            return RedirectToAction("Members");
        }

        // API endpoint to save/update seat map
        [HttpPost]
        public async Task<IActionResult> SaveSeatMap([FromBody] SaveSeatMapRequest request)
        {
            _logger.LogInformation("SaveSeatMap called with resourceId={ResourceId}, seats count={Count}", 
                request?.ResourceId ?? 0, request?.Seats?.Count ?? 0);

            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Użytkownik niezalogowany." });
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null)
            {
                return Json(new { success = false, message = "Firma nie znaleziona." });
            }

            if (request == null || request.ResourceId == 0)
            {
                return Json(new { success = false, message = "Brak ID zasobu." });
            }

            var resource = company.Resources?.FirstOrDefault(r => r.Id == request.ResourceId);
            if (resource == null)
            {
                return Json(new { success = false, message = $"Zasób {request.ResourceId} nie znaleziony." });
            }

            if (request.Seats == null || !request.Seats.Any())
            {
                return Json(new { success = false, message = "Brak danych miejsc." });
            }

            try
            {
                var seatModels = request.Seats.Select(s => new Seat
                {
                    X = s.X,
                    Y = s.Y,
                    Label = s.Label ?? $"{s.X},{s.Y}",
                    ResourceId = request.ResourceId,
                    IsAvailable = !string.Equals(s.Status, "unavailable", StringComparison.OrdinalIgnoreCase)
                }).ToList();

                await _seatMapService.SaveSeatMapAsync(request.ResourceId, seatModels);
                
                _logger.LogInformation("Seat map saved for resource {ResourceId}, {SeatCount} seats", 
                    request.ResourceId, seatModels.Count);

                return Json(new { success = true, message = "Mapa pomieszczeń została zapisana!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving seat map for resource {ResourceId}", request.ResourceId);
                return Json(new { success = false, message = $"Błąd: {ex.Message}" });
            }
        }

        public class SaveSeatMapRequest
        {
            public int ResourceId { get; set; }
            public List<SeatData> Seats { get; set; } = new();
        }

        // Edit resource
        [HttpGet]
        public async Task<IActionResult> EditResource(int resourceId)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return Forbid();
            }

            var resource = company.Resources?.FirstOrDefault(r => r.Id == resourceId);
            if (resource == null)
            {
                return NotFound();
            }

            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResource(int resourceId, string name, int resourceType, 
            string? location, string? description, int? capacity, int? seatMapWidth, int? seatMapHeight, decimal price)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null)
            {
                return Forbid();
            }

            var existing = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (existing == null || existing.CompanyId != company.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Nazwa zasobu jest wymagana");
                return View(existing);
            }

            existing.Name = name;
            existing.Description = description;
            existing.Location = location;
            existing.ResourceType = (ResourceType)resourceType;
            existing.Capacity = capacity;
            existing.SeatMapHeight = seatMapHeight;
            existing.SeatMapWidth = seatMapWidth;
            existing.Price = price;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Resources.Update(existing);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Resource {ResourceId} updated by company {CompanyId}", resourceId, company.Id);
            TempData["Success"] = "Zasób został zaktualizowany.";
            return RedirectToAction("Resources");
        }

        // Delete resource
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResource(int resourceId)
        {
            _logger.LogInformation("DeleteResource called with resourceId={ResourceId}", resourceId);
            
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("DeleteResource: userId is null");
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return Forbid();
            }

            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (resource == null)
            {
                return NotFound();
            }

            if (resource.CompanyId != company.Id)
            {
                return Forbid();
            }

            var existingReservations = await _unitOfWork.Reservations.GetByResourceIdAsync(resourceId);
            if (existingReservations.Any(r => r.Status != ReservationStatus.Cancelled))
            {
                TempData["Error"] = "Nie można usunąć zasobu z aktywnymi rezerwacjami.";
                return RedirectToAction("Resources");
            }

            try
            {
                _unitOfWork.Resources.Remove(resource);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Resource {ResourceId} deleted by company {CompanyId}", resourceId, company.Id);
                TempData["Success"] = "Zasób został usunięty.";
                return RedirectToAction("Resources");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resource {ResourceId}", resourceId);
                TempData["Error"] = "Wystąpił błąd podczas usuwania zasobu.";
                return RedirectToAction("Resources");
            }
        }

        // View all reservations for company's resources
        public async Task<IActionResult> Reservations(DateTime? from, DateTime? to, string? status, int page = 1, int pageSize = 15)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            var reservations = new List<Reservation>();
            var resourceIds = company.Resources?.Select(r => r.Id).ToList() ?? new List<int>();

            foreach (var id in resourceIds)
            {
                var res = await _unitOfWork.Reservations.GetByResourceIdAsync(id, from, to);
                reservations.AddRange(res);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var parsedStatus))
            {
                reservations = reservations.Where(r => r.Status == parsedStatus).ToList();
            }

            reservations = reservations
                .OrderByDescending(r => r.StartTime)
                .ToList();

            var totalCount = reservations.Count;
            var pendingCount = reservations.Count(r => r.Status == ReservationStatus.Pending);
            var confirmedCount = reservations.Count(r => r.Status == ReservationStatus.Confirmed);
            var cancelledCount = reservations.Count(r => r.Status == ReservationStatus.Cancelled);
            var safePage = page < 1 ? 1 : page;
            var paged = reservations
                .Skip((safePage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Company = company;

            var vm = new CompanyReservationsViewModel
            {
                Reservations = paged,
                Page = safePage,
                PageSize = pageSize,
                TotalCount = totalCount,
                PendingCount = pendingCount,
                ConfirmedCount = confirmedCount,
                CancelledCount = cancelledCount,
                From = from,
                To = to,
                Status = status
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return Forbid();
            }

            if (company.Resources == null)
            {
                return NotFound();
            }

            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
            if (resource == null || resource.CompanyId != company.Id)
            {
                return Forbid();
            }

            if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
            {
                TempData["Error"] = "Nie można potwierdzić tej rezerwacji.";
                return RedirectToAction(nameof(Reservations));
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.SaveAsync();

            // Send confirmation email
            var email = reservation.User?.Email ?? reservation.GuestEmail;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var userName = reservation.User?.FirstName ?? reservation.User?.UserName ?? "Gość";
                var seat = reservation.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(reservation.SeatId.Value) : null;
                var seatInfo = seat != null ? $"Rząd {seat.X}, Miejsce {seat.Y}" : null;

                try
                {
                    await _emailService.SendReservationConfirmationAsync(
                        email,
                        userName,
                        resource.Name,
                        reservation.StartTime,
                        seatInfo,
                        company.Name
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email for reservation {ReservationId}", id);
                }
            }

            TempData["Success"] = "Rezerwacja została potwierdzona.";
            return RedirectToAction(nameof(Reservations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return Forbid();
            }

            if (company.Resources == null)
            {
                return NotFound();
            }

            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
            if (resource == null || resource.CompanyId != company.Id)
            {
                return Forbid();
            }

            if (reservation.Status == ReservationStatus.Completed || reservation.Status == ReservationStatus.Cancelled)
            {
                TempData["Error"] = "Rezerwacja jest już zakończona lub anulowana.";
                return RedirectToAction(nameof(Reservations));
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.SaveAsync();

            // Send cancellation email
            var email = reservation.User?.Email ?? reservation.GuestEmail;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var userName = reservation.User?.FirstName ?? reservation.User?.UserName ?? "Gość";

                try
                {
                    await _emailService.SendReservationCancellationAsync(
                        email,
                        userName,
                        resource.Name,
                        reservation.StartTime,
                        company.Name
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email for reservation {ReservationId}", id);
                }
            }

            TempData["Success"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(Reservations));
        }

        // Reports and analytics
        public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyByOwnerAsync(userId);

            if (company == null)
            {
                return RedirectToAction("Create");
            }

            var report = new CompanyReportViewModel
            {
                TotalResources = company.Resources?.Count ?? 0,
                TotalSeats = company.Resources?.Sum(r => r.Seats?.Count ?? 0) ?? 0,
                From = from,
                To = to
            };

            int confirmedAcrossAll = 0;

            foreach (var resource in company.Resources ?? new List<Resource>())
            {
                var resReservations = (await _unitOfWork.Reservations.GetByResourceIdAsync(resource.Id, from, to)).ToList();
                var seatCount = resource.Seats?.Count ?? 0;
                var confirmed = resReservations.Count(r => r.Status == ReservationStatus.Confirmed);
                confirmedAcrossAll += confirmed;

                var row = new ResourceReportRow
                {
                    ResourceId = resource.Id,
                    Name = resource.Name,
                    ResourceType = resource.ResourceType.ToString(),
                    SeatCount = seatCount,
                    ReservationCount = resReservations.Count,
                    Occupancy = seatCount > 0 ? Math.Round(confirmed / (double)seatCount, 2) : 0,
                    Revenue = 0 // Brak danych płatności w tym etapie
                };

                report.Resources.Add(row);
                report.TotalReservations += resReservations.Count;
                report.UpcomingReservations += resReservations.Count(r => r.StartTime >= DateTime.UtcNow);
            }

            report.OccupancyRate = report.TotalSeats > 0
                ? Math.Round(confirmedAcrossAll / (double)report.TotalSeats, 2)
                : 0;

            ViewBag.Company = company;
            return View(report);
        }

        // Export reservations to PDF
        [HttpGet]
        public async Task<IActionResult> ExportReservationsPdf(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Forbid();

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null) return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var reservations = await _unitOfWork.Reservations
                .FindAsync(r => r.Resource != null && r.Resource.CompanyId == company.Id
                    && r.StartTime >= start && r.StartTime <= end);

            var pdf = await _reportService.GenerateReservationsPdfAsync(
                reservations,
                $"Rezerwacje: {company.Name}");

            return File(pdf, "application/pdf", $"Rezerwacje_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // Export reservations to Excel
        [HttpGet]
        public async Task<IActionResult> ExportReservationsExcel(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Forbid();

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null) return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var reservations = await _unitOfWork.Reservations
                .FindAsync(r => r.Resource != null && r.Resource.CompanyId == company.Id
                    && r.StartTime >= start && r.StartTime <= end);

            var excel = await _reportService.GenerateReservationsExcelAsync(
                reservations,
                "Rezerwacje");

            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Rezerwacje_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // Export financial summary to PDF
        [HttpGet]
        public async Task<IActionResult> ExportFinancialSummaryPdf(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Forbid();

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null) return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var pdf = await _reportService.GenerateCompanySummaryPdfAsync(company, start, end);

            return File(pdf, "application/pdf", $"Raport_Finansowy_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // Export revenue report to Excel
        [HttpGet]
        public async Task<IActionResult> ExportRevenueExcel(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Forbid();

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null) return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var excel = await _reportService.GenerateRevenueReportExcelAsync(company, start, end);

            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Raport_Przychodow_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // Analytics Dashboard
        [HttpGet]
        public async Task<IActionResult> Analytics(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Forbid();

            var company = await _companyService.GetCompanyByOwnerAsync(userId);
            if (company == null) return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var analytics = await _analyticsService.GetCompanyAnalyticsAsync(company.Id, start, end);
            ViewBag.CompanyName = company.Name;

            return View(analytics);
        }
    }

    // Helper DTO for seat data
    public class SeatData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Status { get; set; } = "available";
    }
}
