using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace REDRFID.Areas.Identity.Pages.Account
{
    public class SignedInModel : PageModel
    {

        public void OnGet()
        {
                 Response.Redirect("/Index");
        }
    }
}
