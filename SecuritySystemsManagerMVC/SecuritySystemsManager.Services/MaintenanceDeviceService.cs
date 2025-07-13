using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class MaintenanceDeviceService : BaseCrudService<MaintenanceDeviceDto, IMaintenanceDeviceRepository>, IMaintenanceDeviceService
    {
        private readonly IInstalledDeviceService _installedDeviceService;
        private readonly IMaintenanceLogRepository _maintenanceLogRepository;

        public MaintenanceDeviceService(
            IMaintenanceDeviceRepository repository,
            IInstalledDeviceService installedDeviceService,
            IMaintenanceLogRepository maintenanceLogRepository) : base(repository)
        {
            _installedDeviceService = installedDeviceService;
            _maintenanceLogRepository = maintenanceLogRepository;
        }

        public async Task<MaintenanceDeviceDto> PrepareMaintenanceDeviceAsync(int deviceId)
        {
            // Първо проверяваме дали устройството съществува
            bool deviceExists = await _installedDeviceService.ExistsByIdAsync(deviceId);
            if (!deviceExists)
            {
                throw new ArgumentException($"Device with ID {deviceId} not found");
            }

            var device = await _installedDeviceService.GetByIdIfExistsAsync(deviceId);
            
            var maintenanceDevice = new MaintenanceDeviceDto
            {
                InstalledDeviceId = deviceId,
                InstalledDevice = device,
                IsFixed = false
            };

            return maintenanceDevice;
        }

        public async Task<MaintenanceDeviceDto> AddDeviceToMaintenanceLogAsync(MaintenanceDeviceDto maintenanceDeviceDto, int logId)
        {
            // Проверка дали логът съществува
            var log = await _maintenanceLogRepository.GetByIdAsync(logId);
            if (log == null)
            {
                throw new ArgumentException($"Maintenance log with ID {logId} not found");
            }

            // Задаване на ID на лога
            maintenanceDeviceDto.MaintenanceLogId = logId;
            
            // Запазване на устройството за поддръжка
            await SaveAsync(maintenanceDeviceDto);
            
            return maintenanceDeviceDto;
        }

        public async Task<MaintenanceDeviceDto> AddInstalledDeviceToMaintenanceAsync(MaintenanceDeviceDto maintenanceDeviceDto, int deviceId)
        {
            // Проверка дали устройството съществува
            bool deviceExists = await _installedDeviceService.ExistsByIdAsync(deviceId);
            if (!deviceExists)
            {
                throw new ArgumentException($"Device with ID {deviceId} not found");
            }

            // Задаване на ID на устройството
            maintenanceDeviceDto.InstalledDeviceId = deviceId;
            
            // Запазване на устройството за поддръжка
            await SaveAsync(maintenanceDeviceDto);
            
            return maintenanceDeviceDto;
        }

        public async Task ToggleDeviceFixedStatusAsync(int deviceId)
        {
            var device = await _repository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Maintenance device with ID {deviceId} not found");
            }

            // Промяна на статуса
            device.IsFixed = !device.IsFixed;
            
            // Запазване на промените
            await _repository.SaveAsync(device);
        }

        public async Task MarkAllDevicesFixedForLogAsync(int logId)
        {
            // Проверка дали логът съществува
            var log = await _maintenanceLogRepository.GetByIdAsync(logId);
            if (log == null)
            {
                throw new ArgumentException($"Maintenance log with ID {logId} not found");
            }

            // Вземане на всички устройства за този лог
            var allDevices = (await _repository.GetAllAsync()).Where(d => d.MaintenanceLogId == logId).ToList();
            
            // Маркиране на всички като поправени
            foreach (var device in allDevices)
            {
                device.IsFixed = true;
                await _repository.SaveAsync(device);
            }
        }

        public async Task<IEnumerable<MaintenanceDeviceDto>> GetDevicesByMaintenanceLogIdAsync(int logId)
        {
            var allDevices = await _repository.GetAllAsync();
            return allDevices.Where(d => d.MaintenanceLogId == logId);
        }
    }
} 