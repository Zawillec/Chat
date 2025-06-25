using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text;
using Chat.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Chat.Presentation.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(ILogger<LoginModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

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
            httpClient.DefaultRequestHeaders.Add("X-Request-Source", "Strona");
            httpClient.DefaultRequestHeaders.Add("User-Agent", Request.Headers["User-Agent"].ToString());

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
                    ErrorMessage = "Konto zosta³o zablokowane, skontaktuj siê z administratorem.";
                else
                    ErrorMessage = "Nieprawid³owa nazwa u¿ytkownika lub has³o.";
                return Page();
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            HttpContext.Session.SetString("userId", authResponse.Id);
            HttpContext.Session.SetString("token", authResponse.Token);
            HttpContext.Session.SetString("username", authResponse.Username);
            HttpContext.Session.SetString("role", authResponse.Role);

            return RedirectToPage("/Chat");
        }


        public class LoginInput
        {
            [Required(ErrorMessage = "Nazwa u¿ytkownika jest wymagana")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Has³o jest wymagane")]
            public string Password { get; set; }
        }
    }
}
