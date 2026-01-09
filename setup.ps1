# Setup script for UniversalReservationMVC
# Run this script after cloning the repository

Write-Host "=== UniversalReservationMVC Setup ===" -ForegroundColor Green
Write-Host ""

# Check if .NET SDK is installed
Write-Host "1. Checking .NET SDK..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ .NET SDK not found. Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check if dotnet-ef is installed
Write-Host ""
Write-Host "2. Checking Entity Framework tools..." -ForegroundColor Cyan
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"
if ($efInstalled) {
    Write-Host "   ✓ dotnet-ef is installed" -ForegroundColor Green
} else {
    Write-Host "   ! Installing dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "   ✓ dotnet-ef installed" -ForegroundColor Green
}

# Restore packages
Write-Host ""
Write-Host "3. Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "   ✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Check if database exists
Write-Host ""
Write-Host "4. Checking database..." -ForegroundColor Cyan
if (Test-Path "reservations.db") {
    Write-Host "   ! Database file 'reservations.db' already exists" -ForegroundColor Yellow
    $response = Read-Host "   Do you want to recreate it? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Remove-Item "reservations.db" -Force
        Remove-Item "reservations.db-shm" -ErrorAction SilentlyContinue -Force
        Remove-Item "reservations.db-wal" -ErrorAction SilentlyContinue -Force
        Write-Host "   ✓ Old database removed" -ForegroundColor Green
    }
}

# Create database
if (-not (Test-Path "reservations.db")) {
    Write-Host "   Creating database with migrations..." -ForegroundColor Cyan
    dotnet ef database update
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Database created successfully" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Failed to create database" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   ✓ Using existing database" -ForegroundColor Green
}

# Configure User Secrets (optional)
Write-Host ""
Write-Host "5. User Secrets configuration (optional)" -ForegroundColor Cyan
Write-Host "   Default admin account can be configured via User Secrets" -ForegroundColor Gray
$configSecrets = Read-Host "   Do you want to configure admin credentials now? (y/N)"
if ($configSecrets -eq 'y' -or $configSecrets -eq 'Y') {
    dotnet user-secrets init
    $adminPassword = Read-Host "   Enter admin password (min 6 chars, uppercase, digit, special char)" -AsSecureString
    $adminPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($adminPassword))
    dotnet user-secrets set "DefaultAdmin:Password" $adminPasswordPlain
    dotnet user-secrets set "DefaultAdmin:Email" "admin@example.com"
    Write-Host "   ✓ User Secrets configured" -ForegroundColor Green
} else {
    Write-Host "   ℹ Skipped. You can use test accounts:" -ForegroundColor Yellow
    Write-Host "     - owner1@example.com / Owner123!" -ForegroundColor Gray
    Write-Host "     - user1@example.com / User123!" -ForegroundColor Gray
}

# Build project
Write-Host ""
Write-Host "6. Building project..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Project built successfully" -ForegroundColor Green
} else {
    Write-Host "   ✗ Build failed" -ForegroundColor Red
    exit 1
}

# Summary
Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Application will be available at:" -ForegroundColor Cyan
Write-Host "  https://localhost:60292" -ForegroundColor White
Write-Host "  http://localhost:60293" -ForegroundColor White
Write-Host ""
Write-Host "Test accounts:" -ForegroundColor Cyan
Write-Host "  Owner: owner1@example.com / Owner123!" -ForegroundColor White
Write-Host "  User:  user1@example.com / User123!" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to start the application now, or Ctrl+C to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host ""
Write-Host "Starting application..." -ForegroundColor Green
dotnet run
