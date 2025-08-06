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
        private readonly IInvoiceService _invoiceService;

        public SecuritySystemOrderService(
            ISecuritySystemOrderRepository repository, 
            IUserRepository userRepository,
            IInvoiceService invoiceService) : base(repository)
        {
            _userRepository = userRepository;
            _orderRepository = repository;
            _invoiceService = invoiceService;
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

            // Get orders based on user role
            var orders = await GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);
            var totalCount = await GetOrdersCountByUserRoleAsync(userId, userRole);

            // Apply filters
            var filteredOrders = orders.AsEnumerable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredOrders = filteredOrders.Where(o =>
                    o.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (startDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.RequestedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.RequestedDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var statusEnum))
            {
                filteredOrders = filteredOrders.Where(o => o.Status == statusEnum);
            }

            return (filteredOrders.ToList(), totalCount);
        }

        // New methods for business logic from controller
        public async Task<(bool Success, string ErrorMessage)> GenerateInvoiceFromOrderAsync(int orderId, decimal laborCost, Dictionary<string, decimal> deviceCosts)
        {
            try
            {
                // Calculate total amount
                decimal totalAmount = laborCost;
                
                // Add device costs
                foreach (var deviceCost in deviceCosts.Values)
                {
                    totalAmount += deviceCost;
                }

                // Generate the invoice with calculated amount
                await _invoiceService.GenerateInvoiceFromOrderAsync(orderId, totalAmount);
                
                return (true, $"Invoice generated successfully with total amount: ${totalAmount:F2}");
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Error generating invoice: {ex.Message}");
            }
        }

        public async Task<(DateTime? ParsedStartDate, DateTime? ParsedEndDate, string ErrorMessage)> ParseDateRangeAsync(string startDate, string endDate)
        {
            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;
            string errorMessage = null;
            
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
            {
                parsedStartDate = start;
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                errorMessage = "Invalid start date format";
            }
            
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
            {
                parsedEndDate = end;
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                errorMessage = "Invalid end date format";
            }

            return (parsedStartDate, parsedEndDate, errorMessage);
        }


    }
} 