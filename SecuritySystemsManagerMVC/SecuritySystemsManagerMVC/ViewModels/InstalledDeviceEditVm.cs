using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class InstalledDeviceEditVm : BaseVm
    {
        [Required(ErrorMessage = "Order is required")]
        [DisplayName("Security System Order")]
        public int SecuritySystemOrderId { get; set; }
        public IEnumerable<SelectListItem> AllOrders { get; set; }

        [Required(ErrorMessage = "Device type is required")]
        [DisplayName("Device Type")]
        public DeviceType DeviceType { get; set; }
        public IEnumerable<SelectListItem> DeviceTypeOptions { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [StringLength(100)]
        [DisplayName("Brand")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100)]
        [DisplayName("Model")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000)]
        [DisplayName("Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Installation date is required")]
        [DisplayName("Date Installed")]
        [DataType(DataType.Date)]
        public DateTime DateInstalled { get; set; }

        [DisplayName("Device Image (URL or path)")]
        public string? DeviceImage { get; set; }

        [DisplayName("Device Image File")]
        public IFormFile? DeviceImageFile { get; set; }

        [Required(ErrorMessage = "Technician is required")]
        [DisplayName("Installed By")]
        public int InstalledById { get; set; }
        public IEnumerable<SelectListItem> AllTechnicians { get; set; }

        public InstalledDeviceEditVm()
        {
            AllOrders = new List<SelectListItem>();
            AllTechnicians = new List<SelectListItem>();
            DeviceTypeOptions = new List<SelectListItem>();
        }
    }
}
