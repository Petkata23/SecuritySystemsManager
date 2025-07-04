using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class ImageProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly Dictionary<string, (byte[] Data, string ContentType, DateTime Expiry)> _imageCache =
            new Dictionary<string, (byte[] Data, string ContentType, DateTime Expiry)>();
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24); // Кеширане за 24 часа

        public ImageProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("imageproxy")]
        public async Task<IActionResult> GetImage([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("[ImageProxy] Missing 'url' parameter.");
                return BadRequest("Missing image URL");
            }

            try
            {
                var decodedUrl = Uri.UnescapeDataString(url);

                // Проверяваме дали изображението е в кеша
                if (_imageCache.TryGetValue(decodedUrl, out var cachedImage))
                {
                    // Проверяваме дали кешът не е изтекъл
                    if (cachedImage.Expiry > DateTime.UtcNow)
                    {
                        Console.WriteLine($"[ImageProxy] Serving from cache: {decodedUrl}");
                        return File(cachedImage.Data, cachedImage.ContentType);
                    }
                    else
                    {
                        // Премахваме изтеклия кеш
                        _imageCache.Remove(decodedUrl);
                    }
                }

                Console.WriteLine($"[ImageProxy] Decoded URL: {decodedUrl}");

                // Create a client with appropriate headers for Dropbox
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                // Add additional headers that might be needed for Dropbox
                client.DefaultRequestHeaders.Add("Accept", "image/*, */*");

                Console.WriteLine($"[ImageProxy] Sending request to: {decodedUrl}");

                // Use ResponseHeadersRead to start processing the response as soon as headers are available
                var response = await client.GetAsync(decodedUrl, HttpCompletionOption.ResponseHeadersRead);

                Console.WriteLine($"[ImageProxy] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ImageProxy] Failed to fetch image. Status: {response.StatusCode}");
                    Console.WriteLine($"[ImageProxy] Response content: {await response.Content.ReadAsStringAsync()}");
                    return StatusCode((int)response.StatusCode, "Failed to load image.");
                }

                // Log all response headers for debugging
                Console.WriteLine("[ImageProxy] Response headers:");
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"[ImageProxy] {header.Key}: {string.Join(", ", header.Value)}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                // Четем данните в паметта
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                // Запазваме в кеша
                _imageCache[decodedUrl] = (imageData, contentType, DateTime.UtcNow.Add(_cacheDuration));

                Console.WriteLine($"[ImageProxy] Loaded and cached successfully. Content-Type: {contentType}, Content-Length: {imageData.Length}");

                // Връщаме изображението с кеширане в браузъра
                Response.Headers.Add("Cache-Control", $"public, max-age={_cacheDuration.TotalSeconds}");
                return File(imageData, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageProxy] ERROR: {ex.Message}");
                Console.WriteLine($"[ImageProxy] Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while proxying the image.");
            }
        }
    }
}