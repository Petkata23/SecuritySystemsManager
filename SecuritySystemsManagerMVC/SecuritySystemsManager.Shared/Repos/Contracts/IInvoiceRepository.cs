using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IInvoiceRepository : IBaseRepository<InvoiceDto>
    {
        Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id);
    }
} 