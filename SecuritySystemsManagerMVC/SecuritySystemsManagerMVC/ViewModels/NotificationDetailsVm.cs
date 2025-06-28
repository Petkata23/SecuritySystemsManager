using System.Buffers.Text;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class NotificationDetailsVm : BaseVm
    {
        [DisplayName("Recipient Name")]
        public UserDetailsVm RecipientName { get; set; }

        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Date Sent")]
        public DateTime DateSent { get; set; }

        [DisplayName("Is Read")]
        public bool IsRead { get; set; }
    }
}
