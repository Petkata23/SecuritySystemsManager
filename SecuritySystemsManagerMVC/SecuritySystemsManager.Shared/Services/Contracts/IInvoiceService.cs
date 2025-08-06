using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Security.Claims;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IInvoiceService : IBaseCrudService<InvoiceDto, IInvoiceRepository>
    {
        Task<InvoiceDto> MarkAsPaidAsync(int id);
        Task<InvoiceDto> MarkAsUnpaidAsync(int id);
        Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber);
        Task<int> GetInvoicesCountByUserRoleAsync(int userId, string userRole);
        Task<InvoiceDto> GenerateInvoiceFromOrderAsync(int orderId, decimal totalAmount = 0);
        
        // Universal filtering method with pagination
        Task<(List<InvoiceDto> Invoices, int TotalCount)> GetFilteredInvoicesAsync(
            string? searchTerm = null,
            string? paymentStatus = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1);
    }
} 