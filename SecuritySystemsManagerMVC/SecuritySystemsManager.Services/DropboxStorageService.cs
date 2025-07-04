using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class DropboxStorageService : IFileStorageService
    {
        private readonly string _accessToken;
        private readonly string _rootFolder;
        // Кеш за URL адресите, за да не правим заявки всеки път
        private static readonly Dictionary<string, string> _urlCache = new Dictionary<string, string>();

        public DropboxStorageService(IConfiguration configuration)
        {
            _accessToken = configuration["Dropbox:AccessToken"];
            _rootFolder = configuration["Dropbox:RootFolder"] ?? "SecuritySystemsManager";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // Generate unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string dropboxPath = $"/{_rootFolder}/{folder}/{uniqueFileName}";
            
            Console.WriteLine($"Uploading file to Dropbox path: {dropboxPath}");
            
            using (var client = new DropboxClient(_accessToken))
            using (var memoryStream = new MemoryStream())
            {
                // Copy file to memory stream
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Upload to Dropbox
                var uploadResult = await client.Files.UploadAsync(
                    new UploadArg(dropboxPath),
                    memoryStream);
                
                Console.WriteLine($"File uploaded successfully. ID: {uploadResult.Id}");

                // Генерираме постоянен URL
                return await GetPermanentLinkAsync(dropboxPath);
            }
        }

        private async Task<string> GetPermanentLinkAsync(string dropboxPath)
        {
            // Проверяваме дали имаме кеширан URL за този път
            if (_urlCache.TryGetValue(dropboxPath, out string cachedUrl))
            {
                Console.WriteLine($"Using cached URL for {dropboxPath}: {cachedUrl}");
                return cachedUrl;
            }

            try
            {
                using (var client = new DropboxClient(_accessToken))
                {
                    // Опитваме да създадем постоянна споделена връзка
                    var sharedLinkArg = new CreateSharedLinkWithSettingsArg(dropboxPath);
                    var sharedLink = await client.Sharing.CreateSharedLinkWithSettingsAsync(sharedLinkArg);
                    
                    // Конвертираме връзката към директен download линк
                    string directLink = sharedLink.Url;
                    
                    // Replace www.dropbox.com with dl.dropboxusercontent.com for direct download
                    if (directLink.Contains("www.dropbox.com"))
                    {
                        directLink = directLink.Replace("www.dropbox.com", "dl.dropboxusercontent.com");
                    }
                    
                    // Add raw=1 parameter to force direct download/display
                    if (directLink.EndsWith("?dl=0"))
                    {
                        directLink = directLink.Replace("?dl=0", "?raw=1");
                    }
                    else if (!directLink.Contains("?raw=1"))
                    {
                        directLink = directLink + "?raw=1";
                    }
                    
                    Console.WriteLine($"Created permanent shared link: {directLink}");
                    
                    // Кеширане на URL адреса
                    string encodedUrl = Uri.EscapeDataString(directLink);
                    string proxyUrl = $"/imageproxy?url={encodedUrl}";
                    
                    // Запазваме в кеша
                    _urlCache[dropboxPath] = proxyUrl;
                    
                    return proxyUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating shared link: {ex.Message}");
                
                try
                {
                    // Като резервен вариант, опитваме временен линк
                    using (var client = new DropboxClient(_accessToken))
                    {
                        var tempLink = await client.Files.GetTemporaryLinkAsync(dropboxPath);
                        string directUrl = tempLink.Link;
                        
                        Console.WriteLine($"Created temporary link: {directUrl}");
                        
                        // Кеширане на URL адреса
                        string encodedUrl = Uri.EscapeDataString(directUrl);
                        string proxyUrl = $"/imageproxy?url={encodedUrl}";
                        
                        return proxyUrl;
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Error getting temporary link: {innerEx.Message}");
                    return "/img/favicon.svg"; // Return default image
                }
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // If it's a proxy URL, extract the original URL
            if (filePath.StartsWith("/imageproxy?url="))
            {
                // Skip this file as we can't easily extract the original path
                Console.WriteLine($"Skipping delete for proxy URL: {filePath}");
                return true;
            }

            try
            {
                // Extract the path from the URL if it's a shared link
                string dropboxPath = filePath;
                if (filePath.Contains("dropboxusercontent.com"))
                {
                    // Extract the file name from the URL
                    string fileName = Path.GetFileName(filePath);
                    dropboxPath = $"/{_rootFolder}/{fileName}";
                }

                Console.WriteLine($"Attempting to delete file at: {dropboxPath}");
                
                using (var client = new DropboxClient(_accessToken))
                {
                    var deleteArg = new DeleteArg(dropboxPath);
                    await client.Files.DeleteV2Async(deleteArg);
                    Console.WriteLine($"File deleted successfully: {dropboxPath}");
                    
                    // Премахваме от кеша
                    if (_urlCache.ContainsKey(dropboxPath))
                    {
                        _urlCache.Remove(dropboxPath);
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }
    }
} 