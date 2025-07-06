using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : BaseCrudController<RoleDto, IRoleRepository, IRoleService, RoleEditVm, RoleDetailsVm>
    {
        public RoleController(IMapper mapper, IRoleService service)
            : base(service, mapper)
        {
        }
        
        protected override async Task<RoleEditVm> PrePopulateVMAsync(RoleEditVm editVM)
        {
            editVM.RoleTypeOptions = Enum.GetValues(typeof(SecuritySystemsManager.Shared.Enums.RoleType))
                .Cast<SecuritySystemsManager.Shared.Enums.RoleType>()
                .Select(rt => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = rt.ToString(),
                    Value = ((int)rt).ToString()
                }).ToList();
                
            return await base.PrePopulateVMAsync(editVM);
        }
    }
} 