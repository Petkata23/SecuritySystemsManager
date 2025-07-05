using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SecuritySystemsManager.Data.Entities
{
    public class User : IdentityUser<int>, IBaseEntity
    {
        public User()
        {
            OrdersAsClient = new List<SecuritySystemOrder>();
            AssignedOrders = new List<SecuritySystemOrder>();
        }

        // Запазваме само полетата, които не са част от IdentityUser
        // IdentityUser вече има Username, Email, PasswordHash и други
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfileImage { get; set; }
        
        // Имплементация на IBaseEntity
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? RoleId { get; set; }
        public virtual Role? Role { get; set; }

        // For Clients
        public virtual ICollection<SecuritySystemOrder> OrdersAsClient { get; set; }

        // For Technicians
        public virtual ICollection<SecuritySystemOrder> AssignedOrders { get; set; }
    }
}
