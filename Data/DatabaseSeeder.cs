using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Check if database already has data
        if (await _context.Companies.AnyAsync())
        {
            _logger.LogInformation("Database already seeded. Skipping seed.");
            return;
        }

        _logger.LogInformation("Starting database seeding...");

        // Create demo owners
        var owner1 = await CreateUserAsync("owner1@example.com", "Owner123!", UserRole.Owner);
        var owner2 = await CreateUserAsync("owner2@example.com", "Owner123!", UserRole.Owner);
        var owner3 = await CreateUserAsync("owner3@example.com", "Owner123!", UserRole.Owner);
        
        // Create demo regular users
        var user1 = await CreateUserAsync("user1@example.com", "User123!", UserRole.User);
        var user2 = await CreateUserAsync("user2@example.com", "User123!", UserRole.User);

        // Create companies
        var cinema = new Company
        {
            Name = "Cinema Centrum",
            Description = "Nowoczesne kino w centrum miasta z 5 salami projekcyjnymi",
            OwnerId = owner1.Id,
            IsActive = true,
            PrimaryColor = "#dc3545",
            SecondaryColor = "#6c757d"
        };
        _context.Companies.Add(cinema);

        var office = new Company
        {
            Name = "CoWork Space",
            Description = "Przestrzeń coworkingowa z salami konferencyjnymi",
            OwnerId = owner2.Id,
            IsActive = true,
            PrimaryColor = "#0d6efd",
            SecondaryColor = "#6c757d"
        };
        _context.Companies.Add(office);

        var restaurant = new Company
        {
            Name = "Restauracja Pod Złotym Lwem",
            Description = "Elegancka restauracja z możliwością rezerwacji stolików i sal bankietowych",
            OwnerId = owner3.Id,
            IsActive = true,
            PrimaryColor = "#ffc107",
            SecondaryColor = "#6c757d"
        };
        _context.Companies.Add(restaurant);

        await _context.SaveChangesAsync();

        // Create resources for cinema
        var sala1 = CreateResource("Sala 1 - IMAX", "Największa sala z ekranem IMAX", ResourceType.Cinema, cinema.Id);
        var sala2 = CreateResource("Sala 2 - VIP", "Ekskluzywna sala z fotelami premium", ResourceType.Cinema, cinema.Id);
        var sala3 = CreateResource("Sala 3 - Standard", "Standardowa sala kinowa", ResourceType.Cinema, cinema.Id);

        // Create resources for coworking
        var meeting1 = CreateResource("Sala konferencyjna A", "Sala dla 20 osób z projektorem", ResourceType.ConferenceRoom, office.Id);
        var meeting2 = CreateResource("Sala konferencyjna B", "Sala dla 10 osób", ResourceType.ConferenceRoom, office.Id);
        var desk1 = CreateResource("Biurko Hot Desk 1", "Elastyczne miejsce pracy", ResourceType.Office, office.Id);

        // Create resources for restaurant
        var table1 = CreateResource("Sala główna", "Główna sala restauracyjna", ResourceType.Restaurant, restaurant.Id);
        var table2 = CreateResource("Sala bankietowa", "Sala na wydarzenia prywatne", ResourceType.Restaurant, restaurant.Id);

        _context.Resources.AddRange(sala1, sala2, sala3, meeting1, meeting2, desk1, table1, table2);
        await _context.SaveChangesAsync();

        // Create seats for cinema rooms (10 rows x 15 seats)
        CreateSeats(sala1.Id, 10, 15);
        CreateSeats(sala2.Id, 8, 12);
        CreateSeats(sala3.Id, 12, 18);

        // Create seats for meeting rooms
        CreateSeats(meeting1.Id, 4, 5); // 20 seats
        CreateSeats(meeting2.Id, 2, 5); // 10 seats

        // Create table seats
        CreateSeats(table1.Id, 6, 8); // 48 tables
        CreateSeats(table2.Id, 5, 6); // 30 tables

        await _context.SaveChangesAsync();

        // Create events for cinema
        var today = DateTime.Today;
        
        var event1 = new Event
        {
            Title = "Premiera: Nowy Początek",
            Description = "Ekscytująca premiera najnowszego filmu sci-fi",
            ResourceId = sala1.Id,
            StartTime = today.AddDays(3).AddHours(18),
            EndTime = today.AddDays(3).AddHours(20).AddMinutes(30)
        };

        var event2 = new Event
        {
            Title = "Klasyka Kina: Ojciec Chrzestny",
            Description = "Specjalny pokaz kultowego filmu",
            ResourceId = sala2.Id,
            StartTime = today.AddDays(5).AddHours(19),
            EndTime = today.AddDays(5).AddHours(22)
        };

        var event3 = new Event
        {
            Title = "Kino dla dzieci: Bajka animowana",
            Description = "Rodzinny seans animacji",
            ResourceId = sala3.Id,
            StartTime = today.AddDays(2).AddHours(15),
            EndTime = today.AddDays(2).AddHours(17)
        };

        // Create events for coworking
        var event4 = new Event
        {
            Title = "Warsztat: Produktywność w pracy",
            Description = "Warsztaty z zakresu zarządzania czasem",
            ResourceId = meeting1.Id,
            StartTime = today.AddDays(7).AddHours(10),
            EndTime = today.AddDays(7).AddHours(14)
        };

        var event5 = new Event
        {
            Title = "Meetup technologiczny",
            Description = "Spotkanie programistów i entuzjastów technologii",
            ResourceId = meeting2.Id,
            StartTime = today.AddDays(4).AddHours(18),
            EndTime = today.AddDays(4).AddHours(21)
        };

        _context.Events.AddRange(event1, event2, event3, event4, event5);
        await _context.SaveChangesAsync();

        // Create some reservations
        var reservation1 = new Reservation
        {
            ResourceId = sala1.Id,
            UserId = user1.Id,
            StartTime = today.AddDays(3).AddHours(18),
            EndTime = today.AddDays(3).AddHours(20).AddMinutes(30),
            Status = ReservationStatus.Confirmed
        };

        var reservation2 = new Reservation
        {
            ResourceId = meeting1.Id,
            UserId = user2.Id,
            StartTime = today.AddDays(7).AddHours(10),
            EndTime = today.AddDays(7).AddHours(14),
            Status = ReservationStatus.Pending
        };

        _context.Reservations.AddRange(reservation1, reservation2);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Database seeding completed successfully!");
        _logger.LogInformation("Demo users created:");
        _logger.LogInformation("  Owner: owner1@example.com / Owner123!");
        _logger.LogInformation("  Owner: owner2@example.com / Owner123!");
        _logger.LogInformation("  Owner: owner3@example.com / Owner123!");
        _logger.LogInformation("  User: user1@example.com / User123!");
        _logger.LogInformation("  User: user2@example.com / User123!");
    }

    private async Task<ApplicationUser> CreateUserAsync(string email, string password, UserRole role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Role = role,
            FirstName = email.Split('@')[0]
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            var roleName = role.ToString();
            await _userManager.AddToRoleAsync(user, roleName);
            _logger.LogInformation("Created user: {Email} with role {Role}", email, roleName);
        }
        else
        {
            _logger.LogError("Failed to create user {Email}: {Errors}", 
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    private Resource CreateResource(string name, string description, ResourceType type, int companyId)
    {
        return new Resource
        {
            Name = name,
            Description = description,
            ResourceType = type,
            CompanyId = companyId,
            Price = 0m
        };
    }

    private void CreateSeats(int resourceId, int rows, int columns)
    {
        for (int row = 1; row <= rows; row++)
        {
            for (int col = 1; col <= columns; col++)
            {
                var seat = new Seat
                {
                    ResourceId = resourceId,
                    Label = $"{(char)('A' + row - 1)}{col}",
                    X = col,
                    Y = row,
                    IsAvailable = true
                };
                _context.Seats.Add(seat);
            }
        }
    }
}
