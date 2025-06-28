using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class UserController : BaseCrudController<UserDto, IUserRepository, IUserService, UserEditVm, UserDetailsVm>
    {
        protected readonly IRoleService _roleService;

        public UserController(IMapper mapper, IUserService service, IRoleService roleService)
            : base(service, mapper)
        {
            _roleService = roleService;
        }

        protected override async Task<UserEditVm> PrePopulateVMAsync(UserEditVm editVM)
        {
            editVM.AvailableRoles = (await _roleService.GetAllAsync())
                .Select(r => new SelectListItem(r.Name, r.Id.ToString()));
            
            return editVM;
        }
    }
} 