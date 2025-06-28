using Microsoft.AspNetCore.Mvc.Rendering;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class NotificationEditVm : BaseVm
    {
        public int UserId { get; set; }

        [DisplayName("Recipient")]
        [Required(ErrorMessage = "Recipient is required.")]
        public IEnumerable<SelectListItem> UserList { get; set; }

        [DisplayName("Message")]
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string Message { get; set; }

        [DisplayName("Is Read")]
        public bool IsRead { get; set; }
    }
}
