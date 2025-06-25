using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class InstalledDeviceDto : BaseDto
    {
        public InstalledDeviceDto()
        {
            MaintenanceDevices = new List<MaintenanceDeviceDto>();
        }

        public int SecuritySystemOrderId { get; set; }
        public SecuritySystemOrderDto SecuritySystemOrder { get; set; }

        public DeviceType DeviceType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Quantity { get; set; }
        public DateTime DateInstalled { get; set; }

        public int InstalledById { get; set; }
        public UserDto InstalledBy { get; set; }

        public List<MaintenanceDeviceDto> MaintenanceDevices { get; set; }
    }
} 