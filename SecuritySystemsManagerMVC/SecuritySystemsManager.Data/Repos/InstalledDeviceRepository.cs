using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    public class InstalledDeviceRepository : BaseRepository<InstalledDevice, InstalledDeviceDto>, IInstalledDeviceRepository
    {
        public InstalledDeviceRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
} 