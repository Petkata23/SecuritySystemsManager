using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

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
    }
} 