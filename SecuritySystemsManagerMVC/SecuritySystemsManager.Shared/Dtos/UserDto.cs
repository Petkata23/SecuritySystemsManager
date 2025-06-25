using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class UserDto : BaseDto
    {
        public UserDto()
        {
            OrdersAsClient = new List<SecuritySystemOrderDto>();
            AssignedOrders = new List<OrderTechnicianDto>();
            Locations = new List<LocationDto>();
            InstalledDevices = new List<InstalledDeviceDto>();
            MaintenanceLogs = new List<MaintenanceLogDto>();
            Notifications = new List<NotificationDto>();
        }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public int RoleId { get; set; }
        public RoleDto Role { get; set; }
        public RoleType RoleType => Role?.RoleType ?? 0;

        // For Clients
        public List<SecuritySystemOrderDto> OrdersAsClient { get; set; }
        public List<LocationDto> Locations { get; set; }

        // For Technicians
        public List<OrderTechnicianDto> AssignedOrders { get; set; }
        public List<InstalledDeviceDto> InstalledDevices { get; set; }
        public List<MaintenanceLogDto> MaintenanceLogs { get; set; }

        // Notifications
        public List<NotificationDto> Notifications { get; set; }
    }
} 