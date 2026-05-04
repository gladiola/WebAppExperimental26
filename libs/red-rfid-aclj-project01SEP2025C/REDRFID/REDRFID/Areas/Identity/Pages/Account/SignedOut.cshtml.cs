using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace REDRFID.Areas.Identity.Pages.Account
{
    public class SignedOutModel : PageModel
    {
        public async Task OnGet()
        {
            await Task.Delay(1);
            Response.Redirect("/Index");
        }
    }
}
