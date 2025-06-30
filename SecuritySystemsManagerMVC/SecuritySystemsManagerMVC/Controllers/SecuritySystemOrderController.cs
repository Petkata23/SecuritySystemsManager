using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class SecuritySystemOrderController : BaseCrudController<SecuritySystemOrderDto, ISecuritySystemOrderRepository, ISecuritySystemOrderService, SecuritySystemOrderEditVm, SecuritySystemOrderDetailsVm>
    {
        protected readonly IUserService _userService;
        protected readonly ILocationService _locationService;

        public SecuritySystemOrderController(IMapper mapper, ISecuritySystemOrderService service, 
            IUserService userService, ILocationService locationService)
            : base(service, mapper)
        {
            _userService = userService;
            _locationService = locationService;
        }

        protected override async Task<SecuritySystemOrderEditVm> PrePopulateVMAsync(SecuritySystemOrderEditVm editVM)
        {
            editVM.AllClients = (await _userService.GetAllAsync())
                .Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.Id.ToString()));

            editVM.AllLocations = (await _locationService.GetAllAsync())
                .Select(l => new SelectListItem(l.Name, l.Id.ToString()));
            
            return editVM;
        }

        [HttpGet]
        public override async Task<IActionResult> Create()
        {
            var editVM = await PrePopulateVMAsync(new SecuritySystemOrderEditVm());
            
            if (User.IsInRole(RoleType.Client.ToString()))
            {
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    editVM.ClientId = userId;
                }
            }
            
            return View(editVM);
        }

        [HttpPost]
        public override async Task<IActionResult> Create(SecuritySystemOrderEditVm editVM)
        {
            if (User.IsInRole(RoleType.Client.ToString()))
            {
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    editVM.ClientId = userId;
                }
            }
            
            return await base.Create(editVM);
        }
    }
} 