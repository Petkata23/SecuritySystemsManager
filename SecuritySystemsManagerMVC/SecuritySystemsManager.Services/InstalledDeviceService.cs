using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class InstalledDeviceService : BaseCrudService<InstalledDeviceDto, IInstalledDeviceRepository>, IInstalledDeviceService
    {
        private readonly IHostEnvironment _hostEnvironment;

        public InstalledDeviceService(IInstalledDeviceRepository repository, IHostEnvironment hostEnvironment) : base(repository)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<string> UploadDeviceImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Create uploads directory if it doesn't exist
            string uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "uploads", "devices");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return relative path for storage in database
            return Path.Combine("/uploads", "devices", uniqueFileName).Replace("\\", "/");
        }

        public async Task<InstalledDeviceDto> AddDeviceToOrderAsync(InstalledDeviceDto deviceDto, IFormFile? imageFile)
        {
            // Upload image if provided
            if (imageFile != null)
            {
                deviceDto.DeviceImage = await UploadDeviceImageAsync(imageFile);
            }

            // Add device to database
            await SaveAsync(deviceDto);
            return deviceDto;
        }
        
        public async Task<InstalledDeviceDto> UpdateDeviceWithImageAsync(InstalledDeviceDto deviceDto, IFormFile imageFile)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(deviceDto.DeviceImage))
            {
                string oldImagePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", deviceDto.DeviceImage.TrimStart('/'));
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }
            
            // Upload new image
            deviceDto.DeviceImage = await UploadDeviceImageAsync(imageFile);
            
            // Update device in database
            await SaveAsync(deviceDto);
            return deviceDto;
        }
        
        public async Task<InstalledDeviceDto> UpdateDeviceAsync(InstalledDeviceDto deviceDto, IFormFile? imageFile, bool removeExistingImage)
        {
            // Handle image update based on conditions
            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload new image and replace existing one
                return await UpdateDeviceWithImageAsync(deviceDto, imageFile);
            }
            else if (removeExistingImage)
            {
                // Remove existing image if requested
                if (!string.IsNullOrEmpty(deviceDto.DeviceImage))
                {
                    string oldImagePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", deviceDto.DeviceImage.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }
                
                // Clear image path in database
                deviceDto.DeviceImage = null;
            }
            
            // Update device in database (with or without image changes)
            await SaveAsync(deviceDto);
            return deviceDto;
        }
    }
} 