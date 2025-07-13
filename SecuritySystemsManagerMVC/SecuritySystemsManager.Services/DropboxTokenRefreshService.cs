using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SecuritySystemsManager.Services
{
    public class DropboxTokenRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DropboxTokenRefreshService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

        public DropboxTokenRefreshService(
            IServiceProvider serviceProvider,
            ILogger<DropboxTokenRefreshService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dropbox Token Refresh Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshTokenIfNeededAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while refreshing Dropbox token.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Dropbox Token Refresh Service is stopping.");
        }

        private async Task RefreshTokenIfNeededAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var tokenRepository = scope.ServiceProvider.GetRequiredService<IDropboxTokenRepository>();
                    var (accessToken, refreshToken, expiryTime) = await tokenRepository.GetLatestTokenAsync();

                    // Check if token exists and is about to expire (within 1 hour)
                    if (!string.IsNullOrEmpty(refreshToken) && DateTime.UtcNow.AddHours(1) >= expiryTime)
                    {
                        _logger.LogInformation("Dropbox access token is about to expire. Refreshing token...");

                        var appKey = _configuration["Dropbox:AppKey"];
                        var appSecret = _configuration["Dropbox:AppSecret"];

                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("refresh_token", refreshToken),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("client_id", appKey),
                            new KeyValuePair<string, string>("client_secret", appSecret)
                        });

                        var response = await _httpClient.PostAsync("https://api.dropbox.com/oauth2/token", content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                            // Calculate new expiry time
                            var newExpiryTime = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
                            
                            // Save the new token to the database
                            await tokenRepository.SaveTokenAsync(tokenData.AccessToken, refreshToken, newExpiryTime);
                            
                            _logger.LogInformation("Dropbox token refreshed successfully. New expiry: {ExpiryTime}", newExpiryTime);
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            _logger.LogError("Failed to refresh Dropbox token. Status: {Status}, Error: {Error}", 
                                response.StatusCode, errorContent);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Dropbox token is still valid or not found. No refresh needed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RefreshTokenIfNeededAsync");
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