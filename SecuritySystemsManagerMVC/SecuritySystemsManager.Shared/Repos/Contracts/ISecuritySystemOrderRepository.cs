using SecuritySystemsManager.Shared.Dtos;
using System.Security.Claims;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface ISecuritySystemOrderRepository : IBaseRepository<SecuritySystemOrderDto>
    {
        Task<SecuritySystemOrderDto> GetOrderWithTechniciansAsync(int orderId);
        Task AddTechnicianToOrderAsync(int orderId, int technicianId);
        Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId);
        
        // Methods for filtering orders
        Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByClientIdAsync(int clientId, int pageSize, int pageNumber);
        Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber);
        Task<int> GetOrdersCountByClientIdAsync(int clientId);
        Task<int> GetOrdersCountByTechnicianIdAsync(int technicianId);
        Task<SecuritySystemOrderDto> GetOrderWithAllDetailsAsync(int orderId);
        
        // Universal filtering method with pagination
        Task<(List<SecuritySystemOrderDto> Orders, int TotalCount)> GetFilteredOrdersAsync(
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? status = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1);
    }
} 