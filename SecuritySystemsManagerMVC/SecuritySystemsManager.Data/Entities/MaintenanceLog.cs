using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class MaintenanceLog : BaseEntity
    {
        public MaintenanceLog()
        {
            MaintenanceDevices = new List<MaintenanceDevice>();
        }

        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public int TechnicianId { get; set; }
        public virtual User Technician { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool Resolved { get; set; }

        public virtual ICollection<MaintenanceDevice> MaintenanceDevices { get; set; }
    }
}
