using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class OrderTechnicianRepository : BaseRepository<OrderTechnician, OrderTechnicianDto>, IOrderTechnicianRepository
    {
        public OrderTechnicianRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
} 