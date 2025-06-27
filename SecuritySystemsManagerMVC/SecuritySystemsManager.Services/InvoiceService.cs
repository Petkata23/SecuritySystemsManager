using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class InvoiceService : BaseCrudService<InvoiceDto, IInvoiceRepository>, IInvoiceService
    {
        public InvoiceService(IInvoiceRepository repository) : base(repository)
        {
        }
    }
} 