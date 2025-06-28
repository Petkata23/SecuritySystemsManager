using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class OrderTechnicianEditVm : BaseVm
    {
        [Required(ErrorMessage = "Order is required")]
        [DisplayName("Security System Order")]
        public int SecuritySystemOrderId { get; set; }
        public IEnumerable<SelectListItem> AllOrders { get; set; }

        [Required(ErrorMessage = "Technician is required")]
        [DisplayName("Technician")]
        public int TechnicianId { get; set; }
        public IEnumerable<SelectListItem> AllTechnicians { get; set; }

        [Required(ErrorMessage = "Assignment date is required")]
        [DisplayName("Assigned At")]
        [DataType(DataType.Date)]
        public DateTime AssignedAt { get; set; }

        public OrderTechnicianEditVm()
        {
            AllOrders = new List<SelectListItem>();
            AllTechnicians = new List<SelectListItem>();
        }
    }
}
