using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        public async Task<InvoiceDto> GenerateInvoiceFromOrderAsync(int orderId, decimal totalAmount = 0)
        {
            // Check if invoice already exists for this order
            var existingInvoice = await _invoiceRepository.GetInvoiceByOrderIdAsync(orderId);
            if (existingInvoice != null)
            {
                throw new InvalidOperationException($"Invoice already exists for order {orderId}");
            }

            // Create new invoice
            var invoice = new InvoiceDto
            {
                SecuritySystemOrderId = orderId,
                IssuedOn = DateTime.UtcNow,
                TotalAmount = totalAmount,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the invoice
            await _repository.SaveAsync(invoice);
            
            // Get the saved invoice with the generated ID
            return await _invoiceRepository.GetInvoiceByOrderIdAsync(orderId);
        }

        // Universal filtering method with pagination
        public async Task<(List<InvoiceDto> Invoices, int TotalCount)> GetFilteredInvoicesAsync(
            string? searchTerm = null,
            string? paymentStatus = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1)
        {
            // Get user role and ID
            string userRole = "Client"; // Default
            int userId = 0;

            if (user != null)
            {
                if (user.IsInRole("Admin") || user.IsInRole("Manager"))
                {
                    userRole = user.IsInRole("Admin") ? "Admin" : "Manager";
                }
                else if (user.IsInRole("Technician"))
                {
                    userRole = "Technician";
                }
                else
                {
                    userRole = "Client";
                }

                var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Get invoices based on user role
            var invoices = await GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);
            var totalCount = await GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Apply filters
            var filteredInvoices = invoices.AsEnumerable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredInvoices = filteredInvoices.Where(i =>
                    i.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (i.SecuritySystemOrder != null && i.SecuritySystemOrder.Title != null && i.SecuritySystemOrder.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    i.TotalAmount.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                if (paymentStatus.ToLower() == "paid")
                {
                    filteredInvoices = filteredInvoices.Where(i => i.IsPaid == true);
                }
                else if (paymentStatus.ToLower() == "unpaid")
                {
                    filteredInvoices = filteredInvoices.Where(i => i.IsPaid == false);
                }
            }

            // Get filtered count
            var filteredCount = filteredInvoices.Count();

            // Apply pagination and ordering
            var paginatedInvoices = filteredInvoices
                .OrderByDescending(i => i.IssuedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedInvoices, filteredCount);
        }
    }
} 