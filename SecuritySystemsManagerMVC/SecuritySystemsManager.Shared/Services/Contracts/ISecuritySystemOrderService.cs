using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Security.Claims;

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
        
        // Universal filtering method with pagination
        Task<(List<SecuritySystemOrderDto> Orders, int TotalCount)> GetFilteredOrdersAsync(
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? status = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1);
            
        // New methods for business logic from controller
        Task<(bool Success, string ErrorMessage)> GenerateInvoiceFromOrderAsync(int orderId, decimal laborCost, Dictionary<string, decimal> deviceCosts);
        Task<(DateTime? ParsedStartDate, DateTime? ParsedEndDate, string ErrorMessage)> ParseDateRangeAsync(string startDate, string endDate);
    }
} 