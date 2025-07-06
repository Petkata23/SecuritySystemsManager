using Microsoft.Extensions.Configuration;
using System;
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
        private string _accessToken;
        private DateTime _accessTokenExpiry;
        private readonly string _refreshToken;
        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly object _lockObject = new object();
        private readonly string _configFilePath;

        public DropboxTokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            
            _appKey = _configuration["Dropbox:AppKey"];
            _appSecret = _configuration["Dropbox:AppSecret"];
            _refreshToken = _configuration["Dropbox:RefreshToken"];
            _accessToken = _configuration["Dropbox:AccessToken"];
            
            // Try to find the appsettings.json file path
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings{(env == "Production" ? "" : "." + env)}.json");
            
            // Assume token expires in 4 hours (Dropbox default)
            _accessTokenExpiry = DateTime.UtcNow.AddHours(4);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Check if token is expired or about to expire (within 5 minutes)
            if (DateTime.UtcNow.AddMinutes(5) >= _accessTokenExpiry)
            {
                await RefreshAccessTokenAsync();
            }

            return _accessToken;
        }

        private async Task RefreshAccessTokenAsync()
        {
            lock (_lockObject)
            {
                // Double-check if token is still expired after acquiring lock
                if (DateTime.UtcNow.AddMinutes(5) < _accessTokenExpiry)
                {
                    return;
                }
            }

            try
            {
                Console.WriteLine("[DropboxTokenManager] Refreshing access token...");

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

                    // Update the access token in the configuration file
                    UpdateAccessTokenInConfig(tokenData.AccessToken);

                    Console.WriteLine($"[DropboxTokenManager] Token refreshed successfully. Expires in {tokenData.ExpiresIn} seconds.");
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
        }

        private void UpdateAccessTokenInConfig(string newAccessToken)
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Console.WriteLine($"[DropboxTokenManager] Config file not found: {_configFilePath}");
                    return;
                }

                // Read the JSON file
                string json = File.ReadAllText(_configFilePath);
                
                // Parse the JSON
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    // Create a new JSON object with the updated access token
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
                                            writer.WriteStringValue(newAccessToken);
                                        }
                                        else
                                        {
                                            // Copy the property as is
                                            writer.WritePropertyName(dropboxProperty.Name);
                                            dropboxProperty.Value.WriteTo(writer);
                                        }
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
                        File.WriteAllText(_configFilePath, updatedJson);
                        
                        Console.WriteLine("[DropboxTokenManager] Updated access token in config file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DropboxTokenManager] Error updating access token in config: {ex.Message}");
                // Don't throw here, as we don't want to fail the token refresh just because we couldn't update the config
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