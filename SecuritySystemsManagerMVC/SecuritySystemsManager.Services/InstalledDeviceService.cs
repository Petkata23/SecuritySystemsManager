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
        private readonly IFileStorageService _fileStorageService;

        public InstalledDeviceService(IInstalledDeviceRepository repository, IFileStorageService fileStorageService) : base(repository)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<string> UploadDeviceImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Use the file storage service to upload the file
            return await _fileStorageService.UploadFileAsync(imageFile, "uploads/devices");
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
                await _fileStorageService.DeleteFileAsync(deviceDto.DeviceImage);
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
                    await _fileStorageService.DeleteFileAsync(deviceDto.DeviceImage);
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