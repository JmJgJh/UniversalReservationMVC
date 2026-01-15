# Diagram Sekwencji - Rezerwacja Sali

## Proces rezerwacji miejsca/zasobu w systemie

```mermaid
sequenceDiagram
    actor User as Użytkownik
    participant Browser as Przeglądarka
    participant Controller as ReservationController
    participant AuthMW as Authentication
    participant RateLimitMW as Rate Limiting
    participant ResSvc as ReservationService
    participant SeatSvc as SeatMapService
    participant UoW as Unit of Work
    participant Repo as Repository
    participant DB as Baza Danych
    participant SeatHub as SignalR SeatHub
    participant EmailSvc as EmailService
    participant SMTP as SMTP Server
    
    User->>Browser: Wybiera zasób i miejsce
    Browser->>Controller: GET /Reservation/Create?resourceId=1&seatId=5
    
    Controller->>AuthMW: Weryfikacja uwierzytelnienia
    AuthMW-->>Controller: Użytkownik zalogowany
    
    Controller->>RateLimitMW: Sprawdzenie limitu żądań
    RateLimitMW-->>Controller: OK (w limicie)
    
    Controller->>UoW: GetResourceById(1)
    UoW->>Repo: GetByIdAsync(1)
    Repo->>DB: SELECT * FROM Resources WHERE Id = 1
    DB-->>Repo: Resource data
    Repo-->>UoW: Resource object
    UoW-->>Controller: Resource object
    
    Controller->>UoW: GetSeatById(5)
    UoW->>Repo: GetByIdAsync(5)
    Repo->>DB: SELECT * FROM Seats WHERE Id = 5
    DB-->>Repo: Seat data
    Repo-->>UoW: Seat object
    UoW-->>Controller: Seat object
    
    Controller-->>Browser: Formularz rezerwacji (View)
    Browser-->>User: Wyświetla formularz
    
    User->>Browser: Wypełnia formularz (data, godziny)
    Browser->>Controller: POST /Reservation/Create (ReservationViewModel)
    
    Controller->>AuthMW: Weryfikacja uwierzytelnienia
    AuthMW-->>Controller: Użytkownik zalogowany
    
    Controller->>Controller: Walidacja ModelState
    
    alt ModelState jest nieprawidłowy
        Controller-->>Browser: Błędy walidacji
        Browser-->>User: Wyświetla błędy
    else ModelState jest prawidłowy
        Controller->>ResSvc: CreateReservationAsync(model)
        
        ResSvc->>ResSvc: Walidacja danych biznesowych
        
        ResSvc->>UoW: BeginTransaction()
        UoW-->>ResSvc: Transaction started
        
        ResSvc->>SeatSvc: CheckSeatAvailability(seatId, startTime, endTime)
        SeatSvc->>UoW: GetReservationsBySeatAndTime(seatId, startTime, endTime)
        UoW->>Repo: FindAsync(predicate)
        Repo->>DB: SELECT * FROM Reservations WHERE SeatId = 5 AND ...
        DB-->>Repo: Existing reservations
        Repo-->>UoW: Reservations list
        UoW-->>SeatSvc: Reservations list
        SeatSvc->>SeatSvc: Sprawdza konflikty
        
        alt Miejsce zajęte
            SeatSvc-->>ResSvc: Miejsce niedostępne
            ResSvc->>UoW: RollbackTransaction()
            ResSvc-->>Controller: Error (Konflikt rezerwacji)
            Controller-->>Browser: Komunikat o błędzie
            Browser-->>User: "Miejsce już zarezerwowane"
        else Miejsce dostępne
            SeatSvc-->>ResSvc: Miejsce dostępne
            
            ResSvc->>ResSvc: Tworzy obiekt Reservation
            ResSvc->>UoW: AddAsync(reservation)
            UoW->>Repo: AddAsync(reservation)
            Repo->>DB: INSERT INTO Reservations ...
            DB-->>Repo: Reservation created (Id)
            Repo-->>UoW: Reservation object
            UoW-->>ResSvc: Reservation object
            
            ResSvc->>UoW: CommitTransaction()
            UoW-->>ResSvc: Transaction committed
            
            ResSvc->>SeatHub: NotifySeatReserved(resourceId, seatId)
            SeatHub-->>Browser: Aktualizacja w czasie rzeczywistym
            Browser-->>User: Mapa miejsc odświeżona
            
            ResSvc->>EmailSvc: SendReservationConfirmationAsync(reservation)
            EmailSvc->>SMTP: Wysłanie emaila z potwierdzeniem
            SMTP-->>EmailSvc: Email wysłany
            EmailSvc-->>ResSvc: Potwierdzenie wysłania
            
            ResSvc-->>Controller: Success (Reservation object)
            Controller-->>Browser: Redirect to /Reservation/Details/{id}
            Browser-->>User: "Rezerwacja utworzona pomyślnie"
            
            Browser->>Controller: GET /Reservation/Details/{id}
            Controller->>UoW: GetReservationById(id)
            UoW->>Repo: GetByIdAsync(id) with includes
            Repo->>DB: SELECT * FROM Reservations r JOIN Resources res ... WHERE r.Id = {id}
            DB-->>Repo: Reservation with related data
            Repo-->>UoW: Reservation object
            UoW-->>Controller: Reservation object
            Controller-->>Browser: Szczegóły rezerwacji (View)
            Browser-->>User: Wyświetla potwierdzenie rezerwacji
        end
    end
```

## Opis przepływu procesu rezerwacji

### Faza 1: Wyświetlenie formularza rezerwacji

1. **Użytkownik** wybiera zasób i opcjonalnie miejsce siedzące
2. **Przeglądarka** wysyła żądanie GET do `/Reservation/Create`
3. **Middleware** weryfikuje uwierzytelnienie i rate limiting
4. **Controller** pobiera dane zasobu i miejsca z bazy danych przez Unit of Work
5. **Widok** z formularzem jest zwracany użytkownikowi

### Faza 2: Walidacja i sprawdzenie dostępności

6. **Użytkownik** wypełnia formularz (data rozpoczęcia, zakończenia, opcjonalne dane)
7. **Przeglądarka** wysyła żądanie POST z danymi rezerwacji
8. **Controller** waliduje dane wejściowe (ModelState)
9. **ReservationService** wykonuje walidację biznesową
10. **SeatMapService** sprawdza dostępność miejsca w podanym czasie
    - Sprawdzenie konfliktów z istniejącymi rezerwacjami
    - Sprawdzenie godzin otwarcia zasobu

### Faza 3: Tworzenie rezerwacji (success path)

11. **Unit of Work** rozpoczyna transakcję bazodanową
12. **ReservationService** tworzy obiekt rezerwacji
13. **Repository** zapisuje rezerwację do bazy danych
14. **Unit of Work** commituje transakcję
15. **SignalR Hub** wysyła powiadomienie do wszystkich połączonych klientów o zmianie dostępności
16. **EmailService** wysyła email z potwierdzeniem rezerwacji
17. **Controller** przekierowuje użytkownika do szczegółów rezerwacji
18. **Przeglądarka** wyświetla potwierdzenie

### Faza 3a: Obsługa konfliktu (error path)

- Jeśli miejsce jest zajęte w wybranym czasie:
  - Transakcja jest wycofywana (rollback)
  - Użytkownik otrzymuje komunikat o błędzie
  - Formularz jest ponownie wyświetlany z błędem

### Faza 4: Wyświetlenie szczegółów

19. **Użytkownik** jest przekierowany do strony szczegółów rezerwacji
20. **Controller** pobiera pełne dane rezerwacji z powiązanymi encjami
21. **Widok** wyświetla potwierdzenie z wszystkimi szczegółami

## Alternatywne ścieżki

### Scenariusz 1: Rezerwacja jako gość (bez logowania)

Dla gości (niezalogowanych użytkowników):
- Pomijane jest uwierzytelnienie
- Wymagane jest podanie emaila lub telefonu (GuestEmail/GuestPhone)
- Rezerwacja jest tworzona bez powiązania z użytkownikiem (UserId = null)
- Email z potwierdzeniem jest wysyłany na GuestEmail

### Scenariusz 2: Rezerwacja na wydarzenie

Jeśli rezerwacja jest tworzona dla konkretnego wydarzenia:
- EventId jest przekazywane w żądaniu
- Godziny są automatycznie ustawiane na podstawie wydarzenia
- Rezerwacja jest powiązana z wydarzeniem

### Scenariusz 3: Przekroczenie limitu żądań (Rate Limiting)

Jeśli użytkownik przekroczy limit żądań:
- Middleware Rate Limiting zwraca błąd 429 (Too Many Requests)
- Użytkownik musi poczekać przed kolejną próbą

### Scenariusz 4: Błąd walidacji

Jeśli dane formularza są nieprawidłowe:
- ModelState zawiera błędy
- Formularz jest ponownie wyświetlany z komunikatami o błędach
- Żadne dane nie są zapisywane w bazie

## Wzorce wykorzystane w procesie

- **Transaction Management**: Unit of Work zarządza transakcją
- **Repository Pattern**: Abstrakcja dostępu do danych
- **Service Layer**: Logika biznesowa w ReservationService
- **Real-time Communication**: SignalR dla aktualizacji w czasie rzeczywistym
- **Asynchronous Processing**: Wysyłanie emaili asynchronicznie
- **Middleware Pipeline**: Authentication, Rate Limiting
- **Error Handling**: Rollback transakcji przy błędzie
