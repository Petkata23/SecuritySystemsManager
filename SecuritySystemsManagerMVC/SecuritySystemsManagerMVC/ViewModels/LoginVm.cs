using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class LoginVm
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
