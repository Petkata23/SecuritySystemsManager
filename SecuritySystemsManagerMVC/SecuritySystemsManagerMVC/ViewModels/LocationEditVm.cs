using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class LocationEditVm : BaseVm
    {
        [Required(ErrorMessage = "Location name is required")]
        [StringLength(100)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [DisplayName("Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        [DisplayName("Latitude")]
        public string Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [DisplayName("Longitude")]
        public string Longitude { get; set; }

        [DisplayName("Description")]
        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
