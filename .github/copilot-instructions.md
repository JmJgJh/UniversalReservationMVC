# Copilot instructions for UniversalReservationMVC

Purpose: give AI coding agents immediate, actionable context to make safe, consistent changes to the project.

- Big picture:
  - This is an ASP.NET Core 8 MVC application (project file: `UniversalReservationMVC.csproj`).
  - Authentication: ASP.NET Core Identity integrated with EF Core (`Data/ApplicationDbContext.cs`) and role seeding is executed in `Program.cs`.
  - Persistence: EF Core with SQLite by default (connection string in `appsettings.json`). Change to MSSQL by updating `Program.cs`/`appsettings.json` and provider packages.
  - Main domains: `Resource` (generic reservable resource), `Seat` (grid coordinates), `Reservation`, `Event`, `Ticket` (see `Models/`).

- Where to look first (fast path):
  - App startup and DI: `Program.cs` — shows DB provider, Identity setup, role seeding, and service registrations (e.g. `IReservationService`).
  - DB model surface: `Data/ApplicationDbContext.cs` and `Models/*` — adding/removing entities should be reflected here and migrations created.
  - Reservation domain logic: `Services/ReservationService.cs` (conflict checks, seat availability).
  - Controllers to change UI/flows: `Controllers/ReservationController.cs`, `Controllers/HomeController.cs`, `Controllers/AdminController.cs`.
  - Views (Polish UI): `Views/` — UI strings are in Polish and must remain Polish unless instructed otherwise.

- Project-specific conventions and patterns
  - Polish UI, English code: keep UI text in Polish (views, labels, validation messages) and class/method names in English without diacritics.
  - Seat map model: seats represented as a grid with integer `X` and `Y` coordinates (see `Models/Seat.cs`). When adding seat-map logic, operate on (X,Y) grid coordinates and persist `Seat` rows.
  - Guest reservations: support reservations without an authenticated user by populating `GuestEmail` and/or `GuestPhone` on `Reservation` (see `Models/Reservation.cs`).
  - Role names: use exact strings `Admin`, `Owner`, `User`, `Guest` (seeded in `Program.cs`) — use these constants when checking or assigning roles.
  - Data migrations: use EF Core CLI (`dotnet ef`) against `UniversalReservationMVC.Data.ApplicationDbContext`.

- Common workflows (explicit commands)
  - Restore & run (PowerShell):

    ```powershell
    cd UniversalReservationMVC
    dotnet restore
    dotnet ef migrations add InitialCreate -c UniversalReservationMVC.Data.ApplicationDbContext
    dotnet ef database update -c UniversalReservationMVC.Data.ApplicationDbContext
    dotnet run
    ```

  - Switch to MSSQL: update `UniversalReservationMVC.csproj` to include `Microsoft.EntityFrameworkCore.SqlServer` and change `appsettings.json` connection string provider accordingly, then update `Program.cs` provider call.

- Making database/model changes
  - Update the model type in `Models/` and `Data/ApplicationDbContext.cs`.
  - Add EF Core migration (see commands above). Keep migrations in `/Migrations` (the folder exists in repo; generated files should be committed).
  - Do not modify the seeded role strings unless you also update role checks across controllers.

- Testing and runtime notes
  - No unit tests are included in this scaffold. When adding tests, prefer xUnit and a separate test project.
  - Identity default admin account is created at startup using `appsettings.json:DefaultAdmin`. If you need a different password flow, update `Program.cs` and secrets accordingly.

- UI & localization
  - The UI is intentionally Polish. When adding new views or validation messages, write Polish strings. Keep method and class names in English.

- Integration points and extension areas
  - Payments/ticket purchases: `Models/Ticket.cs` exists but payment provider integration is not implemented. Add a service under `Services/` (e.g. `IPaymentService`) and a controller action for checkout.
  - Seat-map frontend: current example is a simple table (`Views/Reservation/Index.cshtml`) — replace with SVG/Canvas for graphical seat maps but preserve (X,Y) coordinate mapping on the server.

- Safety & constraints for AI edits
  - Preserve role strings and Identity configuration unless the change explicitly targets authentication.
  - When altering DB schema, always add a migration and include generated migration files under `/Migrations`.
  - UI text is Polish — avoid translating UI strings to English unless requested.

If anything above is unclear or you want the agent to follow a stricter convention (naming, folder layout, or style rules), tell me which parts to enforce and I will update these instructions.
