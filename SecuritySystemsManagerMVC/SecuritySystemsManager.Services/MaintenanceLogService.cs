using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class MaintenanceLogService : BaseCrudService<MaintenanceLogDto, IMaintenanceLogRepository>, IMaintenanceLogService
    {
        private readonly IMaintenanceLogRepository _maintenanceLogRepository;
        private readonly ISecuritySystemOrderService _orderService;

        public MaintenanceLogService(
            IMaintenanceLogRepository repository, 
            ISecuritySystemOrderService orderService) : base(repository)
        {
            _maintenanceLogRepository = repository;
            _orderService = orderService;
        }

        public async Task<IEnumerable<MaintenanceLogDto>> GetLogsByUserRoleAsync(int userId, string userRole, int pageSize, int pageNumber)
        {
            // Admin and Manager can see all logs
            if (userRole == "Admin" || userRole == "Manager")
            {
                return await _repository.GetWithPaginationAsync(pageSize, pageNumber);
            }
            // Client can see only logs for their orders
            else if (userRole == "Client")
            {
                return await _maintenanceLogRepository.GetLogsByClientIdAsync(userId, pageSize, pageNumber);
            }
            // Technician can see only logs for orders assigned to them
            else if (userRole == "Technician")
            {
                return await _maintenanceLogRepository.GetLogsByTechnicianIdAsync(userId, pageSize, pageNumber);
            }
            
            // Default: return empty list
            return new List<MaintenanceLogDto>();
        }
        
        public async Task<int> GetLogsCountByUserRoleAsync(int userId, string userRole)
        {
            // Admin and Manager can see all logs
            if (userRole == "Admin" || userRole == "Manager")
            {
                var allLogs = await _repository.GetAllAsync();
                return allLogs.Count();
            }
            // Client can see only logs for their orders
            else if (userRole == "Client")
            {
                return await _maintenanceLogRepository.GetLogsCountByClientIdAsync(userId);
            }
            // Technician can see only logs for orders assigned to them
            else if (userRole == "Technician")
            {
                return await _maintenanceLogRepository.GetLogsCountByTechnicianIdAsync(userId);
            }
            
            // Default: return 0
            return 0;
        }

        public async Task<MaintenanceLogDto> PrepareMaintenanceLogForOrderAsync(int orderId, int? technicianId = null)
        {
            var order = await _orderService.GetByIdIfExistsAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found");
            }

            var maintenanceLog = new MaintenanceLogDto
            {
                SecuritySystemOrderId = orderId,
                SecuritySystemOrder = order,
                Date = DateTime.Now
            };

            if (technicianId.HasValue)
            {
                maintenanceLog.TechnicianId = technicianId.Value;
            }

            return maintenanceLog;
        }
    }
} 