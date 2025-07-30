using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class MaintenanceDevice : BaseEntity
    {
        public int MaintenanceLogId { get; set; }
        public virtual MaintenanceLog MaintenanceLog { get; set; }

        public int InstalledDeviceId { get; set; }
        public virtual InstalledDevice InstalledDevice { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot be longer than 1000 characters")]
        public string? Notes { get; set; }
        public bool IsFixed { get; set; }
    }
} 