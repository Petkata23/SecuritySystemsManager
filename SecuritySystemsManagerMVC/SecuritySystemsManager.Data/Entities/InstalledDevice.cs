using SecuritySystemsManager.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class InstalledDevice : BaseEntity
    {
        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public DeviceType DeviceType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Quantity { get; set; }
        public DateTime DateInstalled { get; set; }

        public int InstalledById { get; set; }
        public virtual User InstalledBy { get; set; }
    }
}
