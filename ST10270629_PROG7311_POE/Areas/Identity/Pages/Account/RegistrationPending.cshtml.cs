using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ST10270629_PROG7311_POE.Areas.Identity.Pages.Account
{
    [AllowAnonymous] // Allow users who just registered (and aren't logged in) to see this page
    public class RegistrationPendingModel : PageModel
    {
        public void OnGet()
        {
            // Nothing needed here for a simple display page
        }
    }
}