# Setup Instructions

## Konfiguracja User Secrets (Wymagane)

Aplikacja używa User Secrets do przechowywania wrażliwych danych konfiguracyjnych.

### Krok 1: Inicjalizacja User Secrets

Uruchom w katalogu projektu:

```powershell
dotnet user-secrets init
```

### Krok 2: Ustaw hasło domyślnego administratora

```powershell
dotnet user-secrets set "DefaultAdmin:Password" "YourSecurePassword123!"
```

**Uwaga:** Hasło musi spełniać wymogi ASP.NET Core Identity:
- Minimum 6 znaków
- Przynajmniej jedna wielka litera
- Przynajmniej jedna cyfra
- Przynajmniej jeden znak specjalny

### Krok 3: (Opcjonalnie) Zmień email administratora

Domyślny email to `admin@example.com`. Możesz go zmienić w `appsettings.json` lub przez User Secrets:

```powershell
dotnet user-secrets set "DefaultAdmin:Email" "your-admin@example.com"
```

## Uruchomienie aplikacji

```powershell
# Przywróć pakiety
dotnet restore

# Zastosuj migracje (jeśli jeszcze nie wykonano)
dotnet ef database update

# Uruchom aplikację
dotnet run
```

## Zmienne środowiskowe (Produkcja)

W środowisku produkcyjnym użyj zmiennych środowiskowych lub Azure Key Vault:

```bash
export DefaultAdmin__Password="SecureProductionPassword!"
```

Lub w Azure App Service -> Configuration -> Application Settings:
- `DefaultAdmin:Password` = `SecureProductionPassword!`
