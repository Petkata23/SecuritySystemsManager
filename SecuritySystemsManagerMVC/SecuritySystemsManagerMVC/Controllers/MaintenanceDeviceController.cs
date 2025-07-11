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
        protected readonly IMaintenanceLogService _maintenanceLogService;

        public MaintenanceDeviceController(IMapper mapper, IMaintenanceDeviceService service, 
            IInstalledDeviceService installedDeviceService, IMaintenanceLogService maintenanceLogService)
            : base(service, mapper)
        {
            _installedDeviceService = installedDeviceService;
            _maintenanceLogService = maintenanceLogService;
        }

        // GET: MaintenanceDevice/Create
        public override async Task<IActionResult> Create()
        {
            return View(await PrePopulateVMAsync(new MaintenanceDeviceEditVm()));
        }

        // GET: MaintenanceDevice/CreateForLog/5
        [HttpGet]
        [Route("MaintenanceDevice/CreateForLog/{logId}")]
        public async Task<IActionResult> CreateForLog(int logId)
        {
            var log = await _maintenanceLogService.GetByIdIfExistsAsync(logId);
            if (log == null)
            {
                return NotFound();
            }

            var vm = await PrePopulateVMAsync(new MaintenanceDeviceEditVm { MaintenanceLogId = logId });
            return View("Create", vm);
        }

        protected override async Task<MaintenanceDeviceEditVm> PrePopulateVMAsync(MaintenanceDeviceEditVm editVM)
        {
            editVM.AllInstalledDevices = (await _installedDeviceService.GetAllAsync())
                .Select(d => new SelectListItem($"{d.Brand} {d.Model} - {d.DeviceType}", d.Id.ToString()));
            
            editVM.AllLogs = (await _maintenanceLogService.GetAllAsync())
                .Select(l => new SelectListItem($"Log #{l.Id} - {l.Date:MM/dd/yyyy}", l.Id.ToString()));
            
            return editVM;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFixed(int id)
        {
            var device = await _service.GetByIdIfExistsAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            device.IsFixed = !device.IsFixed;
            await _service.SaveAsync(device);
            
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllFixed(int logId)
        {
            var log = await _maintenanceLogService.GetByIdIfExistsAsync(logId);
            if (log == null)
            {
                return NotFound();
            }

            var allDevices = (await _service.GetAllAsync()).Where(d => d.MaintenanceLogId == logId).ToList();
            
            foreach (var device in allDevices)
            {
                device.IsFixed = true;
                await _service.SaveAsync(device);
            }
            
            return RedirectToAction("Details", "MaintenanceLog", new { id = logId });
        }
    }
} 