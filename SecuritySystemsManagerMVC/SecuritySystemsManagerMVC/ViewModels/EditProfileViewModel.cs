using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class EditProfileViewModel
    {
        [Display(Name = "Username")]
        [ReadOnly(true)]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "Profile Image")]
        public IFormFile ProfileImageFile { get; set; }
    }
} 