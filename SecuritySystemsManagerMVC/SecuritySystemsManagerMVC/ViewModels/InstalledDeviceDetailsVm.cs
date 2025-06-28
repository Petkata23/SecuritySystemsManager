using SecuritySystemsManager.Shared.Enums;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class InstalledDeviceDetailsVm : BaseVm
    {
        [DisplayName("Device Type")]
        public DeviceType DeviceType { get; set; }

        [DisplayName("Brand")]
        public string Brand { get; set; }

        [DisplayName("Model")]
        public string Model { get; set; }

        [DisplayName("Quantity")]
        public int Quantity { get; set; }

        [DisplayName("Date Installed")]
        public DateTime DateInstalled { get; set; }

        [DisplayName("Device Image")]
        public string? DeviceImage { get; set; }

        [DisplayName("Installed By")]
        public string InstalledByFullName { get; set; }

        [DisplayName("Maintenance Records")]
        public List<MaintenanceDeviceDetailsVm> MaintenanceDevices { get; set; }

        public InstalledDeviceDetailsVm()
        {
            MaintenanceDevices = new List<MaintenanceDeviceDetailsVm>();
        }
    }
}
