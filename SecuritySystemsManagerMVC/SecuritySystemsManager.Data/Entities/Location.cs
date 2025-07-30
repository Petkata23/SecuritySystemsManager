using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class Location : BaseEntity
    {
        public Location()
        {
            Orders = new List<SecuritySystemOrder>();
        }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Location name must be between 2 and 100 characters")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters")]
        public string Address { get; set; }
        
        [Required]
        [StringLength(20, ErrorMessage = "Latitude cannot be longer than 20 characters")]
        public string Latitude { get; set; }
        
        [Required]
        [StringLength(20, ErrorMessage = "Longitude cannot be longer than 20 characters")]
        public string Longitude { get; set; }
        
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string? Description { get; set; }

        public virtual ICollection<SecuritySystemOrder> Orders { get; set; }
    }
}
