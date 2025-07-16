using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IInvoiceService : IBaseCrudService<InvoiceDto, IInvoiceRepository>
    {
        Task<InvoiceDto> MarkAsPaidAsync(int id);
        Task<InvoiceDto> MarkAsUnpaidAsync(int id);
        Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber);
        Task<int> GetInvoicesCountByUserRoleAsync(int userId, string userRole);
    }
} 