namespace UniversalReservationMVC.Common
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Owner = "Owner"; // Company owner
            public const string User = "User";
            public const string Guest = "Guest";
            public const string AdminOrOwner = "Admin,Owner";
            public const string OwnerOrAdmin = "Owner,Admin";
        }

        public static class SeatHold
        {
            public const int DefaultTTLSeconds = 90;
            public const int MaxTTLSeconds = 300;
        }

        public static class Claims
        {
            public const string UserId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        }

        public static class PageTitles
        {
            public const string CompanyDashboard = "Panel Firmy";
            public const string CompanyCreate = "Nowa Firma";
            public const string CompanyEdit = "Edytuj Firmę";
            public const string ResourceManagement = "Zarządzanie Zasobami";
            public const string SeatMapBuilder = "Konstruktor Mapy Pomieszczeń";
        }
    }
}
