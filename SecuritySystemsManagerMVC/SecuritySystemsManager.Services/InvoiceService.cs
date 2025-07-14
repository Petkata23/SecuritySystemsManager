using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class InvoiceService : BaseCrudService<InvoiceDto, IInvoiceRepository>, IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository repository) : base(repository)
        {
            _invoiceRepository = repository;
        }

        public async Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id)
        {
            return await _invoiceRepository.GetInvoiceWithDetailsAsync(id);
        }

        public async Task<InvoiceDto> MarkAsPaidAsync(int id)
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                throw new ArgumentException($"Invoice with ID {id} not found");
            }

            invoice.IsPaid = true;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync(invoice);

            return invoice;
        }

        public async Task<InvoiceDto> MarkAsUnpaidAsync(int id)
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                throw new ArgumentException($"Invoice with ID {id} not found");
            }

            invoice.IsPaid = false;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync(invoice);

            return invoice;
        }
    }
} 