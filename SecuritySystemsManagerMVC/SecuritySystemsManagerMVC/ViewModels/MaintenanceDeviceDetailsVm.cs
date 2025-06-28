using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class MaintenanceDeviceDetailsVm : BaseVm
    {
        [DisplayName("Maintenance Log Date")]
        public string MaintenanceLogDate { get; set; }

        [DisplayName("Installed Device")]
        public string InstalledDeviceName { get; set; }

        [DisplayName("Notes")]
        public string? Notes { get; set; }

        [DisplayName("Fixed")]
        public bool IsFixed { get; set; }
    }
}
