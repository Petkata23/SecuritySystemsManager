using SecuritySystemsManager.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class InstalledDevice : BaseEntity
    {
        public InstalledDevice()
        {
            MaintenanceDevices = new List<MaintenanceDevice>();
        }

        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public DeviceType DeviceType { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Brand must be between 2 and 50 characters")]
        public string Brand { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Model must be between 2 and 50 characters")]
        public string Model { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
        
        public DateTime DateInstalled { get; set; }
        
        [StringLength(500, ErrorMessage = "Device image URL cannot be longer than 500 characters")]
        public string? DeviceImage { get; set; }

        public int InstalledById { get; set; }
        public virtual User InstalledBy { get; set; }

        public virtual ICollection<MaintenanceDevice> MaintenanceDevices { get; set; }
    }
}
