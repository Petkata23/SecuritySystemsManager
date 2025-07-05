using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class LoginVm
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
