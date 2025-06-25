using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class Location : BaseEntity
    {
        public Location()
        {
            Orders = new List<SecuritySystemOrder>();
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string? Description { get; set; }

        public int ClientId { get; set; }
        public virtual User Client { get; set; }

        public virtual ICollection<SecuritySystemOrder> Orders { get; set; }
    }
}
