using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class OrderTechnicianDetailsVm : BaseVm
    {
        [DisplayName("Order")]
        public string OrderTitle { get; set; }

        [DisplayName("Technician")]
        public string TechnicianFullName { get; set; }

        [DisplayName("Assigned At")]
        public DateTime AssignedAt { get; set; }
    }
}
