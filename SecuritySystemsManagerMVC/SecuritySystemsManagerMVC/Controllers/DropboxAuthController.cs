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
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing");
            }

            try
            {
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
                    
                    return View();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Failed to get token: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestTokenRefresh()
        {
            try
            {
                // This will trigger a token refresh if needed
                var accessToken = await _tokenManager.GetAccessTokenAsync();
                
                return Json(new { 
                    success = true, 
                    message = "Token refresh successful",
                    tokenFirstChars = accessToken.Substring(0, 10) + "..." // Only show first few characters for security
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Token refresh failed: {ex.Message}" 
                });
            }
        }
        
        private void UpdateTokenInConfig(string accessToken, string refreshToken, DateTime expiryTime)
        {
            try
            {
                if (!System.IO.File.Exists(_configFilePath))
                {
                    Console.WriteLine($"[DropboxAuthController] Config file not found: {_configFilePath}");
                    return;
                }

                // Read the JSON file
                string json = System.IO.File.ReadAllText(_configFilePath);
                
                // Parse the JSON
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    // Create a new JSON object with the updated tokens
                    var options = new JsonWriterOptions
                    {
                        Indented = true
                    };

                    using (var ms = new MemoryStream())
                    {
                        using (var writer = new Utf8JsonWriter(ms, options))
                        {
                            writer.WriteStartObject();
                            
                            // Copy all properties from the original JSON
                            foreach (var property in document.RootElement.EnumerateObject())
                            {
                                if (property.Name == "Dropbox")
                                {
                                    writer.WritePropertyName("Dropbox");
                                    writer.WriteStartObject();
                                    
                                    // Copy all properties from the Dropbox object
                                    foreach (var dropboxProperty in property.Value.EnumerateObject())
                                    {
                                        if (dropboxProperty.Name == "AccessToken")
                                        {
                                            // Update the access token
                                            writer.WritePropertyName("AccessToken");
                                            writer.WriteStringValue(accessToken);
                                        }
                                        else if (dropboxProperty.Name == "RefreshToken")
                                        {
                                            // Update the refresh token
                                            writer.WritePropertyName("RefreshToken");
                                            writer.WriteStringValue(refreshToken);
                                        }
                                        else if (dropboxProperty.Name == "TokenExpiry")
                                        {
                                            // Update the token expiry
                                            writer.WritePropertyName("TokenExpiry");
                                            writer.WriteStringValue(expiryTime.ToString("o")); // ISO 8601 format
                                        }
                                        else
                                        {
                                            // Copy the property as is
                                            writer.WritePropertyName(dropboxProperty.Name);
                                            dropboxProperty.Value.WriteTo(writer);
                                        }
                                    }
                                    
                                    // Add TokenExpiry if it doesn't exist
                                    if (!property.Value.TryGetProperty("TokenExpiry", out _))
                                    {
                                        writer.WritePropertyName("TokenExpiry");
                                        writer.WriteStringValue(expiryTime.ToString("o")); // ISO 8601 format
                                    }
                                    
                                    writer.WriteEndObject();
                                }
                                else
                                {
                                    // Copy the property as is
                                    writer.WritePropertyName(property.Name);
                                    property.Value.WriteTo(writer);
                                }
                            }
                            
                            writer.WriteEndObject();
                        }

                        // Write the updated JSON back to the file
                        var updatedJson = Encoding.UTF8.GetString(ms.ToArray());
                        System.IO.File.WriteAllText(_configFilePath, updatedJson);
                        
                        Console.WriteLine("[DropboxAuthController] Updated tokens in config file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DropboxAuthController] Error updating tokens in config: {ex.Message}");
                // Don't throw here, as we don't want to fail the authentication just because we couldn't update the config
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