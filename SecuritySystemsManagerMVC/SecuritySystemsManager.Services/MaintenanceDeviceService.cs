using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class MaintenanceDeviceService : BaseCrudService<MaintenanceDeviceDto, IMaintenanceDeviceRepository>, IMaintenanceDeviceService
    {
        public MaintenanceDeviceService(IMaintenanceDeviceRepository repository) : base(repository)
        {
        }
    }
} 