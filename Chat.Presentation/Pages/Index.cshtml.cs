using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Registered { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool LoggedOut { get; set; }
}
