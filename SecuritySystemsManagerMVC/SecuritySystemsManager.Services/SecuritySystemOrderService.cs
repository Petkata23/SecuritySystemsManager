using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

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
    }
} 