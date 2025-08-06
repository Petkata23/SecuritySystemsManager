using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class InvoiceRepository : BaseRepository<Invoice, InvoiceDto>, IInvoiceRepository
    {
        private readonly SecuritySystemsManagerDbContext _dbContext;

        public InvoiceRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) 
        {
            _dbContext = context;
        }

        public async Task<InvoiceDto> GetInvoiceWithDetailsAsync(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Client)
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Location)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return null;

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByClientIdAsync(int clientId, int pageSize, int pageNumber)
        {
            var query = _dbContext.Invoices
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Client)
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Location)
                .Where(i => i.SecuritySystemOrder.ClientId == clientId);

            var invoices = await query
                .OrderByDescending(i => i.IssuedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber)
        {
            var query = _dbContext.Invoices
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Client)
                .Include(i => i.SecuritySystemOrder)
                    .ThenInclude(o => o.Location)
                .Where(i => i.SecuritySystemOrder.Technicians.Any(t => t.Id == technicianId));

            var invoices = await query
                .OrderByDescending(i => i.IssuedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<int> GetInvoicesCountByClientIdAsync(int clientId)
        {
            return await _dbContext.Invoices
                .Where(i => i.SecuritySystemOrder.ClientId == clientId)
                .CountAsync();
        }

        public async Task<int> GetInvoicesCountByTechnicianIdAsync(int technicianId)
        {
            return await _dbContext.Invoices
                .Where(i => i.SecuritySystemOrder.Technicians.Any(t => t.Id == technicianId))
                .CountAsync();
        }

        public async Task<InvoiceDto> GetInvoiceByOrderIdAsync(int orderId)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.SecuritySystemOrder)
                .FirstOrDefaultAsync(i => i.SecuritySystemOrderId == orderId);

            return invoice != null ? _mapper.Map<InvoiceDto>(invoice) : null;
        }

        // Universal filtering method with pagination
        public async Task<(List<InvoiceDto> Invoices, int TotalCount)> GetFilteredInvoicesAsync(
            string? searchTerm = null,
            string? paymentStatus = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1)
        {
            // Get user ID and role
            string userIdStr = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = user?.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userRole))
            {
                return (new List<InvoiceDto>(), 0);
            }
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return (new List<InvoiceDto>(), 0);
            }

            // Get base invoices based on user role using existing methods
            IEnumerable<InvoiceDto> baseInvoices;
            int totalCount;
            
            if (userRole == "Client")
            {
                baseInvoices = await GetInvoicesByClientIdAsync(userId, int.MaxValue, 1); // Get all invoices for client
                totalCount = await GetInvoicesCountByClientIdAsync(userId);
            }
            else if (userRole == "Technician")
            {
                baseInvoices = await GetInvoicesByTechnicianIdAsync(userId, int.MaxValue, 1); // Get all invoices for technician
                totalCount = await GetInvoicesCountByTechnicianIdAsync(userId);
            }
            else
            {
                // Manager/Admin - get all invoices
                baseInvoices = await GetAllAsync();
                totalCount = await _dbContext.Invoices.CountAsync();
            }

            // Convert to list for filtering
            var invoicesList = baseInvoices.ToList();

            // Apply additional filters
            var filteredInvoices = invoicesList.AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredInvoices = filteredInvoices.Where(i => 
                    i.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (i.SecuritySystemOrder != null && i.SecuritySystemOrder.Title != null && i.SecuritySystemOrder.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    i.TotalAmount.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply payment status filter
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