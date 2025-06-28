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
        }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? ProfileImage { get; set; }

        public int RoleId { get; set; }
        public RoleDto Role { get; set; }
        public RoleType RoleType => Role?.RoleType ?? 0;

        // For Clients
        public List<SecuritySystemOrderDto> OrdersAsClient { get; set; }

        // For Technicians
        public List<OrderTechnicianDto> AssignedOrders { get; set; }

    }
} 