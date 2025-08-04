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

namespace SecuritySystemsManager.Services
{
    public class DropboxTokenManager
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private string _accessToken;
        private DateTime _accessTokenExpiry;
        private string _refreshToken;
        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly object _lockObject = new object();
        private bool _isRefreshing = false;

        public DropboxTokenManager(
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _httpClient = new HttpClient();
            
            _appKey = _configuration["Dropbox:AppKey"];
            _appSecret = _configuration["Dropbox:AppSecret"];
            
            // Load initial values from configuration
            _accessToken = _configuration["Dropbox:AccessToken"];
            _refreshToken = _configuration["Dropbox:RefreshToken"];
            
            // Set initial expiry time - assume token expires in 4 hours from now
            // In a real scenario, we would store this in a persistent store
            var expiryStr = _configuration["Dropbox:TokenExpiry"];
            if (!string.IsNullOrEmpty(expiryStr) && DateTime.TryParse(expiryStr, out DateTime storedExpiry))
            {
                _accessTokenExpiry = storedExpiry;
            }
            else
            {
                // Default expiry if not stored
                _accessTokenExpiry = DateTime.UtcNow;  // Set to now to force refresh on first use
            }

            Console.WriteLine($"[DropboxTokenManager] Initialized. Token expires at: {_accessTokenExpiry} UTC");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // First try to load from memory
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow.AddMinutes(5) < _accessTokenExpiry)
            {
                Console.WriteLine($"[DropboxTokenManager] Using cached token. Current time: {DateTime.UtcNow}, Expiry: {_accessTokenExpiry}");
                return _accessToken;
            }
            
            // Try to load from database
            using (var scope = _serviceProvider.CreateScope())
            {
                var tokenRepository = scope.ServiceProvider.GetRequiredService<IDropboxTokenRepository>();
                var (accessToken, refreshToken, expiryTime) = await tokenRepository.GetLatestTokenAsync();
                
                if (!string.IsNullOrEmpty(accessToken) && DateTime.UtcNow.AddMinutes(5) < expiryTime)
                {
                    // Update memory cache
                    _accessToken = accessToken;
                    _refreshToken = refreshToken;
                    _accessTokenExpiry = expiryTime;
                    
                    Console.WriteLine($"[DropboxTokenManager] Using token from database. Current time: {DateTime.UtcNow}, Expiry: {expiryTime}");
                    return _accessToken;
                }
            }
            
            // If we got here, we need to refresh the token
            Console.WriteLine($"[DropboxTokenManager] Token expired or about to expire. Current time: {DateTime.UtcNow}, Expiry: {_accessTokenExpiry}");
            await RefreshAccessTokenAsync();
            
            return _accessToken;
        }

        private async Task RefreshAccessTokenAsync()
        {
            // Prevent multiple simultaneous refresh attempts
            lock (_lockObject)
            {
                // Double-check if token is still expired after acquiring lock
                if (DateTime.UtcNow.AddMinutes(5) < _accessTokenExpiry || _isRefreshing)
                {
                    return;
                }
                
                _isRefreshing = true;
            }

            try
            {
                Console.WriteLine("[DropboxTokenManager] Refreshing access token...");

                // Make sure we have a refresh token
                if (string.IsNullOrEmpty(_refreshToken))
                {
                    // Try to get refresh token from database
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenRepository = scope.ServiceProvider.GetRequiredService<IDropboxTokenRepository>();
                        var (_, refreshToken, __) = await tokenRepository.GetLatestTokenAsync();
                        
                        if (string.IsNullOrEmpty(refreshToken))
                        {
                            throw new InvalidOperationException("Refresh token is missing. Please authenticate with Dropbox first.");
                        }
                        
                        _refreshToken = refreshToken;
                    }
                }

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("refresh_token", _refreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", _appKey),
                    new KeyValuePair<string, string>("client_secret", _appSecret)
                });

                var response = await _httpClient.PostAsync("https://api.dropbox.com/oauth2/token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                    lock (_lockObject)
                    {
                        _accessToken = tokenData.AccessToken;
                        // Default expiration is 4 hours, but use the value from the response if available
                        _accessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
                    }

                    // Save token to database
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenRepository = scope.ServiceProvider.GetRequiredService<IDropboxTokenRepository>();
                        await tokenRepository.SaveTokenAsync(_accessToken, _refreshToken, _accessTokenExpiry);
                    }

                    Console.WriteLine($"[DropboxTokenManager] Token refreshed successfully. Expires in {tokenData.ExpiresIn} seconds ({_accessTokenExpiry} UTC).");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DropboxTokenManager] Failed to refresh token. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new Exception($"Failed to refresh Dropbox token: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DropboxTokenManager] Error refreshing token: {ex.Message}");
                throw;
            }
            finally
            {
                lock (_lockObject)
                {
                    _isRefreshing = false;
                }
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
        }
    }
} 