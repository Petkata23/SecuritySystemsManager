using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Security.Claims;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class SecuritySystemOrderService : BaseCrudService<SecuritySystemOrderDto, ISecuritySystemOrderRepository>, ISecuritySystemOrderService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecuritySystemOrderRepository _orderRepository;

        public SecuritySystemOrderService(
            ISecuritySystemOrderRepository repository, 
            IUserRepository userRepository) : base(repository)
        {
            _userRepository = userRepository;
            _orderRepository = repository;
        }

        public async Task AddTechnicianToOrderAsync(int orderId, int technicianId)
        {
            // Verify that the order exists
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found.");

            // Verify that the technician exists
            var technician = await _userRepository.GetByIdAsync(technicianId);
            if (technician == null)
                throw new ArgumentException("Technician not found.");

            // Use the repository method to add the technician to the order
            await _orderRepository.AddTechnicianToOrderAsync(orderId, technicianId);
        }

        public async Task RemoveTechnicianFromOrderAsync(int orderId, int technicianId)
        {
            // Verify that the order exists
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found.");

            // Verify that the technician exists
            var technician = await _userRepository.GetByIdAsync(technicianId);
            if (technician == null)
                throw new ArgumentException("Technician not found.");

            // Use the repository method to remove the technician from the order
            await _orderRepository.RemoveTechnicianFromOrderAsync(orderId, technicianId);
        }
        
        // Get orders based on user role
        public async Task<IEnumerable<SecuritySystemOrderDto>> GetOrdersByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber)
        {
            Console.WriteLine($"GetOrdersByUserRoleAsync - User ID: {userId}, User Role: {userRole}, Page Size: {pageSize}, Page Number: {pageNumber}");
            
            // Admin and Manager can see all orders
            if (userRole == "Admin" || userRole == "Manager")
            {
                Console.WriteLine("User is Admin or Manager - Getting all orders");
                var orders = await _orderRepository.GetWithPaginationAsync(pageSize, pageNumber);
                Console.WriteLine($"Found {orders.Count()} orders");
                return orders;
            }
            // Client can see only their orders
            else if (userRole == "Client")
            {
                Console.WriteLine("User is Client - Getting client orders");
                var orders = await _orderRepository.GetOrdersByClientIdAsync(userId, pageSize, pageNumber);
                Console.WriteLine($"Found {orders.Count()} orders");
                return orders;
            }
            // Technician can see only orders assigned to them
            else if (userRole == "Technician")
            {
                Console.WriteLine("User is Technician - Getting assigned orders");
                var orders = await _orderRepository.GetOrdersByTechnicianIdAsync(userId, pageSize, pageNumber);
                Console.WriteLine($"Found {orders.Count()} orders");
                return orders;
            }
            
            // Default: return empty list
            Console.WriteLine("Unknown role - Returning empty list");
            return new List<SecuritySystemOrderDto>();
        }
        
        // Get orders count based on user role
        public async Task<int> GetOrdersCountByUserRoleAsync(int userId, string userRole)
        {
            Console.WriteLine($"GetOrdersCountByUserRoleAsync - User ID: {userId}, User Role: {userRole}");
            
            // Admin and Manager can see all orders
            if (userRole == "Admin" || userRole == "Manager")
            {
                Console.WriteLine("User is Admin or Manager - Getting all orders count");
                var allOrders = await _orderRepository.GetAllAsync();
                Console.WriteLine($"Found {allOrders.Count()} orders");
                return allOrders.Count();
            }
            // Client can see only their orders
            else if (userRole == "Client")
            {
                Console.WriteLine("User is Client - Getting client orders count");
                var count = await _orderRepository.GetOrdersCountByClientIdAsync(userId);
                Console.WriteLine($"Found {count} orders");
                return count;
            }
            // Technician can see only orders assigned to them
            else if (userRole == "Technician")
            {
                Console.WriteLine("User is Technician - Getting assigned orders count");
                var count = await _orderRepository.GetOrdersCountByTechnicianIdAsync(userId);
                Console.WriteLine($"Found {count} orders");
                return count;
            }
            
            // Default: return 0
            Console.WriteLine("Unknown role - Returning 0");
            return 0;
        }

        public async Task<SecuritySystemOrderDto> GetOrderWithAllDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithAllDetailsAsync(orderId);
        }

        // Universal filtering method with pagination
        public async Task<(List<SecuritySystemOrderDto> Orders, int TotalCount)> GetFilteredOrdersAsync(
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? status = null,
            ClaimsPrincipal? user = null,
            int pageSize = 10,
            int pageNumber = 1)
        {
            return await _orderRepository.GetFilteredOrdersAsync(searchTerm, startDate, endDate, status, user, pageSize, pageNumber);
        }
    }
} 