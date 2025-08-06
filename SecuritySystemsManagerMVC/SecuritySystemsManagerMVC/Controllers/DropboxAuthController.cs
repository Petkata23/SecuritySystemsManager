using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DropboxAuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SecuritySystemsManager.Services.DropboxTokenManager _tokenManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _configFilePath;

        public DropboxAuthController(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            SecuritySystemsManager.Services.DropboxTokenManager tokenManager,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _tokenManager = tokenManager;
            _serviceProvider = serviceProvider;
            
            // Get the config file path
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings{(env == "Production" ? "" : "." + env)}.json");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var appKey = _configuration["Dropbox:AppKey"];
            var redirectUri = Url.Action("Callback", "DropboxAuth", null, Request.Scheme, Request.Host.Value);
            
            var authUrl = $"https://www.dropbox.com/oauth2/authorize?client_id={appKey}&response_type=code&token_access_type=offline&redirect_uri={Uri.EscapeDataString(redirectUri)}";
            
            ViewBag.AuthUrl = authUrl;
            ViewBag.RedirectUri = redirectUri;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest("Authorization code is missing");
                }

                var appKey = _configuration["Dropbox:AppKey"];
                var appSecret = _configuration["Dropbox:AppSecret"];
                var redirectUri = Url.Action("Callback", "DropboxAuth", null, Request.Scheme, Request.Host.Value);

                var client = _httpClientFactory.CreateClient();
                
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", appKey),
                    new KeyValuePair<string, string>("client_secret", appSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                });

                var response = await client.PostAsync("https://api.dropbox.com/oauth2/token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
                    
                    // Calculate token expiry
                    var expiryTime = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
                    
                    // Save token to database
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenRepository = scope.ServiceProvider.GetRequiredService<IDropboxTokenRepository>();
                        await tokenRepository.SaveTokenAsync(tokenData.AccessToken, tokenData.RefreshToken, expiryTime);
                    }
                    
                    // Update the configuration file for backward compatibility
                    UpdateTokenInConfig(tokenData.AccessToken, tokenData.RefreshToken, expiryTime);
                    
                    ViewBag.AccessToken = tokenData.AccessToken;
                    ViewBag.RefreshToken = tokenData.RefreshToken;
                    ViewBag.ExpiresIn = tokenData.ExpiresIn;
                    ViewBag.ExpiryTime = expiryTime;
                    
                    return View("Callback");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Failed to exchange code for token: {response.StatusCode} - {errorResponse}";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error during OAuth callback: {ex.Message}";
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestTokenRefresh()
        {
            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync();
                var result = !string.IsNullOrEmpty(accessToken);
                if (result)
                {
                    ViewBag.Message = "Token refreshed successfully";
                }
                else
                {
                    ViewBag.Message = "Token refresh not needed or failed";
                }
                
                return View("TestResult");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error testing token refresh: {ex.Message}";
                return View("Error");
            }
        }

        private void UpdateTokenInConfig(string accessToken, string refreshToken, DateTime expiryTime)
        {
            try
            {
                if (!System.IO.File.Exists(_configFilePath))
                {
                    ViewBag.Error = "Configuration file not found";
                    return;
                }

                var jsonContent = System.IO.File.ReadAllText(_configFilePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                // Create a new JSON object with updated Dropbox settings
                var updatedJson = new
                {
                    Dropbox = new
                    {
                        AppKey = _configuration["Dropbox:AppKey"],
                        AppSecret = _configuration["Dropbox:AppSecret"],
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiryTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }
                };

                // Merge with existing configuration
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJsonString = JsonSerializer.Serialize(updatedJson, options);

                // Write back to file
                System.IO.File.WriteAllText(_configFilePath, updatedJsonString);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error updating configuration file: {ex.Message}";
            }
        }

        private class TokenResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
        }
    }
} 