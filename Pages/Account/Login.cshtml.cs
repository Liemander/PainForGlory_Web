using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace PainForGlory_Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public LoginModel(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();

            var loginData = new
            {
                Username,
                Password
            };

            var apiBase = _config["LoginApiUrl"] ?? "http://painforglory_loginserver:8080";
            var response = await client.PostAsJsonAsync($"{apiBase}/api/account/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResult>();
                // TODO: Save accessToken (and refreshToken if desired)
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Invalid login attempt.";
                return Page();
            }
        }

        public class LoginResult
        {
            public string AccessToken { get; set; } = "";
            public string RefreshToken { get; set; } = "";
        }
    }
}
