using SecuritySystemsManager.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class SecuritySystemOrder : BaseEntity
    {
        public SecuritySystemOrder()
        {
            InstalledDevices = new List<InstalledDevice>();
            Technicians = new List<OrderTechnician>();
            MaintenanceLogs = new List<MaintenanceLog>();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }

        public int ClientId { get; set; }
        public virtual User Client { get; set; }

        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime RequestedDate { get; set; }

        public virtual ICollection<InstalledDevice> InstalledDevices { get; set; }
        public virtual ICollection<OrderTechnician> Technicians { get; set; }
        public virtual ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
    }
}
