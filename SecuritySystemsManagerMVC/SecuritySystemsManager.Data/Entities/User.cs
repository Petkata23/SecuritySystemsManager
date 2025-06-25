using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class User : BaseEntity
    {
        public User()
        {
            OrdersAsClient = new List<SecuritySystemOrder>();
            AssignedOrders = new List<OrderTechnician>();
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        // For Clients
        public virtual ICollection<SecuritySystemOrder> OrdersAsClient { get; set; }

        // For Technicians
        public virtual ICollection<OrderTechnician> AssignedOrders { get; set; }
    }
}
