using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class LoginWith2faViewModel
    {
        [Required(ErrorMessage = "Authentication code is required")]
        [StringLength(7, ErrorMessage = "The code must be between {2} and {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authentication code")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Remember this device")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
    }
} 