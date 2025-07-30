using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class MaintenanceLog : BaseEntity
    {
        public MaintenanceLog()
        {
            MaintenanceDevices = new List<MaintenanceDevice>();
        }

        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public int TechnicianId { get; set; }
        public virtual User Technician { get; set; }

        public DateTime Date { get; set; }
        
        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; }
        
        public bool Resolved { get; set; }

        public virtual ICollection<MaintenanceDevice> MaintenanceDevices { get; set; }
    }
}
