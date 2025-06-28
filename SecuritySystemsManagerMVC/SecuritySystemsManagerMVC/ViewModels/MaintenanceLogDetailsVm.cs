using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class MaintenanceLogDetailsVm : BaseVm
    {
        [DisplayName("Security System Order")]
        public string OrderTitle { get; set; }

        [DisplayName("Technician")]
        public string TechnicianFullName { get; set; }

        [DisplayName("Date")]
        public DateTime Date { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Resolved")]
        public bool Resolved { get; set; }

        [DisplayName("Maintenance Devices")]
        public List<MaintenanceDeviceDetailsVm> MaintenanceDevices { get; set; }

        public MaintenanceLogDetailsVm()
        {
            MaintenanceDevices = new List<MaintenanceDeviceDetailsVm>();
        }
    }
}
