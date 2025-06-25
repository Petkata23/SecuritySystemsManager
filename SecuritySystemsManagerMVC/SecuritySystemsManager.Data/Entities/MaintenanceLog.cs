using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class MaintenanceLog : BaseEntity
    {
        public int InstalledDeviceId { get; set; }
        public virtual InstalledDevice InstalledDevice { get; set; }

        public int TechnicianId { get; set; }
        public virtual User Technician { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool Resolved { get; set; }
    }
}
