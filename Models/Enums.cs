namespace UniversalReservationMVC.Models
{
    public enum ResourceType
    {
        Restaurant,
        Cinema,
        Office,
        ConferenceRoom,
        Theatre,
        Desk
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public enum TicketStatus
    {
        Available,
        Reserved,
        Purchased,
        Cancelled
    }

    public enum UserRole
    {
        Admin,
        Owner,
        User,
        Guest
    }
}
