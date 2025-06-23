using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chat.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

[AllowAnonymous]
public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [TempData]
    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var httpClient = new HttpClient();
        var payload = new
        {
            username = Input.Username,
            password = Input.Password
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/auth/login", payload);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (content.Contains("Account is deactivated"))
            {
                ErrorMessage = "Konto zosta³o zablokowane, skontaktuj siê z administratorem.";
            }
            else
            {
                ErrorMessage = "Nieprawid³owa nazwa u¿ytkownika lub has³o.";
            }

            return Page();
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        HttpContext.Session.SetString("userId", authResponse.Id);
        HttpContext.Session.SetString("token", authResponse.Token);
        HttpContext.Session.SetString("username", authResponse.Username);
        HttpContext.Session.SetString("role", authResponse.Role);

        return RedirectToPage("/Chat");
    }

    [AllowAnonymous]
    public class LoginInput
    {
        [Required(ErrorMessage = "Nazwa u¿ytkownika jest wymagana")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Has³o jest wymagane")]
        public string Password { get; set; }
    }

    [AllowAnonymous]
    public class AuthResponse
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
