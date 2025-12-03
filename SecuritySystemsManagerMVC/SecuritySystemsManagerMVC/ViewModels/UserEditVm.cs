using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class UserEditVm : BaseVm
    {
        [Required(ErrorMessage = "Потребителското име е задължително")]
        [StringLength(50, ErrorMessage = "Потребителското име трябва да бъде до 50 символа")]
        [DisplayName("Потребителско име")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(50, ErrorMessage = "Името трябва да бъде до 50 символа")]
        [DisplayName("Име")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        [StringLength(50, ErrorMessage = "Фамилията трябва да бъде до 50 символа")]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        public string Email { get; set; }

        [DisplayName("Профилна снимка")]
        public string? ProfileImage { get; set; }

        [Required(ErrorMessage = "Ролята е задължителна")]
        [DisplayName("Роля")]
        public int RoleId { get; set; }

        public IEnumerable<SelectListItem> AvailableRoles { get; set; }

        public UserEditVm()
        {
            AvailableRoles = new List<SelectListItem>();
        }
    }
}
