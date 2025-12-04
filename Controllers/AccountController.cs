using Microsoft.AspNetCore.Mvc;

namespace VirtualEventTicketingSystem.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("/Account/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            return Redirect($"/Identity/Account/Login?returnUrl={returnUrl}");
        }

        [HttpGet("/Account/Register")]
        public IActionResult Register(string? returnUrl = null)
        {
            return Redirect($"/Identity/Account/Register?returnUrl={returnUrl}");
        }

        [HttpGet("/Account/Logout")]
        public IActionResult Logout(string? returnUrl = null)
        {
            return Redirect($"/Identity/Account/Logout?returnUrl={returnUrl}");
        }

        [HttpGet("/Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return Redirect("/Identity/Account/AccessDenied");
        }
    }
}