using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Input.Password != Input.ConfirmPassword)
        {
            ModelState.AddModelError("Input.ConfirmPassword", "Has³a nie s¹ zgodne.");
            return Page();
        }

        var httpClient = new HttpClient();
        var payload = new
        {
            username = Input.Username,
            password = Input.Password,
            role = "User"
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/auth/register", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            if (errorContent.Contains("User already exists"))
            {
                ModelState.AddModelError("Input.Username", "Taki u¿ytkownik ju¿ istnieje.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Rejestracja nie powiod³a siê.");
            }

            return Page();
        }

        return RedirectToPage("/Index", new { registered = true });
    }

    public class RegisterInput
    {
        [Required(ErrorMessage = "Nazwa u¿ytkownika jest wymagana")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Has³o jest wymagane")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Powtórzenie has³a jest wymagane")]
        [Display(Name = "Powtórz has³o")]
        public string ConfirmPassword { get; set; }
    }
}
