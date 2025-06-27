using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class InstalledDeviceService : BaseCrudService<InstalledDeviceDto, IInstalledDeviceRepository>, IInstalledDeviceService
    {
        public InstalledDeviceService(IInstalledDeviceRepository repository) : base(repository)
        {
        }
    }
} 