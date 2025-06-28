using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class MaintenanceDeviceEditVm : BaseVm
    {
        [Required(ErrorMessage = "Maintenance log is required")]
        [DisplayName("Maintenance Log")]
        public int MaintenanceLogId { get; set; }
        public IEnumerable<SelectListItem> AllLogs { get; set; }

        [Required(ErrorMessage = "Installed device is required")]
        [DisplayName("Installed Device")]
        public int InstalledDeviceId { get; set; }
        public IEnumerable<SelectListItem> AllInstalledDevices { get; set; }

        [DisplayName("Notes")]
        [StringLength(1000, ErrorMessage = "Notes must be up to 1000 characters")]
        public string? Notes { get; set; }

        [DisplayName("Fixed")]
        public bool IsFixed { get; set; }

        public MaintenanceDeviceEditVm()
        {
            AllLogs = new List<SelectListItem>();
            AllInstalledDevices = new List<SelectListItem>();
        }
    }
}
