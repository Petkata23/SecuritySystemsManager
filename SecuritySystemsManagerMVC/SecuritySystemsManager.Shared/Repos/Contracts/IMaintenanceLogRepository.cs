using SecuritySystemsManager.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IMaintenanceLogRepository : IBaseRepository<MaintenanceLogDto>
    {
        Task<IEnumerable<MaintenanceLogDto>> GetLogsByClientIdAsync(int clientId, int pageSize, int pageNumber);
        Task<IEnumerable<MaintenanceLogDto>> GetLogsByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber);
        Task<int> GetLogsCountByClientIdAsync(int clientId);
        Task<int> GetLogsCountByTechnicianIdAsync(int technicianId);
    }
} 