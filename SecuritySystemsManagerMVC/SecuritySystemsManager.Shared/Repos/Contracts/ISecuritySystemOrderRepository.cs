using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface ISecuritySystemOrderRepository : IBaseRepository<SecuritySystemOrderDto>
    {
        Task<SecuritySystemOrderDto> GetOrderWithTechniciansAsync(int orderId);
        Task AddTechnicianToOrderAsync(int orderId, int technicianId);
        Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId);
    }
} 