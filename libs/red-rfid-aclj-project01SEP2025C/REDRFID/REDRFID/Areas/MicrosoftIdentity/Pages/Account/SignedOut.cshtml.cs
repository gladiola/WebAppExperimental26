using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HW5NoteKeeperD.Areas.MicrosoftIdentity.Pages.Account
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
