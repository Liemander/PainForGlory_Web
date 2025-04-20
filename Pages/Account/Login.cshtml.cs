using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace PainForGlory_Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public LoginModel(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();

            var apiUrl = _config["LoginApiUrl"] ?? "http://painforglory_loginserver:8080";
            var loginEndpoint = $"{apiUrl}/api/account/login";

            var loginData = new { Username, Password };
            var response = await client.PostAsJsonAsync(loginEndpoint, loginData);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result is not null)
                {
                    // 🔐 Store JWT in secure cookie
                    Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    });

                    return RedirectToPage("/Index");
                }

                ErrorMessage = "Could not parse token.";
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }

            return Page();
        }

        public class LoginResult
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
