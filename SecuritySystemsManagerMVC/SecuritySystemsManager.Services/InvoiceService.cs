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

        // Get invoices based on user role
        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber)
        {
            Console.WriteLine($"GetInvoicesByUserRoleAsync - User ID: {userId}, User Role: {userRole}, Page Size: {pageSize}, Page Number: {pageNumber}");

            // Admin and Manager can see all invoices
            if (userRole == "Admin" || userRole == "Manager")
            {
                Console.WriteLine("User is Admin or Manager - Getting all invoices");
                var invoices = await _repository.GetWithPaginationAsync(pageSize, pageNumber);
                Console.WriteLine($"Found {invoices.Count()} invoices");
                return invoices;
            }
            // Client can see only their invoices
            else if (userRole == "Client")
            {
                Console.WriteLine("User is Client - Getting client invoices");
                var invoices = await _invoiceRepository.GetInvoicesByClientIdAsync(userId, pageSize, pageNumber);
                Console.WriteLine($"Found {invoices.Count()} invoices");
                return invoices;
            }
            // Technician can see only invoices for orders assigned to them
            else if (userRole == "Technician")
            {
                Console.WriteLine("User is Technician - Getting assigned invoices");
                var invoices = await _invoiceRepository.GetInvoicesByTechnicianIdAsync(userId, pageSize, pageNumber);
                Console.WriteLine($"Found {invoices.Count()} invoices");
                return invoices;
            }

            // Default: return empty list
            Console.WriteLine("Unknown role - Returning empty list");
            return new List<InvoiceDto>();
        }

        // Get invoices count based on user role
        public async Task<int> GetInvoicesCountByUserRoleAsync(int userId, string userRole)
        {
            Console.WriteLine($"GetInvoicesCountByUserRoleAsync - User ID: {userId}, User Role: {userRole}");

            // Admin and Manager can see all invoices
            if (userRole == "Admin" || userRole == "Manager")
            {
                Console.WriteLine("User is Admin or Manager - Getting all invoices count");
                var allInvoices = await _repository.GetAllAsync();
                Console.WriteLine($"Found {allInvoices.Count()} invoices");
                return allInvoices.Count();
            }
            // Client can see only their invoices
            else if (userRole == "Client")
            {
                Console.WriteLine("User is Client - Getting client invoices count");
                var count = await _invoiceRepository.GetInvoicesCountByClientIdAsync(userId);
                Console.WriteLine($"Found {count} invoices");
                return count;
            }
            // Technician can see only invoices for orders assigned to them
            else if (userRole == "Technician")
            {
                Console.WriteLine("User is Technician - Getting assigned invoices count");
                var count = await _invoiceRepository.GetInvoicesCountByTechnicianIdAsync(userId);
                Console.WriteLine($"Found {count} invoices");
                return count;
            }

            // Default: return 0
            Console.WriteLine("Unknown role - Returning 0");
            return 0;
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