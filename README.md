# UniversalReservationMVC

System rezerwacji online dla różnych typów zasobów: restauracje, kina, teatry, biura, sale konferencyjne.

## 🚀 Technologie

- **ASP.NET Core 8 MVC** - framework webowy
- **ASP.NET Core Identity** - autoryzacja i uwierzytelnianie z rolami (Admin, Owner, User, Guest)
- **Entity Framework Core** - ORM do zarządzania bazą danych
- **SQLite** - domyślna baza danych (łatwa zmiana na SQL Server)
- **SignalR** - komunikacja real-time dla mapy miejsc
- **Bootstrap 5** - responsywny interfejs użytkownika
- **Serilog** - zaawansowane logowanie
- **xUnit** - testy jednostkowe

## 📋 Funkcje

- ✅ System ról użytkowników (Admin, Owner, User, Guest)
- ✅ Zarządzanie firmami i ich zasobami
- ✅ Interaktywna mapa miejsc z wyborem miejsc (CSS Grid)
- ✅ Rezerwacje z obsługą konfliktów czasowych
- ✅ Rezerwacje dla gości (bez konta)
- ✅ Zarządzanie wydarzeniami
- ✅ System biletów
- ✅ Płatności (integracja przygotowana)
- ✅ Real-time aktualizacja dostępności miejsc
- ✅ Dashboard analityczny dla właścicieli
- ✅ Eksport raportów do Excel
- ✅ Wielojęzyczność (PL/EN)
- ✅ Tryb ciemny/jasny

## 🛠️ Wymagania

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows PowerShell, Command Prompt lub Terminal
- Edytor kodu (Visual Studio, VS Code, Rider)

## 📦 Instalacja i uruchomienie

### Krok 1: Klonowanie repozytorium

```powershell
git clone https://github.com/JmJgJh/UniversalReservationMVC.git
cd UniversalReservationMVC
```

### Krok 2: Przywracanie pakietów

```powershell
dotnet restore
```

### Krok 3: Konfiguracja User Secrets (opcjonalne)

Ustaw hasło dla domyślnego administratora:

```powershell
dotnet user-secrets init
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
dotnet user-secrets set "DefaultAdmin:Email" "admin@example.com"
```

> **Uwaga:** Jeśli nie skonfigurujesz hasła admina, będziesz mógł zalogować się kontami testowymi:
> - **Owner:** owner1@example.com / Owner123!
> - **User:** user1@example.com / User123!

### Krok 4: Aktualizacja bazy danych

Jeśli baza danych nie istnieje lub potrzebujesz zastosować nowe migracje:

```powershell
dotnet ef database update -c UniversalReservationMVC.Data.ApplicationDbContext
```

Baza danych SQLite zostanie utworzona jako `reservations.db` w głównym folderze projektu.

### Krok 5: Uruchomienie aplikacji

```powershell
dotnet run
```

Aplikacja uruchomi się domyślnie na:
- **HTTPS:** https://localhost:60292
- **HTTP:** http://localhost:60293

Otwórz przeglądarkę i przejdź do jednego z powyższych adresów.

### Krok 6: Logowanie

Aplikacja zawiera przykładowe dane testowe:

**Właściciele firm:**
- Email: `owner1@example.com` | Hasło: `Owner123!` (Cinema Centrum)
- Email: `owner2@example.com` | Hasło: `Owner123!` (CoWork Space)
- Email: `owner3@example.com` | Hasło: `Owner123!` (Restauracja)

**Zwykli użytkownicy:**
- Email: `user1@example.com` | Hasło: `User123!`
- Email: `user2@example.com` | Hasło: `User123!`

**Administrator** (jeśli skonfigurowano User Secrets):
- Email: `admin@example.com` | Hasło: `Admin123!`

## 🗂️ Struktura projektu

```
UniversalReservationMVC/
├── Controllers/         # Kontrolery MVC
├── Models/             # Modele domenowe
├── ViewModels/         # ViewModels dla widoków
├── Views/              # Widoki Razor
├── Data/               # DbContext i seedowanie
├── Services/           # Logika biznesowa
├── Repositories/       # Wzorzec Repository
├── Middleware/         # Custom middleware
├── Hubs/               # SignalR hubs
├── Migrations/         # Migracje EF Core
├── wwwroot/           # Pliki statyczne (CSS, JS)
└── appsettings.json   # Konfiguracja aplikacji
```

## 🔄 Zmiana na SQL Server

Aby użyć SQL Server zamiast SQLite:

1. Zainstaluj pakiet NuGet:
```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

2. Zmień connection string w `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=UniversalReservation;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

3. Zmień provider w `Program.cs`:
```csharp
// Zamień UseSqlite na UseSqlServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

## 🧪 Uruchomienie testów

```powershell
dotnet test
```

## 📊 Funkcje według ról

### Administrator
- Pełny dostęp do wszystkich danych
- Zarządzanie użytkownikami i firmami
- Raporty systemowe

### Owner (Właściciel firmy)
- Zarządzanie swoją firmą
- Dodawanie/edycja zasobów
- Konfiguracja map miejsc
- Zarządzanie członkami zespołu
- Przeglądanie rezerwacji
- Raporty finansowe i analityczne

### User (Użytkownik)
- Przeglądanie dostępnych zasobów
- Rezerwowanie miejsc/zasobów
- Zarządzanie swoimi rezerwacjami
- Kupowanie biletów

### Guest (Gość)
- Rezerwacje bez konta (z emailem/telefonem)
- Przeglądanie dostępnych zasobów

## 🎯 Najważniejsze funkcje

### ✅ Zaimplementowane

- [x] Inicjalizacja projektu ASP.NET Core 8 MVC
- [x] Konfiguracja ASP.NET Core Identity z rolami (Admin, Owner, User, Guest)
- [x] EF Core + SQLite (domyślna baza `reservations.db`)
- [x] Model `Resource` - generyczne zasoby rezerwowalne
- [x] Model `Seat` - mapa miejsc (grid X,Y)
- [x] Model `Reservation` - rezerwacje z obsługą gościa (bez konta)
- [x] Model `Event` - zdarzenia powiązane z zasobami
- [x] Model `Ticket` - bilety/przepustki
- [x] Model `Company` - zarządzanie firmami i ich członkami
- [x] `ReservationService` - logika rezerwacji i sprawdzanie konfliktów
- [x] `SeatHoldService` - tymczasowe blokowanie miejsc
- [x] `PaymentService` - obsługa płatności (przygotowane do integracji)
- [x] `EmailService` - wysyłanie powiadomień email
- [x] `ReportService` - generowanie raportów Excel
- [x] Kontrolery: Reservation, Resource, Event, Ticket, Company, Account
- [x] Widoki z responsywnym designem (Bootstrap 5)
- [x] Interaktywna mapa miejsc (CSS Grid z real-time aktualizacją)
- [x] Dashboard dla właścicieli firm z analityką
- [x] System uprawnień dla członków firmy
- [x] Walidacja formularzy i obsługa błędów
- [x] Middleware do globalnej obsługi wyjątków
- [x] Rate limiting dla endpointów API
- [x] Lokalizacja (PL/EN)
- [x] Tryb ciemny/jasny
- [x] SignalR dla real-time komunikacji
- [x] Migracje EF Core w `/Migrations`
- [x] Seeding danych testowych
- [x] Testy jednostkowe (xUnit)

### 📋 Planowane

- [ ] Rozszerzony dashboard analytics (wykresy, statystyki czasowe)
- [ ] System powiadomień push (PWA)
- [ ] QR kody dla biletów
- [ ] Automatyczne przypomnienia o rezerwacjach
- [ ] Obsługa anulowania rezerwacji z refundacją
- [ ] Automatyczne archiwizowanie starych rezerwacji
- [ ] Export rezerwacji do PDF
- [ ] Integracja kalendarza (Google Calendar, Outlook)
- [ ] API REST z dokumentacją Swagger (częściowo zaimplementowane)
- [ ] Containeryzacja (Docker)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] System rabatów/kodów promocyjnych
- [ ] Integracja testów e2e (Playwright)
- [ ] Mobile app (Xamarin/MAUI)

## 🐛 Rozwiązywanie problemów

### Brak interfejsu (tylko linki)
Jeśli po uruchomieniu widzisz tylko listę linków bez stylów:
- Upewnij się, że istnieje plik `Views/_ViewStart.cshtml`
- Wykonaj `Ctrl+F5` w przeglądarce (hard refresh)

### Błędy migracji
```powershell
# Usuń bazę danych i utwórz od nowa
Remove-Item reservations.db
dotnet ef database update
```

### Szara mapa miejsc
Sprawdź konsolę przeglądarki (F12) - może brakować danych w bazie. Upewnij się, że seeding danych się wykonał.

## 📝 Licencja

Ten projekt jest tworzony w celach edukacyjnych.

## 👥 Autorzy

Projekt stworzony jako część pracy dyplomowej.

## 🔗 Linki

- [Dokumentacja ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5](https://getbootstrap.com/)

---

**Wersja:** 2.0  
**Ostatnia aktualizacja:** Grudzień 2025
