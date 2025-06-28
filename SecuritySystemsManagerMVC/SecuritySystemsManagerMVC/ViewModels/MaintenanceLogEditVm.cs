using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class MaintenanceLogEditVm : BaseVm
    {
        [Required(ErrorMessage = "Order is required")]
        [DisplayName("Security System Order")]
        public int SecuritySystemOrderId { get; set; }
        public IEnumerable<SelectListItem> AllOrders { get; set; }

        [Required(ErrorMessage = "Technician is required")]
        [DisplayName("Technician")]
        public int TechnicianId { get; set; }
        public IEnumerable<SelectListItem> AllTechnicians { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description must be up to 500 characters")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Resolved")]
        public bool Resolved { get; set; }

        public MaintenanceLogEditVm()
        {
            AllOrders = new List<SelectListItem>();
            AllTechnicians = new List<SelectListItem>();
        }
    }
}
