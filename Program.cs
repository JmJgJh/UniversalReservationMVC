using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Database: default to SQLite. Change to MSSQL in appsettings if desired.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=reservations.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Application services
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ISeatMapService, SeatMapService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

// Seed roles and a default admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    string[] roles = new[] { "Admin", "Owner", "User", "Guest" };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }
    // Create a default admin if not exists
    var adminEmail = builder.Configuration["DefaultAdmin:Email"] ?? "admin@example.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, Role = UserRole.Admin };
        await userManager.CreateAsync(admin, builder.Configuration["DefaultAdmin:Password"] ?? "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
