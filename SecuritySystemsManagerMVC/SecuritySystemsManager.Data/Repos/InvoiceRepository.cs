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
    }
} 