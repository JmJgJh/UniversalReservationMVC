# Dokumentacja Diagramów - System Rezerwacji Uniwersalnych

## Przegląd

Ten katalog zawiera szczegółową dokumentację graficzną systemu UniversalReservationMVC. Wszystkie diagramy są stworzone w formacie Mermaid, który jest natywnie wspierany przez GitHub.

## Zawartość

### 1. [Diagram Przypadków Użycia](01-use-case-diagram.md)
**Use Case Diagram** - Przedstawia funkcjonalności systemu z perspektywy użytkowników.

- **Co pokazuje**: Interakcje między aktorami (Gość, Użytkownik, Właściciel, Administrator) a systemem
- **Aktorzy**: Gość, Użytkownik (User), Właściciel (Owner), Administrator (Admin)
- **Systemy zewnętrzne**: System płatności (Stripe), System email
- **Przypadki użycia**: 
  - Przeglądanie i rezerwacja zasobów
  - Zarządzanie rezerwacjami i wydarzeniami
  - Zarządzanie firmami i członkami
  - Płatności i bilety
  - Analityka i raporty

**Kiedy używać**: Podczas analizy wymagań, planowania funkcjonalności, prezentacji systemu interesariuszom.

---

### 2. [Diagram ERD](02-erd-diagram.md)
**Entity Relationship Diagram** - Przedstawia model danych i relacje między encjami.

- **Co pokazuje**: Struktura danych, relacje między tabelami, klucze główne i obce
- **Główne encje**: 
  - ApplicationUser (użytkownicy)
  - Company (firmy)
  - Resource (zasoby)
  - Reservation (rezerwacje)
  - Event (wydarzenia)
  - Seat (miejsca)
  - Ticket (bilety)
  - Payment (płatności)
- **Typy relacji**: One-to-Many, Many-to-One, One-to-One

**Kiedy używać**: Podczas projektowania bazy danych, rozszerzania modelu danych, analizy integralności referencyjnej.

---

### 3. [Diagram Architektury MVC](03-mvc-architecture-diagram.md)
**MVC Architecture Diagram** - Przedstawia architekturę warstwową aplikacji.

- **Co pokazuje**: Organizacja kodu w warstwy, przepływ danych, wzorce projektowe
- **Warstwy**:
  - Warstwa Klienta (Browser, JavaScript, SignalR)
  - Warstwa Prezentacji (Controllers, Views, ViewModels)
  - Warstwa Biznesowa (Services)
  - Warstwa Danych (Repositories, Unit of Work)
  - Warstwa Persystencji (Entity Framework Core, Database)
  - System Uwierzytelniania (ASP.NET Identity)
  - Middleware (Security, Rate Limiting, Localization)
  - Real-time Communication (SignalR Hubs)
- **Wzorce**: MVC, Repository Pattern, Unit of Work, Dependency Injection, Service Layer

**Kiedy używać**: Podczas onboardingu nowych developerów, refaktoryzacji, dodawania nowych funkcjonalności.

---

### 4. [Diagram Sekwencji - Rezerwacja Sali](04-sequence-diagram-reservation.md)
**Sequence Diagram** - Przedstawia szczegółowy przepływ procesu rezerwacji.

- **Co pokazuje**: Krok po kroku interakcje między komponentami podczas tworzenia rezerwacji
- **Komponenty**:
  - Użytkownik
  - Przeglądarka
  - Controller (ReservationController)
  - Middleware (Authentication, Rate Limiting)
  - Services (ReservationService, SeatMapService, EmailService)
  - Unit of Work i Repository
  - Baza danych
  - SignalR Hub
  - SMTP Server
- **Scenariusze**:
  - Główny przepływ (success path)
  - Obsługa konfliktów (error path)
  - Rezerwacja jako gość
  - Rezerwacja na wydarzenie
  - Rate limiting
  - Walidacja

**Kiedy używać**: Podczas debugowania problemów z rezerwacjami, implementacji podobnych przepływów, analizy wydajności.

---

### 5. [Model Fizyczny Bazy Danych](05-physical-database-model.md)
**Physical Database Model** - Przedstawia fizyczną strukturę bazy danych.

- **Co pokazuje**: Szczegółowa definicja tabel, kolumn, typów danych, indeksów i kluczy obcych
- **Zawartość**:
  - Definicje wszystkich tabel z typami danych
  - Klucze główne i obce
  - Indeksy i ich przeznaczenie
  - Zasady integralności referencyjnej (ON DELETE CASCADE/RESTRICT/SET NULL)
  - Typy enumeracji i ich wartości
  - Limity i ograniczenia pól
  - Strategia przechowywania JSON
  - Wskazówki optymalizacyjne
- **Tabele ASP.NET Identity**: AspNetUsers, AspNetRoles, AspNetUserRoles
- **Tabele aplikacji**: Companies, Resources, Seats, Events, Reservations, Tickets, Payments

**Kiedy używać**: Podczas migracji bazy danych, optymalizacji zapytań, dodawania nowych tabel, analizy wydajności.

---

## Jak przeglądać diagramy

### Na GitHub
Wszystkie diagramy w formacie Mermaid są automatycznie renderowane przez GitHub. Wystarczy otworzyć plik `.md` w przeglądarce GitHub.

### Lokalnie w VS Code
1. Zainstaluj rozszerzenie **Markdown Preview Mermaid Support**
2. Otwórz plik `.md`
3. Użyj `Ctrl+Shift+V` (lub `Cmd+Shift+V` na Mac) aby otworzyć podgląd

### W edytorach Mermaid online
Jeśli chcesz edytować diagramy, możesz użyć:
- [Mermaid Live Editor](https://mermaid.live/)
- [Mermaid Chart](https://www.mermaidchart.com/)

## Konwencje

### Język
- **Polski**: Wszystkie opisy, etykiety w diagramach i dokumentacja
- **Angielski**: Nazwy techniczne (klasy, metody, tabele) zgodnie z kodem

### Kolory w diagramach
- **Niebieski**: Aktorzy użytkownicy (Gość, User, Owner, Admin)
- **Żółty**: Systemy zewnętrzne (Stripe, Email)
- **Zielony**: Warstwa biznesowa (Services)
- **Różowy**: Warstwa danych (Repositories)
- **Fioletowy**: Baza danych

## Aktualizacja diagramów

Jeśli wprowadzasz zmiany w kodzie, które wpływają na:
- **Model danych** → Zaktualizuj ERD i Model fizyczny
- **Nowe funkcjonalności** → Zaktualizuj Use Case Diagram
- **Architektura** → Zaktualizuj MVC Architecture Diagram
- **Przepływy procesów** → Zaktualizuj Sequence Diagram lub dodaj nowy

## Wersjonowanie

Diagramy są wersjonowane razem z kodem w repozytorium Git. Historia zmian jest dostępna w historii commitów.

---

## Przydatne linki

- [Dokumentacja Mermaid](https://mermaid.js.org/)
- [Przykłady diagramów Mermaid](https://mermaid.js.org/intro/syntax-reference.html)
- [ASP.NET Core MVC Documentation](https://docs.microsoft.com/en-us/aspnet/core/mvc/overview)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

*Ostatnia aktualizacja: 2026-01-15*
