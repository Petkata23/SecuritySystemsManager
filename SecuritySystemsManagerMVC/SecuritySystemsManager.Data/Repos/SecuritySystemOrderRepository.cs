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
        
        // Get orders by client ID with pagination
        public async Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByClientIdAsync(int clientId, int pageSize, int pageNumber)
        {
            Console.WriteLine($"GetOrdersByClientIdAsync - Client ID: {clientId}, Page Size: {pageSize}, Page Number: {pageNumber}");
            
            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Location)
                .Include(o => o.Technicians)
                .Include(o => o.InstalledDevices)
                .Include(o => o.MaintenanceLogs)
                .Where(o => o.ClientId == clientId);
                
            Console.WriteLine($"SQL Query: {query.ToQueryString()}");
            
            var count = await query.CountAsync();
            Console.WriteLine($"Total count before pagination: {count}");
            
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            Console.WriteLine($"GetOrdersByClientIdAsync - Found {orders.Count} orders");
                
            return _mapper.Map<IEnumerable<SecuritySystemOrderDto>>(orders);
        }
        
        // Get orders assigned to a technician with pagination
        public async Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber)
        {
            Console.WriteLine($"GetOrdersByTechnicianIdAsync - Technician ID: {technicianId}, Page Size: {pageSize}, Page Number: {pageNumber}");
            
            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Location)
                .Include(o => o.Technicians)
                .Include(o => o.InstalledDevices)
                .Include(o => o.MaintenanceLogs)
                .Where(o => o.Technicians.Any(t => t.Id == technicianId));
                
            Console.WriteLine($"SQL Query: {query.ToQueryString()}");
            
            var count = await query.CountAsync();
            Console.WriteLine($"Total count before pagination: {count}");
            
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            Console.WriteLine($"GetOrdersByTechnicianIdAsync - Found {orders.Count} orders");
                
            return _mapper.Map<IEnumerable<SecuritySystemOrderDto>>(orders);
        }
        
        // Get total count of orders for a client
        public async Task<int> GetOrdersCountByClientIdAsync(int clientId)
        {
            Console.WriteLine($"GetOrdersCountByClientIdAsync - Client ID: {clientId}");
            
            var query = _context.Orders.Where(o => o.ClientId == clientId);
            Console.WriteLine($"SQL Query: {query.ToQueryString()}");
            
            var count = await query.CountAsync();
            Console.WriteLine($"GetOrdersCountByClientIdAsync - Found {count} orders");
            
            return count;
        }
        
        // Get total count of orders assigned to a technician
        public async Task<int> GetOrdersCountByTechnicianIdAsync(int technicianId)
        {
            Console.WriteLine($"GetOrdersCountByTechnicianIdAsync - Technician ID: {technicianId}");
            
            var query = _context.Orders.Where(o => o.Technicians.Any(t => t.Id == technicianId));
            Console.WriteLine($"SQL Query: {query.ToQueryString()}");
            
            var count = await query.CountAsync();
            Console.WriteLine($"GetOrdersCountByTechnicianIdAsync - Found {count} orders");
            
            return count;
        }
    }
} 