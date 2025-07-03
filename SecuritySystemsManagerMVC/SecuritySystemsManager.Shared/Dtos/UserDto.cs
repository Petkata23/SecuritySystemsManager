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
            AssignedOrders = new List<SecuritySystemOrderDto>();
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public int RoleId { get; set; }
        public RoleDto Role { get; set; }

        public string? ProfileImage { get; set; }


        // For Clients
        public List<SecuritySystemOrderDto> OrdersAsClient { get; set; }

        // For Technicians
        public List<SecuritySystemOrderDto> AssignedOrders { get; set; }

    }
} 