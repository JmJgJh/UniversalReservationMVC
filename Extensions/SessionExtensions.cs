using Microsoft.AspNetCore.Http;

namespace UniversalReservationMVC.Extensions
{
    public static class SessionExtensions
    {
        public static bool IsOwnerViewMode(this ISession session)
        {
            var viewMode = session.GetString("ViewMode");
            return string.IsNullOrEmpty(viewMode) || viewMode == "owner";
        }

        public static bool IsCustomerViewMode(this ISession session)
        {
            var viewMode = session.GetString("ViewMode");
            return viewMode == "customer";
        }
    }
}
