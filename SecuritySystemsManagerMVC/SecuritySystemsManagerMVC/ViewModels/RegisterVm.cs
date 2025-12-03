using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class RegisterVm 
    {
        [Required(ErrorMessage = "Потребителското име е задължително")]
        [StringLength(50)]
        [DisplayName("Потребителско име")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Паролата е задължителна")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да бъде поне 6 символа")]
        [DisplayName("Парола")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Потвърждението на паролата е задължително")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        [DisplayName("Потвърди парола")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(50)]
        [DisplayName("Име")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        [StringLength(50)]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        [DisplayName("Имейл")]
        public string Email { get; set; }

        [DisplayName("Профилна снимка")]
        public IFormFile? ProfileImageFile { get; set; }

        [DisplayName("URL на профилна снимка")]
        public string? ProfileImage { get; set; }
    }
}
