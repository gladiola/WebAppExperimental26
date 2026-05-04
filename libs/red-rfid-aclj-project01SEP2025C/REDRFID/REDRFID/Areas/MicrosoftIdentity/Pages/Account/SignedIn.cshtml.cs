
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HW5NoteKeeperD.Areas.MicrosoftIdentity.Pages.Account
{
    public class SignedInModel : PageModel
    {

        public void OnGet()
        {
                Response.Redirect("/Index");
        }
    }
}
