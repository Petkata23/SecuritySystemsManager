using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class SecuritySystemOrderDto : BaseDto
    {
        public SecuritySystemOrderDto()
        {
            InstalledDevices = new List<InstalledDeviceDto>();
            Technicians = new List<OrderTechnicianDto>();
            MaintenanceLogs = new List<MaintenanceLogDto>();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }

        public int ClientId { get; set; }
        public UserDto Client { get; set; }

        public int LocationId { get; set; }
        public LocationDto Location { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime RequestedDate { get; set; }

        public List<InstalledDeviceDto> InstalledDevices { get; set; }
        public List<OrderTechnicianDto> Technicians { get; set; }
        public List<MaintenanceLogDto> MaintenanceLogs { get; set; }
        
        public InvoiceDto Invoice { get; set; }
    }
} 