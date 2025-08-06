using SecuritySystemsManager.Shared.Dtos;
using System.Security.Claims;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IInvoiceRepository : IBaseRepository<InvoiceDto>
    {
        Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByClientIdAsync(int clientId, int pageSize, int pageNumber);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber);
        Task<int> GetInvoicesCountByClientIdAsync(int clientId);
        Task<int> GetInvoicesCountByTechnicianIdAsync(int technicianId);
        Task<InvoiceDto> GetInvoiceByOrderIdAsync(int orderId);
        
        // Universal filtering method with pagination
        Task<(List<InvoiceDto> Invoices, int TotalCount)> GetFilteredInvoicesAsync(
            string? searchTerm = null,
            string? paymentStatus = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1);
    }
} 