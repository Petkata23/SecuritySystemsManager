using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface ISecuritySystemOrderRepository : IBaseRepository<SecuritySystemOrderDto>
    {
        Task<SecuritySystemOrderDto> GetOrderWithTechniciansAsync(int orderId);
        Task AddTechnicianToOrderAsync(int orderId, int technicianId);
        Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId);
        
        // New methods for filtering orders
        Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByClientIdAsync(int clientId, int pageSize, int pageNumber);
        Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber);
        Task<int> GetOrdersCountByClientIdAsync(int clientId);
        Task<int> GetOrdersCountByTechnicianIdAsync(int technicianId);
    }
} 