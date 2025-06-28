using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class SecuritySystemOrderEditVm : BaseVm
    {
        [Required]
        [StringLength(100)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        [Phone]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [DisplayName("Client")]
        public int ClientId { get; set; }
        public IEnumerable<SelectListItem> AllClients { get; set; }

        [Required]
        [DisplayName("Location")]
        public int LocationId { get; set; }
        public IEnumerable<SelectListItem> AllLocations { get; set; }

        [Required]
        [DisplayName("Status")]
        public OrderStatus Status { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }

        [DisplayName("Requested Date")]
        [DataType(DataType.Date)]
        public DateTime RequestedDate { get; set; }

        public SecuritySystemOrderEditVm()
        {
            AllClients = new List<SelectListItem>();
            AllLocations = new List<SelectListItem>();
            StatusOptions = new List<SelectListItem>();
        }
    }
}
