using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class MaintenanceLogRepository : BaseRepository<MaintenanceLog, MaintenanceLogDto>, IMaintenanceLogRepository
    {
        public MaintenanceLogRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
} 