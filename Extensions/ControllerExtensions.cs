using System.Security.Claims;

namespace UniversalReservationMVC.Extensions
{
    public static class ControllerExtensions
    {
        public static string? GetCurrentUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(Common.AppConstants.Roles.Admin);
        }

        public static bool IsAdminOrOwner(this ClaimsPrincipal user)
        {
            return user.IsInRole(Common.AppConstants.Roles.Admin) 
                || user.IsInRole(Common.AppConstants.Roles.Owner);
        }
    }
}
