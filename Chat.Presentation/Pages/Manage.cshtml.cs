using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chat.Application.DTOs;

namespace Chat.Presentation.Pages
{
    public class ManageModel : PageModel
    {
        public List<UserResponse> Users { get; set; } = new();
        public string CurrentUsername { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("token");
            var role = HttpContext.Session.GetString("role");
            CurrentUsername = HttpContext.Session.GetString("username");

            if (string.IsNullOrEmpty(token) || role != "Admin")
                return RedirectToPage("/Login");

            var url = "http://localhost:5000/api/users";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var result = await httpClient.GetFromJsonAsync<List<UserResponse>>(url);
                Users = result ?? new List<UserResponse>();
            }
            catch
            {
                Users = new List<UserResponse>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Add("X-Request-Source", "Strona");
            httpClient.DefaultRequestHeaders.Add("User-Agent", Request.Headers["User-Agent"].ToString());

            var url = $"http://localhost:5000/api/users?id={id}";
            var response = await httpClient.DeleteAsync(url);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(Guid id, bool active)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Add("X-Request-Source", "Strona");
            httpClient.DefaultRequestHeaders.Add("User-Agent", Request.Headers["User-Agent"].ToString());

            var getUrl = $"http://localhost:5000/api/users";
            var getResponse = await httpClient.GetFromJsonAsync<List<UserResponse>>(getUrl);
            var user = getResponse?.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return RedirectToPage();

            var request = new
            {
                username = user.Username,
                password = "placeholder",
                role = user.Role,
                active = active
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var putUrl = $"http://localhost:5000/api/users?id={id}";
            var response = await httpClient.PutAsync(putUrl, content);

            return RedirectToPage();
        }

    }
}
