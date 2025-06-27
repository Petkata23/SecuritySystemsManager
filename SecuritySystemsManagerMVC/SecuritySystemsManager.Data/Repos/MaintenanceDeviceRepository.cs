using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    public class MaintenanceDeviceRepository : BaseRepository<MaintenanceDevice, MaintenanceDeviceDto>, IMaintenanceDeviceRepository
    {
        public MaintenanceDeviceRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
} 