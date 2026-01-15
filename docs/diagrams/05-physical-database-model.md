# Model Fizyczny Bazy Danych

## Szczegółowa struktura tabel w systemie rezerwacji

```mermaid
erDiagram
    AspNetUsers {
        nvarchar_450 Id PK "Identity User ID"
        nvarchar_256 UserName "Nazwa użytkownika"
        nvarchar_256 NormalizedUserName "Znormalizowana nazwa"
        nvarchar_256 Email UK "Adres email"
        nvarchar_256 NormalizedEmail "Znormalizowany email"
        bit EmailConfirmed "Czy email potwierdzony"
        nvarchar_max PasswordHash "Hash hasła"
        nvarchar_max SecurityStamp "Znacznik bezpieczeństwa"
        nvarchar_max ConcurrencyStamp "Znacznik współbieżności"
        nvarchar_max PhoneNumber "Numer telefonu"
        bit PhoneNumberConfirmed "Czy telefon potwierdzony"
        bit TwoFactorEnabled "Czy 2FA włączone"
        datetimeoffset LockoutEnd "Koniec blokady"
        bit LockoutEnabled "Czy blokada włączona"
        int AccessFailedCount "Licznik niepowodzeń"
        nvarchar_max FirstName "Imię"
        nvarchar_max LastName "Nazwisko"
        int Role "Rola (enum)"
    }
    
    AspNetRoles {
        nvarchar_450 Id PK "Role ID"
        nvarchar_256 Name UK "Nazwa roli"
        nvarchar_256 NormalizedName "Znormalizowana nazwa"
        nvarchar_max ConcurrencyStamp "Znacznik współbieżności"
    }
    
    AspNetUserRoles {
        nvarchar_450 UserId PK_FK "User ID"
        nvarchar_450 RoleId PK_FK "Role ID"
    }
    
    Companies {
        int Id PK "AUTO_INCREMENT"
        nvarchar_200 Name NOT_NULL "Nazwa firmy"
        nvarchar_1000 Description NULL "Opis"
        nvarchar_200 Address NULL "Adres"
        nvarchar_20 PhoneNumber NULL "Telefon"
        nvarchar_100 Email NULL "Email"
        nvarchar_100 Website NULL "Strona WWW"
        nvarchar_500 LogoUrl NULL "URL logo"
        nvarchar_450 OwnerId NOT_NULL_FK "ID właściciela"
        nvarchar_7 PrimaryColor NULL "Kolor główny (hex)"
        nvarchar_7 SecondaryColor NULL "Kolor dodatkowy (hex)"
        bit IsActive NOT_NULL "Czy aktywna"
        datetime CreatedAt NOT_NULL "Data utworzenia"
        datetime UpdatedAt NULL "Data modyfikacji"
    }
    
    CompanyMembers {
        int Id PK "AUTO_INCREMENT"
        int CompanyId NOT_NULL_FK "ID firmy"
        nvarchar_450 UserId NOT_NULL_FK "ID użytkownika"
        nvarchar_50 Role NOT_NULL "Rola w firmie"
        bit CanManageResources NOT_NULL "Uprawnienie: zasoby"
        bit CanViewReservations NOT_NULL "Uprawnienie: przeglądanie"
        bit CanManageReservations NOT_NULL "Uprawnienie: rezerwacje"
        bit CanManageEvents NOT_NULL "Uprawnienie: wydarzenia"
        bit CanViewAnalytics NOT_NULL "Uprawnienie: analityka"
        bit CanExportReports NOT_NULL "Uprawnienie: eksport"
        bit CanManageMembers NOT_NULL "Uprawnienie: członkowie"
        bit IsActive NOT_NULL "Czy aktywny"
        datetime JoinedAt NOT_NULL "Data dołączenia"
        datetime LastActivityAt NULL "Ostatnia aktywność"
    }
    
    Resources {
        int Id PK "AUTO_INCREMENT"
        nvarchar_200 Name NOT_NULL "Nazwa zasobu"
        int ResourceType NOT_NULL "Typ (enum)"
        nvarchar_500 Location NULL "Lokalizacja"
        nvarchar_2000 Description NULL "Opis"
        int SeatMapWidth NULL "Szerokość mapy miejsc"
        int SeatMapHeight NULL "Wysokość mapy miejsc"
        int Capacity NULL "Pojemność"
        decimal_18_2 Price NOT_NULL "Cena (PLN)"
        nvarchar_max WorkingHours NULL "Godziny pracy (JSON)"
        int CompanyId NULL_FK "ID firmy"
        datetime CreatedAt NOT_NULL "Data utworzenia"
        datetime UpdatedAt NULL "Data modyfikacji"
    }
    
    Seats {
        int Id PK "AUTO_INCREMENT"
        int ResourceId NOT_NULL_FK "ID zasobu"
        int X NOT_NULL "Współrzędna X"
        int Y NOT_NULL "Współrzędna Y"
        nvarchar_20 Label NULL "Etykieta (np. A1)"
        nvarchar_10 Row NULL "Rząd"
        int Column NULL "Kolumna"
        bit IsAvailable NOT_NULL "Czy dostępne"
    }
    
    Events {
        int Id PK "AUTO_INCREMENT"
        nvarchar_200 Title NOT_NULL "Tytuł wydarzenia"
        nvarchar_2000 Description NULL "Opis"
        int ResourceId NOT_NULL_FK "ID zasobu"
        datetime StartTime NOT_NULL "Czas rozpoczęcia"
        datetime EndTime NOT_NULL "Czas zakończenia"
        int ParentEventId NULL_FK "ID wydarzenia nadrzędnego"
    }
    
    RecurrencePatterns {
        int Id PK "AUTO_INCREMENT"
        int EventId NOT_NULL_FK "ID wydarzenia"
        nvarchar_50 RecurrenceType NOT_NULL "Typ powtarzania"
        int Interval NOT_NULL "Interwał"
        nvarchar_100 DaysOfWeek NULL "Dni tygodnia (CSV)"
        int DayOfMonth NULL "Dzień miesiąca"
        int MonthOfYear NULL "Miesiąc roku"
        datetime StartDate NOT_NULL "Data początkowa"
        datetime EndDate NULL "Data końcowa"
        int MaxOccurrences NULL "Max wystąpień"
    }
    
    Reservations {
        int Id PK "AUTO_INCREMENT"
        nvarchar_450 UserId NULL_FK "ID użytkownika"
        nvarchar_255 GuestEmail NULL "Email gościa"
        nvarchar_50 GuestPhone NULL "Telefon gościa"
        int ResourceId NOT_NULL_FK "ID zasobu"
        int SeatId NULL_FK "ID miejsca"
        int EventId NULL_FK "ID wydarzenia"
        datetime StartTime NOT_NULL "Czas rozpoczęcia"
        datetime EndTime NOT_NULL "Czas zakończenia"
        int Status NOT_NULL "Status (enum)"
        bit IsPaid NOT_NULL "Czy opłacone"
        datetime CreatedAt NOT_NULL "Data utworzenia"
        datetime UpdatedAt NULL "Data modyfikacji"
    }
    
    Tickets {
        int Id PK "AUTO_INCREMENT"
        int ReservationId NOT_NULL_FK "ID rezerwacji"
        decimal_18_2 Price NOT_NULL "Cena biletu"
        int Status NOT_NULL "Status (enum)"
        nvarchar_100 PurchaseReference NULL "Referencja zakupu"
        datetime CreatedAt NOT_NULL "Data utworzenia"
        datetime PurchasedAt NULL "Data zakupu"
    }
    
    Payments {
        int Id PK "AUTO_INCREMENT"
        int ReservationId NOT_NULL_FK "ID rezerwacji"
        nvarchar_100 StripePaymentIntentId NOT_NULL "Stripe Intent ID"
        nvarchar_100 StripeChargeId NULL "Stripe Charge ID"
        decimal_18_2 Amount NOT_NULL "Kwota"
        nvarchar_3 Currency NOT_NULL "Waluta (ISO)"
        int Status NOT_NULL "Status (enum)"
        datetime CreatedAt NOT_NULL "Data utworzenia"
        datetime PaidAt NULL "Data płatności"
        nvarchar_500 FailureReason NULL "Powód niepowodzenia"
        nvarchar_1000 Metadata NULL "Metadane (JSON)"
    }
    
    AspNetUsers ||--o{ AspNetUserRoles : "has"
    AspNetRoles ||--o{ AspNetUserRoles : "assigned to"
    AspNetUsers ||--o{ Companies : "owns"
    AspNetUsers ||--o{ CompanyMembers : "member of"
    AspNetUsers ||--o{ Reservations : "creates"
    
    Companies ||--o{ Resources : "owns"
    Companies ||--o{ CompanyMembers : "has"
    CompanyMembers }o--|| Companies : "belongs to"
    CompanyMembers }o--|| AspNetUsers : "is"
    
    Resources ||--o{ Seats : "contains"
    Resources ||--o{ Events : "hosts"
    Resources ||--o{ Reservations : "reserved"
    
    Seats ||--o{ Reservations : "reserved"
    
    Events ||--o{ Reservations : "linked"
    Events ||--o| RecurrencePatterns : "has pattern"
    Events ||--o{ Events : "parent of"
    
    Reservations }o--|| Resources : "for"
    Reservations }o--o| Seats : "at"
    Reservations }o--o| Events : "during"
    Reservations }o--o| AspNetUsers : "by"
    Reservations ||--o{ Tickets : "has"
    Reservations ||--o{ Payments : "paid by"
    
    Tickets }o--|| Reservations : "for"
    Payments }o--|| Reservations : "pays"
```

## Indeksy bazodanowe

### AspNetUsers
- `PK_AspNetUsers` PRIMARY KEY (`Id`)
- `IX_AspNetUsers_NormalizedUserName` UNIQUE (`NormalizedUserName`)
- `IX_AspNetUsers_NormalizedEmail` (`NormalizedEmail`)

### Companies
- `PK_Companies` PRIMARY KEY (`Id`)
- `IX_Companies_OwnerId` (`OwnerId`)
- `FK_Companies_AspNetUsers_OwnerId` FOREIGN KEY (`OwnerId`) REFERENCES `AspNetUsers`(`Id`) ON DELETE RESTRICT

### CompanyMembers
- `PK_CompanyMembers` PRIMARY KEY (`Id`)
- `IX_CompanyMembers_CompanyId_UserId` UNIQUE (`CompanyId`, `UserId`)
- `IX_CompanyMembers_UserId` (`UserId`)
- `IX_CompanyMembers_IsActive` (`IsActive`)
- `FK_CompanyMembers_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE CASCADE
- `FK_CompanyMembers_AspNetUsers` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers`(`Id`) ON DELETE CASCADE

### Resources
- `PK_Resources` PRIMARY KEY (`Id`)
- `IX_Resources_ResourceType` (`ResourceType`)
- `IX_Resources_CompanyId` (`CompanyId`)
- `FK_Resources_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE CASCADE

### Seats
- `PK_Seats` PRIMARY KEY (`Id`)
- `IX_Seats_ResourceId_X_Y` (`ResourceId`, `X`, `Y`)
- `FK_Seats_Resources` FOREIGN KEY (`ResourceId`) REFERENCES `Resources`(`Id`) ON DELETE CASCADE

### Events
- `PK_Events` PRIMARY KEY (`Id`)
- `IX_Events_ResourceId_StartTime` (`ResourceId`, `StartTime`)
- `IX_Events_ParentEventId` (`ParentEventId`)
- `FK_Events_Resources` FOREIGN KEY (`ResourceId`) REFERENCES `Resources`(`Id`) ON DELETE CASCADE
- `FK_Events_Events_ParentEventId` FOREIGN KEY (`ParentEventId`) REFERENCES `Events`(`Id`) ON DELETE NO ACTION

### RecurrencePatterns
- `PK_RecurrencePatterns` PRIMARY KEY (`Id`)
- `IX_RecurrencePatterns_EventId` (`EventId`)
- `FK_RecurrencePatterns_Events` FOREIGN KEY (`EventId`) REFERENCES `Events`(`Id`) ON DELETE CASCADE

### Reservations
- `PK_Reservations` PRIMARY KEY (`Id`)
- `IX_Reservations_UserId` (`UserId`)
- `IX_Reservations_ResourceId_StartTime_EndTime` (`ResourceId`, `StartTime`, `EndTime`)
- `IX_Reservations_Status` (`Status`)
- `IX_Reservations_SeatId` (`SeatId`)
- `IX_Reservations_EventId` (`EventId`)
- `FK_Reservations_Resources` FOREIGN KEY (`ResourceId`) REFERENCES `Resources`(`Id`) ON DELETE RESTRICT
- `FK_Reservations_Seats` FOREIGN KEY (`SeatId`) REFERENCES `Seats`(`Id`) ON DELETE RESTRICT
- `FK_Reservations_AspNetUsers` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers`(`Id`) ON DELETE RESTRICT
- `FK_Reservations_Events` FOREIGN KEY (`EventId`) REFERENCES `Events`(`Id`) ON DELETE SET NULL

### Tickets
- `PK_Tickets` PRIMARY KEY (`Id`)
- `IX_Tickets_ReservationId` (`ReservationId`)
- `IX_Tickets_Status` (`Status`)
- `FK_Tickets_Reservations` FOREIGN KEY (`ReservationId`) REFERENCES `Reservations`(`Id`) ON DELETE CASCADE

### Payments
- `PK_Payments` PRIMARY KEY (`Id`)
- `IX_Payments_ReservationId` (`ReservationId`)
- `IX_Payments_StripePaymentIntentId` UNIQUE (`StripePaymentIntentId`)
- `IX_Payments_Status` (`Status`)
- `FK_Payments_Reservations` FOREIGN KEY (`ReservationId`) REFERENCES `Reservations`(`Id`) ON DELETE CASCADE

## Typy enum

### ResourceType (int)
- 0 = Restaurant (Restauracja)
- 1 = Cinema (Kino)
- 2 = Office (Biuro)
- 3 = ConferenceRoom (Sala konferencyjna)
- 4 = Theatre (Teatr)

### ReservationStatus (int)
- 0 = Pending (Oczekująca)
- 1 = Confirmed (Potwierdzona)
- 2 = Cancelled (Anulowana)
- 3 = Completed (Zakończona)

### TicketStatus (int)
- 0 = Available (Dostępny)
- 1 = Reserved (Zarezerwowany)
- 2 = Purchased (Zakupiony)
- 3 = Cancelled (Anulowany)

### UserRole (int)
- 0 = Admin (Administrator)
- 1 = Owner (Właściciel)
- 2 = User (Użytkownik)
- 3 = Guest (Gość)

### PaymentStatus (int)
- 0 = Pending (Oczekująca)
- 1 = Processing (Przetwarzana)
- 2 = Succeeded (Zakończona sukcesem)
- 3 = Failed (Niepowodzenie)
- 4 = Refunded (Zwrócona)
- 5 = Cancelled (Anulowana)

## Zasady integralności referencyjnej

### ON DELETE RESTRICT
- `Companies.OwnerId` → `AspNetUsers.Id` (właściciel nie może być usunięty jeśli ma firmę)
- `Reservations.ResourceId` → `Resources.Id` (zasób nie może być usunięty jeśli ma rezerwacje)
- `Reservations.SeatId` → `Seats.Id` (miejsce nie może być usunięte jeśli ma rezerwacje)
- `Reservations.UserId` → `AspNetUsers.Id` (użytkownik nie może być usunięty jeśli ma rezerwacje)

### ON DELETE CASCADE
- `Companies.Id` → `Resources.CompanyId` (usunięcie firmy usuwa zasoby)
- `Companies.Id` → `CompanyMembers.CompanyId` (usunięcie firmy usuwa członków)
- `Resources.Id` → `Seats.ResourceId` (usunięcie zasobu usuwa miejsca)
- `Resources.Id` → `Events.ResourceId` (usunięcie zasobu usuwa wydarzenia)
- `Reservations.Id` → `Tickets.ReservationId` (usunięcie rezerwacji usuwa bilety)
- `Reservations.Id` → `Payments.ReservationId` (usunięcie rezerwacji usuwa płatności)
- `Events.Id` → `RecurrencePatterns.EventId` (usunięcie wydarzenia usuwa wzorzec)

### ON DELETE SET NULL
- `Reservations.EventId` → `Events.Id` (usunięcie wydarzenia odłącza rezerwacje)

### ON DELETE NO ACTION
- `Events.ParentEventId` → `Events.Id` (cykliczne wydarzenia wymagają ręcznej obsługi)

## Rozmiary i limity

### Limity długości pól tekstowych
- Email: 256 znaków (zgodnie z ASP.NET Identity)
- Nazwa firmy: 200 znaków
- Opis firmy: 1000 znaków
- Nazwa zasobu: 200 znaków
- Opis zasobu: 2000 znaków
- Tytuł wydarzenia: 200 znaków
- Etykieta miejsca: 20 znaków
- Numer telefonu: 50 znaków
- Referencja zakupu: 100 znaków

### Limity numeryczne
- Cena: decimal(18,2) - max 999,999,999,999,999.99
- Współrzędne miejsc (X, Y): 1-1000
- Szerokość/wysokość mapy: 1-100
- Pojemność zasobu: 1-10000

## Strategia przechowywania danych JSON

Niektóre pola przechowują dane w formacie JSON:
- **Resources.WorkingHours**: Godziny otwarcia dla każdego dnia tygodnia
  ```json
  {
    "monday": {"open": "09:00", "close": "17:00"},
    "tuesday": {"open": "09:00", "close": "17:00"}
  }
  ```
- **Payments.Metadata**: Dodatkowe informacje o płatności

## Charakterystyka wydajnościowa

### Optymalizacja zapytań
- Indeksy kompozytowe dla częstych zapytań (ResourceId + StartTime + EndTime)
- Indeksy na kolumnach używanych w WHERE, JOIN i ORDER BY
- Indeksy na kolumnach enumeracji (Status, Type)

### Strategia buforowania
- Dane zasobów (Resources) - rzadko zmieniane
- Dane firm (Companies) - rzadko zmieniane
- Miejsca siedzące (Seats) - rzadko zmieniane
- Rezerwacje - często zmieniane (bez buforowania lub krótki TTL)

### Partycjonowanie (dla dużych systemów)
- Tabela Reservations może być partycjonowana według StartTime
- Tabela Payments może być partycjonowana według CreatedAt
- Archiwizacja starych rezerwacji do osobnej tabeli (np. ReservationsArchive)
