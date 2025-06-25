using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class OrderTechnician : BaseEntity
    {
        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public int TechnicianId { get; set; }
        public virtual User Technician { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
