using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IMaintenanceDeviceService : IBaseCrudService<MaintenanceDeviceDto, IMaintenanceDeviceRepository>
    {
        Task<MaintenanceDeviceDto> PrepareMaintenanceDeviceAsync(int deviceId);
        Task<MaintenanceDeviceDto> AddDeviceToMaintenanceLogAsync(MaintenanceDeviceDto maintenanceDeviceDto, int logId);
        Task<MaintenanceDeviceDto> AddInstalledDeviceToMaintenanceAsync(MaintenanceDeviceDto maintenanceDeviceDto, int deviceId);
        Task ToggleDeviceFixedStatusAsync(int deviceId);
        Task MarkAllDevicesFixedForLogAsync(int logId);
        Task<IEnumerable<MaintenanceDeviceDto>> GetDevicesByMaintenanceLogIdAsync(int logId);
    }
} 