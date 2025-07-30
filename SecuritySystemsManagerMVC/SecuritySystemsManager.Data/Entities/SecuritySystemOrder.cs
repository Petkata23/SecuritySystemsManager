using SecuritySystemsManager.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class SecuritySystemOrder : BaseEntity
    {
        public SecuritySystemOrder()
        {
            InstalledDevices = new List<InstalledDevice>();
            Technicians = new List<User>();
            MaintenanceLogs = new List<MaintenanceLog>();
        }

        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters")]
        public string Title { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; }
        
        [Required]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Phone number must be between 8 and 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        public int ClientId { get; set; }
        public virtual User Client { get; set; }

        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime RequestedDate { get; set; }

        public virtual ICollection<InstalledDevice> InstalledDevices { get; set; }
        public virtual ICollection<User> Technicians { get; set; }
        public virtual ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
    }
}
