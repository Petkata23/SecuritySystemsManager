using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class SecuritySystemOrderRepository : BaseRepository<SecuritySystemOrder, SecuritySystemOrderDto>, ISecuritySystemOrderRepository
    {
        private readonly SecuritySystemsManagerDbContext _context;
        private readonly IMapper _mapper;

        public SecuritySystemOrderRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SecuritySystemOrderDto> GetOrderWithTechniciansAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return _mapper.Map<SecuritySystemOrderDto>(order);
        }

        public async Task AddTechnicianToOrderAsync(int orderId, int technicianId)
        {
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException("Order not found.");

            var technician = await _context.Users.FindAsync(technicianId);
            if (technician == null)
                throw new ArgumentException("Technician not found.");

            // Check if the technician is already assigned to this order
            if (order.Technicians.Any(t => t.Id == technicianId))
                return; // Technician is already assigned, nothing to do

            // Add the technician to the order
            order.Technicians.Add(technician);

            // Save changes
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId)
        {
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException("Order not found.");

            var technician = order.Technicians.FirstOrDefault(t => t.Id == technicianId);
            if (technician == null)
                return; // Technician is not assigned to this order, nothing to do

            // Remove the technician from the order
            order.Technicians.Remove(technician);

            // Save changes
            await _context.SaveChangesAsync();
        }
    }
} 