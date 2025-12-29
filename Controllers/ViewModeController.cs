using Microsoft.AspNetCore.Mvc;

namespace UniversalReservationMVC.Controllers
{
    public class ViewModeController : Controller
    {
        [HttpPost]
        public IActionResult Toggle(string returnUrl)
        {
            var currentMode = HttpContext.Session.GetString("ViewMode") ?? "owner";
            var newMode = currentMode == "owner" ? "customer" : "owner";
            
            HttpContext.Session.SetString("ViewMode", newMode);
            
            return LocalRedirect(returnUrl ?? "/");
        }

        [HttpPost]
        public IActionResult SetOwnerMode(string returnUrl)
        {
            HttpContext.Session.SetString("ViewMode", "owner");
            return LocalRedirect(returnUrl ?? "/");
        }

        [HttpPost]
        public IActionResult SetCustomerMode(string returnUrl)
        {
            HttpContext.Session.SetString("ViewMode", "customer");
            return LocalRedirect(returnUrl ?? "/");
        }
    }
}
