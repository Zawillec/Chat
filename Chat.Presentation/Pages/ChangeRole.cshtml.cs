using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chat.Presentation.Pages;

public class ChangeRoleModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public string SelectedRole { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        var url = $"http://localhost:5000/api/users?id={Id}";

        var updateRequest = new
        {
            Username = "",
            Password = "",
            Role = SelectedRole,
            Active = true
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await httpClient.PutAsync(url, content);

        return RedirectToPage("/Manage");
    }
}
