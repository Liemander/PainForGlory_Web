using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PainForGlory_Web.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Remove the JWT cookie
            Response.Cookies.Delete("access_token");

            // Optional: clear the auth context
            HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

            // Redirect to home or login page
            return RedirectToPage("/Index");
        }
    }
}
