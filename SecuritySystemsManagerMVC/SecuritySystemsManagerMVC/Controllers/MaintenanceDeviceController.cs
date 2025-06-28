using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class MaintenanceDeviceController : BaseCrudController<MaintenanceDeviceDto, IMaintenanceDeviceRepository, IMaintenanceDeviceService, MaintenanceDeviceEditVm, MaintenanceDeviceDetailsVm>
    {
        protected readonly IInstalledDeviceService _installedDeviceService;

        public MaintenanceDeviceController(IMapper mapper, IMaintenanceDeviceService service, IInstalledDeviceService installedDeviceService)
            : base(service, mapper)
        {
            _installedDeviceService = installedDeviceService;
        }

        protected override async Task<MaintenanceDeviceEditVm> PrePopulateVMAsync(MaintenanceDeviceEditVm editVM)
        {
            editVM.AllInstalledDevices = (await _installedDeviceService.GetAllAsync())
                .Select(d => new SelectListItem($"{d.Brand} {d.Model} - {d.DeviceType}", d.Id.ToString()));
            
            return editVM;
        }
    }
} 