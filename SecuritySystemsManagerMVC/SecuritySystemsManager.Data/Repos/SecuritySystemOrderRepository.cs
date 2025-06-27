using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    public class SecuritySystemOrderRepository : BaseRepository<SecuritySystemOrder, SecuritySystemOrderDto>, ISecuritySystemOrderRepository
    {
        public SecuritySystemOrderRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
} 