using SecuritySystemsManager.Shared.Enums;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class SecuritySystemOrderDetailsVm : BaseVm
    {
        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("Status")]
        public OrderStatus Status { get; set; }

        [DisplayName("Requested Date")]
        public DateTime RequestedDate { get; set; }

        [DisplayName("Client")]
        public UserDetailsVm Client { get; set; }

        [DisplayName("Location")]
        public LocationDetailsVm Location { get; set; }

        [DisplayName("Invoice")]
        public InvoiceDetailsVm Invoice { get; set; }

        [DisplayName("Installed Devices")]
        public List<InstalledDeviceDetailsVm> InstalledDevices { get; set; }

        [DisplayName("Technicians")]
        public List<UserDetailsVm> Technicians { get; set; }

        [DisplayName("Maintenance Logs")]
        public List<MaintenanceLogDetailsVm> MaintenanceLogs { get; set; }

        public SecuritySystemOrderDetailsVm()
        {
            InstalledDevices = new List<InstalledDeviceDetailsVm>();
            Technicians = new List<UserDetailsVm>();
            MaintenanceLogs = new List<MaintenanceLogDetailsVm>();
        }
    }
}
