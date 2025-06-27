using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class MaintenanceLogService : BaseCrudService<MaintenanceLogDto, IMaintenanceLogRepository>, IMaintenanceLogService
    {
        public MaintenanceLogService(IMaintenanceLogRepository repository) : base(repository)
        {
        }
    }
} 