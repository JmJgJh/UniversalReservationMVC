using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Database: default to SQLite. Change to MSSQL in appsettings if desired.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=reservations.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Payment service
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Report service
builder.Services.AddScoped<IReportService, ReportService>();

// Analytics service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Application services
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ISeatMapService, SeatMapService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICompanyMemberService, CompanyMemberService>();
builder.Services.AddSingleton<ISeatHoldService, SeatHoldService>();

// Repositories
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyMemberRepository, CompanyMemberRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Seed roles and a default admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        string[] roles = new[] { "Admin", "Owner", "User", "Guest" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
                logger.LogInformation("Created role: {Role}", r);
            }
        }
        
        // Create a default admin if not exists
        var adminEmail = builder.Configuration["DefaultAdmin:Email"] ?? "admin@example.com";
        var adminPassword = builder.Configuration["DefaultAdmin:Password"];
        
        if (string.IsNullOrEmpty(adminPassword))
        {
            logger.LogWarning("DefaultAdmin:Password not configured in User Secrets. Default admin will not be created.");
            logger.LogWarning("Run: dotnet user-secrets set \"DefaultAdmin:Password\" \"YourSecurePassword123!\"");
        }
        else
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser 
                { 
                    UserName = adminEmail, 
                    Email = adminEmail, 
                    EmailConfirmed = true, 
                    Role = UserRole.Admin 
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation("Default admin user created: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create default admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR hubs
app.MapHub<UniversalReservationMVC.Hubs.SeatHub>("/hubs/seat");

app.Run();
