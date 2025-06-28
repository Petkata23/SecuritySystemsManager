using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class RoleEditVm : BaseVm
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name must be up to 50 characters")]
        [DisplayName("Role Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role type is required")]
        [DisplayName("Role Type")]
        public RoleType RoleType { get; set; }

        public IEnumerable<SelectListItem> RoleTypeOptions { get; set; }

        public RoleEditVm()
        {
            RoleTypeOptions = new List<SelectListItem>();
        }
    }
}
