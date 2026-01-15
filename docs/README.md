# Dokumentacja Projektu UniversalReservationMVC

## Przegląd

System Rezerwacji Uniwersalnych (UniversalReservationMVC) to zaawansowana aplikacja webowa do zarządzania rezerwacjami różnego rodzaju zasobów: restauracji, kin, sal konferencyjnych, biur i teatrów.

## Struktura Dokumentacji

### 📊 [Diagramy](diagrams/)

Kompleksowa dokumentacja graficzna systemu zawierająca:

1. **[Diagram Przypadków Użycia](diagrams/01-use-case-diagram.md)**
   - Funkcjonalności systemu z perspektywy użytkowników
   - Role: Gość, Użytkownik, Właściciel, Administrator
   - Integracje z systemami zewnętrznymi

2. **[Diagram ERD (Entity Relationship Diagram)](diagrams/02-erd-diagram.md)**
   - Model danych i relacje między encjami
   - Klucze główne i obce
   - Kardynalność relacji

3. **[Diagram Architektury MVC](diagrams/03-mvc-architecture-diagram.md)**
   - Architektura warstwowa aplikacji
   - Wzorce projektowe (Repository, Unit of Work, DI)
   - Przepływ danych między warstwami

4. **[Diagram Sekwencji - Rezerwacja Sali](diagrams/04-sequence-diagram-reservation.md)**
   - Szczegółowy przepływ procesu rezerwacji
   - Interakcje między komponentami
   - Scenariusze alternatywne i obsługa błędów

5. **[Model Fizyczny Bazy Danych](diagrams/05-physical-database-model.md)**
   - Szczegółowa struktura tabel
   - Typy danych i ograniczenia
   - Indeksy i klucze obce
   - Optymalizacja wydajności

## Technologie

### Backend
- **ASP.NET Core 8 MVC** - Framework aplikacji webowej
- **Entity Framework Core** - ORM do komunikacji z bazą danych
- **ASP.NET Identity** - System uwierzytelniania i autoryzacji
- **SQLite / MSSQL** - Baza danych (SQLite domyślnie, możliwość zmiany na MSSQL)
- **SignalR** - Komunikacja w czasie rzeczywistym

### Frontend
- **Razor Views** - Szablony widoków
- **Bootstrap 5** - Framework CSS
- **JavaScript/jQuery** - Interaktywność po stronie klienta
- **SignalR Client** - WebSocket dla aktualizacji na żywo

### Integracje
- **Stripe** - Płatności online
- **SMTP** - Wysyłanie emaili
- **Serilog** - Logowanie
- **EPPlus** - Generowanie raportów Excel

### Middleware & Security
- **Rate Limiting** - Ochrona przed nadużyciami
- **Security Headers** - CSP, X-Frame-Options, XSS Protection
- **Localization** - Wsparcie dla języków PL/EN
- **Response Caching** - Optymalizacja wydajności

## Główne Funkcjonalności

### 🏢 Zarządzanie Firmami
- Tworzenie i zarządzanie profilami firm
- System członków z uprawnieniami
- Przypisywanie zasobów do firm

### 📍 Zarządzanie Zasobami
- Definiowanie różnych typów zasobów (restauracja, kino, sala konferencyjna, itp.)
- Konfiguracja map miejsc siedzących (X, Y coordinates)
- Ustawienie godzin otwarcia i cen

### 📅 System Rezerwacji
- Rezerwacja dla zalogowanych użytkowników
- Rezerwacja dla gości (bez rejestracji)
- Wybór konkretnych miejsc siedzących
- Walidacja konfliktów rezerwacji
- Powiadomienia email

### 🎭 Wydarzenia
- Tworzenie wydarzeń na zasobach
- Wzorce powtarzania (cykliczne wydarzenia)
- Rezerwacje powiązane z wydarzeniami

### 🎫 Bilety i Płatności
- System sprzedaży biletów
- Integracja z Stripe
- Historia płatności
- Status transakcji

### 📊 Analityka i Raporty
- Dashboard ze statystykami
- Eksport danych do CSV/Excel/PDF
- Analiza przychodów i rezerwacji

### ⚡ Real-time Updates
- Aktualizacje dostępności miejsc na żywo
- Blokowanie miejsc podczas rezerwacji
- SignalR WebSocket

## Architektura

System wykorzystuje architekturę warstwową:

```
┌─────────────────────────────────────────┐
│         Warstwa Prezentacji             │
│    (Controllers, Views, ViewModels)     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│        Warstwa Biznesowa                │
│          (Services)                     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│          Warstwa Danych                 │
│  (Repositories, Unit of Work)           │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│       Warstwa Persystencji              │
│  (Entity Framework Core, Database)      │
└─────────────────────────────────────────┘
```

Szczegóły w [Diagramie Architektury MVC](diagrams/03-mvc-architecture-diagram.md).

## Model Danych

### Główne Encje

- **ApplicationUser** - Użytkownicy systemu (rozszerzenie Identity)
- **Company** - Firmy będące właścicielami zasobów
- **CompanyMember** - Członkowie firm z uprawnieniami
- **Resource** - Zasoby do rezerwacji
- **Seat** - Miejsca siedzące w zasobach
- **Event** - Wydarzenia odbywające się w zasobach
- **Reservation** - Rezerwacje zasobów/miejsc
- **Ticket** - Bilety na wydarzenia
- **Payment** - Płatności za rezerwacje

Szczegóły w [Diagramie ERD](diagrams/02-erd-diagram.md) i [Modelu Fizycznym](diagrams/05-physical-database-model.md).

## Rozpoczęcie Pracy

### Wymagania
- .NET 8 SDK
- SQLite (domyślnie) lub MSSQL Server
- IDE: Visual Studio 2022, VS Code, lub Rider

### Instalacja

```powershell
# Sklonuj repozytorium
git clone https://github.com/JmJgJh/UniversalReservationMVC.git
cd UniversalReservationMVC

# Przywróć pakiety
dotnet restore

# Utwórz bazę danych
dotnet ef migrations add InitialCreate
dotnet ef database update

# Ustaw hasło administratora (opcjonalnie)
dotnet user-secrets set "DefaultAdmin:Password" "YourSecurePassword123!"

# Uruchom aplikację
dotnet run
```

Aplikacja będzie dostępna pod adresem: https://localhost:5001

### Konfiguracja

Plik `appsettings.json` zawiera konfigurację:
- Connection string (SQLite domyślnie)
- Email settings (SMTP)
- Stripe API keys
- Logging settings

## Bezpieczeństwo

System implementuje następujące mechanizmy bezpieczeństwa:

- ✅ ASP.NET Identity z hashowaniem haseł
- ✅ Role-based authorization (Admin, Owner, User, Guest)
- ✅ CSRF protection
- ✅ XSS protection (Content Security Policy)
- ✅ SQL Injection protection (Entity Framework parameterization)
- ✅ Rate limiting (ochrona przed nadużyciami)
- ✅ Security headers (X-Frame-Options, X-Content-Type-Options)
- ✅ HTTPS enforcement
- ✅ Input validation and sanitization
- ✅ Secure session management

## Testowanie

```powershell
# Uruchom testy jednostkowe
cd UniversalReservationMVC.Tests
dotnet test
```

## Deployment

### SQLite (Development)
- Domyślna konfiguracja
- Baza danych w pliku `reservations.db`

### MSSQL (Production)
1. Zaktualizuj `appsettings.json` connection string
2. Zmień provider w `Program.cs` z `UseSqlite` na `UseSqlServer`
3. Dodaj pakiet `Microsoft.EntityFrameworkCore.SqlServer`
4. Wykonaj migracje: `dotnet ef database update`

## Licencja

Ten projekt jest licencjonowany na podstawie licencji określonej przez właściciela repozytorium.

## Wsparcie

W razie pytań lub problemów:
- Otwórz Issue na GitHub
- Sprawdź [dokumentację diagramów](diagrams/)
- Sprawdź logi aplikacji w katalogu `logs/`

---

*Ostatnia aktualizacja: 2026-01-15*
