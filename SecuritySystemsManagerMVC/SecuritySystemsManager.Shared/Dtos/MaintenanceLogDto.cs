using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class MaintenanceLogDto : BaseDto
    {
        public MaintenanceLogDto()
        {
            MaintenanceDevices = new List<MaintenanceDeviceDto>();
        }

        public int SecuritySystemOrderId { get; set; }
        public SecuritySystemOrderDto SecuritySystemOrder { get; set; }

        public int TechnicianId { get; set; }
        public UserDto Technician { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool Resolved { get; set; }

        public List<MaintenanceDeviceDto> MaintenanceDevices { get; set; }
    }
} 