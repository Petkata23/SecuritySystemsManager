using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class UserDetailsVm : BaseVm
    {
        [DisplayName("Username")]
        public string Username { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Profile Image")]
        public string? ProfileImage { get; set; }

        [DisplayName("Role")]
        public string RoleName { get; set; }

        [DisplayName("Orders (as Client)")]
        public List<SecuritySystemOrderDetailsVm> OrdersAsClient { get; set; }

        [DisplayName("Assigned Orders (as Technician)")]
        public List<SecuritySystemOrderDetailsVm> AssignedOrders { get; set; }

        public UserDetailsVm()
        {
            OrdersAsClient = new List<SecuritySystemOrderDetailsVm>();
            AssignedOrders = new List<SecuritySystemOrderDetailsVm>();
        }
    }
}
