using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PainForGlory_Web.Helpers;
using PainForGlory_Common.DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PainForGlory_Web.Pages.Account
{
    public class AccountModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AccountModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [BindProperty] public string? NewUsername { get; set; }
        [BindProperty] public string? NewEmail { get; set; }
        [BindProperty] public string? NewPassword { get; set; }
        [BindProperty] public string? ConfirmPassword { get; set; }
        [BindProperty] public string? CurrentPassword { get; set; }


        public string? CurrentEmail { get; set; }
        public List<PreviousAccountInfo> PreviousInfos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var username = GetUsernameFromJwt();
            if (username == null) return RedirectToPage("/Account/Login");

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _config["LoginServer:BaseUrl"];
            if (Request.Cookies.TryGetValue("access_token", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }


            // Fetch account info
            var response = await client.GetAsync($"{apiUrl}/api/account/info");
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AccountInfo>(
                    await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                CurrentEmail = result?.Email;
            }

            // Fetch history
            var history = await client.GetAsync($"{apiUrl}/api/account/history");
            if (history.IsSuccessStatusCode)
            {
                var historyList = JsonSerializer.Deserialize<List<PreviousAccountInfo>>(
                    await history.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (historyList != null) PreviousInfos = historyList;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = GetUsernameFromJwt();
            if (username == null) return RedirectToPage("/Account/Login");

            if (string.IsNullOrWhiteSpace(NewUsername) && string.IsNullOrWhiteSpace(NewEmail) && string.IsNullOrWhiteSpace(NewPassword))
            {
                ModelState.AddModelError(string.Empty, "No changes detected.");
                return await OnGetAsync(); // Redisplay with current data
            }
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "Passwords do not match.");
                return await OnGetAsync();
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ModelState.AddModelError(nameof(CurrentPassword), "Current password is required.");
                return await OnGetAsync();
            }


            var client = _httpClientFactory.CreateClient();
            var apiUrl = _config["LoginServer:BaseUrl"];

            var updateRequest = new UpdateAccount
            {
                NewUsername = NewUsername,
                NewEmail = NewEmail,
                NewPassword = NewPassword,
                CurrentPassword = CurrentPassword
            };

            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (Request.Cookies.TryGetValue("access_token", out var token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"{apiUrl}/api/account/update", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Update failed.");
                return await OnGetAsync();
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(jsonResult);
            var newToken = result.RootElement.GetProperty("accessToken").GetString();

            if (!string.IsNullOrWhiteSpace(newToken))
            {
                Response.Cookies.Append("access_token", newToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                });
            }

            return RedirectToPage("/Account/Account"); // Reload to show updated data
        }
        private string? GetUsernameFromJwt()
        {
            if (!Request.Cookies.TryGetValue("access_token", out var token))
                return null;

            var principal = JwtHelper.GetPrincipalFromToken(token);
            return principal?.Identity?.Name;
        }
    }
}
