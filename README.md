# UniversalReservationMVC

Aktualizacja v2.0 — Grudzień 2025

System został gruntownie zrefaktoryzowany i przygotowany do obrony pracy dyplomowej na poziomie BARDZO DOBRY (5.0). Szczegóły zmian znajdują się w IMPROVEMENTS.md.

Najważniejsze ulepszenia:
- Repository Pattern + Unit of Work (testowalność, separacja warstw)
- Globalny middleware obsługi błędów (ExceptionHandlingMiddleware)
- User Secrets (usunięto hasła z appsettings.json)
- 10 testów jednostkowych (xUnit + EF InMemory + Moq)
- Edycja rezerwacji z wykrywaniem konfliktów
- Indeksy i precyzyjna konfiguracja relacji w EF Core
- Konsekwentne logowanie (ILogger) i walidacja (IValidatableObject)

Szybki start (Windows PowerShell):

```powershell
cd UniversalReservationMVC
dotnet restore
dotnet user-secrets init
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
dotnet ef database update -c UniversalReservationMVC.Data.ApplicationDbContext
dotnet run
```

Szczegóły instalacji i konfiguracji: SETUP.md. Pełny raport zmian: IMPROVEMENTS.md.

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

- [ ] Integracja płatności (Stripe/PayPal)
- [ ] Rozszerzone widoki zarządzania zasobami dla Admin/Owner (edycja, usuwanie, statystyki)
- [ ] Graficzna reprezentacja mapy miejsc (SVG/Canvas)
- [ ] System powiadomień e-mail (SendGrid)
- [ ] Export rezerwacji do PDF
- [ ] Publiczne API REST dla aplikacji mobilnych
- [ ] Testy e2e (Playwright/Selenium)
- [ ] Dashboard i analityka dla Admin
- [ ] Anulowanie z refundacją
- [ ] Rabaty/kody promocyjne
- [ ] Wielojęzyczność (i18n)
- [ ] Archiwizacja starych rezerwacji
- [ ] Migracja na MS SQL Server (opcjonalnie)
- [ ] Docker (containeryzacja)
- [ ] CI/CD (GitHub Actions)

## 🧭 API mapy miejsc (JSON)

Te endpointy umożliwiają budowę graficznej, interaktywnej mapy miejsc (SVG/Canvas) bez konieczności generowania HTML po stronie serwera.

- **GET** `/Seat/MapJson?resourceId={id}`
	- **Opis:** Zwraca pełną siatkę miejsc dla zasobu.
	- **Parametry:** `resourceId` (int)
	- **Odpowiedź:** lista obiektów `{ id, resourceId, x, y, row, column, label }`

- **GET** `/Seat/Availability?resourceId={id}&start={iso}&end={iso}`
	- **Opis:** Zwraca listę zajętych miejsc w podanym oknie czasu.
	- **Parametry:**
		- `resourceId` (int)
		- `start` (DateTime ISO 8601, np. `2025-12-28T18:00:00Z`)
		- `end` (DateTime ISO 8601)
	- **Odpowiedź:** obiekt `{ resourceId, start, end, occupiedSeatIds: number[] }`

### Przykłady (PowerShell / curl)

```powershell
curl "https://localhost:5001/Seat/MapJson?resourceId=1"
curl "https://localhost:5001/Seat/Availability?resourceId=1&start=2025-12-28T18:00:00Z&end=2025-12-28T20:00:00Z"
```

### Integracja frontendu (skrót)

- Pobierz siatkę z `MapJson` i narysuj elementy SVG po współrzędnych `x/y`.
- Przy zmianie przedziału czasu odpytuj `Availability` i koloruj zajęte miejsca.
- Kliknięcie w wolne miejsce może prowadzić do rezerwacji (`Reservation/Create` lub `Reservation/GuestCreate`).

> Uwaga: W kolejnych krokach można dodać SignalR do odświeżania zajętości w czasie rzeczywistym oraz mechanizm „soft-hold” tymczasowo blokujący miejsce podczas wyboru.

