using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class MaintenanceDevice : BaseEntity
    {
        public int MaintenanceLogId { get; set; }
        public virtual MaintenanceLog MaintenanceLog { get; set; }

        public int InstalledDeviceId { get; set; }
        public virtual InstalledDevice InstalledDevice { get; set; }

        public string? Notes { get; set; }
        public bool IsFixed { get; set; }
    }
} 