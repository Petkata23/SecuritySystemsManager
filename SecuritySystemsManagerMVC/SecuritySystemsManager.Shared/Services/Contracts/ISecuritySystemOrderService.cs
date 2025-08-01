using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface ISecuritySystemOrderService : IBaseCrudService<SecuritySystemOrderDto, ISecuritySystemOrderRepository>
    {
        Task AddTechnicianToOrderAsync(int orderId, int technicianId);
        Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId);
        
        // Methods for filtering orders based on user role
        Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber);
        Task<int> GetOrdersCountByUserRoleAsync(int userId, string userRole);
        Task<SecuritySystemOrderDto> GetOrderWithAllDetailsAsync(int orderId);
    }
} 