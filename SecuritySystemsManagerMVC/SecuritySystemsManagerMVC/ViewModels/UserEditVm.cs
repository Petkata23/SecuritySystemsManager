using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class UserEditVm : BaseVm
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be up to 50 characters")]
        [DisplayName("Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name must be up to 50 characters")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name must be up to 50 characters")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [DisplayName("Profile Image")]
        public string? ProfileImage { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [DisplayName("Role")]
        public int RoleId { get; set; }

        public IEnumerable<SelectListItem> AvailableRoles { get; set; }

        public UserEditVm()
        {
            AvailableRoles = new List<SelectListItem>();
        }
    }
}
