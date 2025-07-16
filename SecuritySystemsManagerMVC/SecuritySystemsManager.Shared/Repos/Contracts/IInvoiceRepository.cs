using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IInvoiceRepository : IBaseRepository<InvoiceDto>
    {
        Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByClientIdAsync(int clientId, int pageSize, int pageNumber);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber);
        Task<int> GetInvoicesCountByClientIdAsync(int clientId);
        Task<int> GetInvoicesCountByTechnicianIdAsync(int technicianId);
    }
} 