using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class RoleController : BaseCrudController<RoleDto, IRoleRepository, IRoleService, RoleEditVm, RoleDetailsVm>
    {
        public RoleController(IMapper mapper, IRoleService service)
            : base(service, mapper)
        {
        }
    }
} 