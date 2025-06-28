using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class InstalledDeviceController : BaseCrudController<InstalledDeviceDto, IInstalledDeviceRepository, IInstalledDeviceService, InstalledDeviceEditVm, InstalledDeviceDetailsVm>
    {
        protected readonly ISecuritySystemOrderService _orderService;

        public InstalledDeviceController(IMapper mapper, IInstalledDeviceService service, ISecuritySystemOrderService orderService)
            : base(service, mapper)
        {
            _orderService = orderService;
        }

        protected override async Task<InstalledDeviceEditVm> PrePopulateVMAsync(InstalledDeviceEditVm editVM)
        {
            editVM.AllOrders = (await _orderService.GetAllAsync())
                .Select(o => new SelectListItem(o.Title, o.Id.ToString()));
            
            return editVM;
        }
    }
} 