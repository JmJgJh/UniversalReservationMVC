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

## ✅ Rzeczy zrobione (Done)

- [x] Inicjalizacja projektu ASP.NET Core 8 MVC
- [x] Konfiguracja ASP.NET Core Identity z rolami (Admin, Owner, User, Guest)
- [x] EF Core + SQLite (domyślna baza `reservations.db`)
- [x] Model `Resource` - generyczne zasoby rezerwowalne
- [x] Model `Seat` - mapa miejsc (grid X,Y)
- [x] Model `Reservation` - rezerwacje z obsługą gościa (bez konta)
- [x] Model `Event` - zdarzenia powiązane z zasobami
- [x] Model `Ticket` - bilety/przepustki
- [x] `ReservationService` - logika rezerwacji i sprawdzanie konfliktów
- [x] `SeatMapService` - zarządzanie dostępnością miejsc
- [x] `EventService` - obsługa zdarzeń
- [x] `TicketService` - obsługa biletów
- [x] Kontrolery: ReservationController, ResourceController, EventController, TicketController, AdminController
- [x] Widoki do rezerwacji, zasobów, zdarzeń
- [x] Walidacja formularzy i obsługa błędów
- [x] Migracje EF Core w `/Migrations`
- [x] Seeding ról domyślnych w `Program.cs`

## 📋 Do zrobienia (Todo)

- [ ] Wdrożenie płatności dla Ticket Purchase Flow (integracja z PayPal/Stripe)
- [ ] Rozszerzone widoki zarządzania zasobami dla Admin/Owner (edycja, usuwanie, statystyki)
- [ ] Graficzna reprezentacja mapy miejsc (SVG/Canvas zamiast tabeli)
- [ ] System powiadomień e-mail dla rezerwacji i zmian
- [ ] Export rezerwacji do PDF
- [ ] API REST endpoints dla mobilnych/zewnętrznych klientów
- [ ] Unit testy (xUnit framework)
- [ ] Integracja testów e2e (Selenium/Playwright)
- [ ] Dashboard analytics dla Admin (statystyki rezerwacji, przychody)
- [ ] Obsługa anulowania rezerwacji z refundacją
- [ ] System rabatów/kodów promocyjnych
- [ ] Wielojęzyczność (wsparcie i18n)
- [ ] Automatyczne archiwizowanie starych rezerwacji
- [ ] Migracja na MS SQL Server (jeśli wymagane)
- [ ] Containeryzacja (Docker)
- [ ] CI/CD pipeline (GitHub Actions)

