using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public RegisterModel(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config = config;
    }

    [BindProperty] public string Username { get; set; }
    [BindProperty] public string Email { get; set; }
    [BindProperty] public string Password { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var payload = new { Username, Email, Password };
        var client = _httpFactory.CreateClient();
        var baseUrl = _config["LoginApiUrl"] ?? "http://localhost:8080"; // note fallback updated for local

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PostAsync($"{baseUrl}/api/account/register",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("Login");
            }

            var errorText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(errorText))
            {
                ErrorMessage = $"Server responded with {(int)response.StatusCode} {response.ReasonPhrase}";
            }
            else
            {
                ErrorMessage = $"Server error ({(int)response.StatusCode}): {errorText}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Request failed: {ex.Message}";
        }

        return Page();
    }
}
