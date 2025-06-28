using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class RoleDetailsVm : BaseVm
    {
        [DisplayName("Role Name")]
        public string Name { get; set; }

        [DisplayName("Role Type")]
        public string RoleType { get; set; }

        [DisplayName("Users in Role")]
        public List<UserDetailsVm> Users { get; set; }

        public RoleDetailsVm()
        {
            Users = new List<UserDetailsVm>();
        }
    }
}
