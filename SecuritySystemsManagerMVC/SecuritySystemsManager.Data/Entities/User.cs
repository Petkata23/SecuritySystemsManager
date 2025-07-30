using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; }
        
        [StringLength(500, ErrorMessage = "Profile image URL cannot be longer than 500 characters")]
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
