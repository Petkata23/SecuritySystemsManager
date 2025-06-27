using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class OrderTechnicianService : BaseCrudService<OrderTechnicianDto, IOrderTechnicianRepository>, IOrderTechnicianService
    {
        public OrderTechnicianService(IOrderTechnicianRepository repository) : base(repository)
        {
        }
    }
} 