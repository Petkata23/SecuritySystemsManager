using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        public DropboxAuthController(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            SecuritySystemsManager.Services.DropboxTokenManager tokenManager)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _tokenManager = tokenManager;
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
                    
                    ViewBag.AccessToken = tokenData.AccessToken;
                    ViewBag.RefreshToken = tokenData.RefreshToken;
                    ViewBag.ExpiresIn = tokenData.ExpiresIn;
                    
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