using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IOrderTechnicianService : IBaseCrudService<OrderTechnicianDto, IOrderTechnicianRepository>
    {
    }
} 