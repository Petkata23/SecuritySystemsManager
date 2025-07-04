using Microsoft.AspNetCore.Http;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IInstalledDeviceService : IBaseCrudService<InstalledDeviceDto, IInstalledDeviceRepository>
    {
        Task<string> UploadDeviceImageAsync(IFormFile imageFile);
        Task<InstalledDeviceDto> AddDeviceToOrderAsync(InstalledDeviceDto deviceDto, IFormFile? imageFile);
        Task<InstalledDeviceDto> UpdateDeviceWithImageAsync(InstalledDeviceDto deviceDto, IFormFile imageFile);
        Task<InstalledDeviceDto> UpdateDeviceAsync(InstalledDeviceDto deviceDto, IFormFile? imageFile, bool removeExistingImage);
    }
} 