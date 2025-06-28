using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class OrderTechnicianController : BaseCrudController<OrderTechnicianDto, IOrderTechnicianRepository, IOrderTechnicianService, OrderTechnicianEditVm, OrderTechnicianDetailsVm>
    {
        protected readonly ISecuritySystemOrderService _orderService;
        protected readonly IUserService _userService;

        public OrderTechnicianController(IMapper mapper, IOrderTechnicianService service, 
            ISecuritySystemOrderService orderService, IUserService userService)
            : base(service, mapper)
        {
            _orderService = orderService;
            _userService = userService;
        }

        protected override async Task<OrderTechnicianEditVm> PrePopulateVMAsync(OrderTechnicianEditVm editVM)
        {
            editVM.AllOrders = (await _orderService.GetAllAsync())
                .Select(o => new SelectListItem(o.Title, o.Id.ToString()));

            editVM.AllTechnicians = (await _userService.GetAllAsync())
                .Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.Id.ToString()));
            
            return editVM;
        }
    }
} 