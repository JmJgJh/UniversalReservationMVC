# UniversalReservationMVC

Prosty szkielet aplikacji ASP.NET Core 8 MVC dla rezerwacji zasobów (restauracje, kina, teatry, biura, sale konferencyjne).

Cechy i decyzje projektowe:
- ASP.NET Core 8 MVC
- Identity z rolami: Admin, Owner, User, Guest
- Entity Framework Core + SQLite (domyślnie `reservations.db`)
- Mapy miejsc (siatka współrzędnych) reprezentowane przez `Seat` (X,Y)
- Rezerwacje możliwe również bez konta poprzez podanie e-mail/telefonu

Szybkie uruchomienie (Windows PowerShell):

```powershell
cd UniversalReservationMVC
dotnet restore
dotnet ef migrations add InitialCreate -c UniversalReservationMVC.Data.ApplicationDbContext
dotnet ef database update -c UniversalReservationMVC.Data.ApplicationDbContext
dotnet run
```

Domyślny administrator (jeśli nie istnieje) jest tworzony według ustawień w `appsettings.json` (można ustawić `DefaultAdmin:Email` i `DefaultAdmin:Password`).

Następne kroki:
- Dodać widoki zarządzania zasobami (Admin)
- Implementacja płatności (Ticket purchase flow)
- Rozszerzyć logikę siatki miejsc (grafika SVG/Canvas)

