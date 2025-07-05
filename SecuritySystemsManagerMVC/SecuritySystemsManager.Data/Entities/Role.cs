using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Data.Entities
{
    public class Role : IdentityRole<int>, IBaseEntity
    {
        public Role()
        {
            Users = new List<User>();
        }

        // IdentityRole вече има Name и Id
        public RoleType RoleType { get; set; }
        
        // Имплементация на IBaseEntity
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
