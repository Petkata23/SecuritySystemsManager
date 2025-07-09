using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IMaintenanceLogService : IBaseCrudService<MaintenanceLogDto, IMaintenanceLogRepository>
    {
        Task<IEnumerable<MaintenanceLogDto>> GetLogsByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber);
        Task<int> GetLogsCountByUserRoleAsync(int userId, string userRole);
    }
} 