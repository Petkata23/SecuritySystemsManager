using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Threading.Tasks;

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
    }
} 