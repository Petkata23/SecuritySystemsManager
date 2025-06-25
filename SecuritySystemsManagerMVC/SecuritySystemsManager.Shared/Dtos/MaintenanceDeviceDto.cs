using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class MaintenanceDeviceDto : BaseDto
    {
        public int MaintenanceLogId { get; set; }
        public MaintenanceLogDto MaintenanceLog { get; set; }

        public int InstalledDeviceId { get; set; }
        public InstalledDeviceDto InstalledDevice { get; set; }

        public string? Notes { get; set; }
        public bool IsFixed { get; set; }
    }
} 