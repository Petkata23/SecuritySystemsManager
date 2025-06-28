using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class LocationDetailsVm : BaseVm
    {
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Address")]
        public string Address { get; set; }

        [DisplayName("Latitude")]
        public string Latitude { get; set; }

        [DisplayName("Longitude")]
        public string Longitude { get; set; }

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Orders at this Location")]
        public List<SecuritySystemOrderDetailsVm> Orders { get; set; }

        public LocationDetailsVm()
        {
            Orders = new List<SecuritySystemOrderDetailsVm>();
        }
    }
}
