using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize]
    public class MaintenanceDeviceController : BaseCrudController<MaintenanceDeviceDto, IMaintenanceDeviceRepository, IMaintenanceDeviceService, MaintenanceDeviceEditVm, MaintenanceDeviceDetailsVm>
    {
        private readonly IInstalledDeviceService _installedDeviceService;
        private readonly IMaintenanceLogService _maintenanceLogService;

        public MaintenanceDeviceController(
            IMapper mapper,
            IMaintenanceDeviceService service,
            IInstalledDeviceService installedDeviceService,
            IMaintenanceLogService maintenanceLogService)
            : base(service, mapper)
        {
            _installedDeviceService = installedDeviceService;
            _maintenanceLogService = maintenanceLogService;
        }

        // GET: MaintenanceDevice/Create
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Create()
        {
            return View(await PrePopulateVMAsync(new MaintenanceDeviceEditVm()));
        }

        // GET: MaintenanceDevice/CreateForLog/5
        [HttpGet]
        [Route("MaintenanceDevice/CreateForLog/{logId}")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> CreateForLog(int logId)
        {
            var log = await _maintenanceLogService.GetByIdIfExistsAsync(logId);
            if (log == null)
            {
                return NotFound();
            }

            var vm = await PrePopulateVMAsync(new MaintenanceDeviceEditVm { MaintenanceLogId = logId });
            
            // Добавяне на информация за лога във ViewBag
            ViewBag.LogId = logId;
            ViewBag.LogDate = log.Date.ToShortDateString();
            ViewBag.IsFromMaintenanceLog = true;
            
            return View("Create", vm);
        }
        
        // POST: MaintenanceDevice/CreateForLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("MaintenanceDevice/CreateForLog/{logId}")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> CreateForLog(MaintenanceDeviceEditVm vm, int logId)
        {
            if (!ModelState.IsValid)
            {
                // Презареждане на данните за формата
                vm = await PrePopulateVMAsync(vm);
                
                // Добавяне на информация за лога във ViewBag
                var log = await _maintenanceLogService.GetByIdIfExistsAsync(logId);
                if (log != null)
                {
                    ViewBag.LogId = logId;
                    ViewBag.LogDate = log.Date.ToShortDateString();
                    ViewBag.IsFromMaintenanceLog = true;
                }
                
                return View("Create", vm);
            }

            try
            {
                // Маппинг към DTO
                var maintenanceDeviceDto = _mapper.Map<MaintenanceDeviceDto>(vm);
                
                // Използване на сервиса за добавяне на устройство към лог
                await _service.AddDeviceToMaintenanceLogAsync(maintenanceDeviceDto, logId);
                
                TempData["SuccessMessage"] = "Device added to maintenance log successfully.";
                
                // Пренасочване към детайлите на лога
                return RedirectToAction("Details", "MaintenanceLog", new { id = logId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                
                // Презареждане на данните за формата
                vm = await PrePopulateVMAsync(vm);
                
                // Добавяне на информация за лога във ViewBag
                var log = await _maintenanceLogService.GetByIdIfExistsAsync(logId);
                if (log != null)
                {
                    ViewBag.LogId = logId;
                    ViewBag.LogDate = log.Date.ToShortDateString();
                    ViewBag.IsFromMaintenanceLog = true;
                }
                
                return View("Create", vm);
            }
        }

        // GET: MaintenanceDevice/AddDeviceToMaintenance/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> AddDeviceToMaintenance(int deviceId)
        {
            try
            {
                // Използване на сервиса за подготовка на модела
                var maintenanceDeviceDto = await _service.PrepareMaintenanceDeviceAsync(deviceId);
                
                // Маппинг към ViewModel
                var vm = _mapper.Map<MaintenanceDeviceEditVm>(maintenanceDeviceDto);
                
                // Добавяне на информация за устройството във ViewBag
                ViewBag.DeviceInfo = $"{maintenanceDeviceDto.InstalledDevice.Brand} {maintenanceDeviceDto.InstalledDevice.Model}";
                ViewBag.DeviceId = deviceId;
                
                // Презареждане на данните за формата
                vm = await PrePopulateVMAsync(vm);
                
                return View("Create", vm);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> AddDeviceToMaintenance(MaintenanceDeviceEditVm vm, int deviceId)
        {
            if (!ModelState.IsValid)
            {
                // Презареждане на данните за формата
                vm = await PrePopulateVMAsync(vm);
                ViewBag.DeviceId = deviceId;
                
                // Вземане на информация за устройството
                var device = await _installedDeviceService.GetByIdIfExistsAsync(deviceId);
                if (device != null)
                {
                    ViewBag.DeviceInfo = $"{device.Brand} {device.Model}";
                }
                
                return View("Create", vm);
            }

            try
            {
                // Маппинг към DTO
                var maintenanceDeviceDto = _mapper.Map<MaintenanceDeviceDto>(vm);
                
                // Използване на сервиса за добавяне на устройство към поддръжка
                await _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDeviceDto, deviceId);
                
                TempData["SuccessMessage"] = "Maintenance record added successfully.";
                
                // Пренасочване към детайлите на устройството
                return RedirectToAction("Details", "InstalledDevice", new { id = deviceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                
                // Презареждане на данните за формата
                vm = await PrePopulateVMAsync(vm);
                ViewBag.DeviceId = deviceId;
                
                // Вземане на информация за устройството
                var device = await _installedDeviceService.GetByIdIfExistsAsync(deviceId);
                if (device != null)
                {
                    ViewBag.DeviceInfo = $"{device.Brand} {device.Model}";
                }
                
                return View("Create", vm);
            }
        }

        protected override async Task<MaintenanceDeviceEditVm> PrePopulateVMAsync(MaintenanceDeviceEditVm editVM)
        {
            try
            {
                // Зареждане на списъка с maintenance logs
                var logs = await _maintenanceLogService.GetAllAsync();
                if (logs != null && logs.Any())
                {
                    editVM.AllMaintenanceLogs = logs.Select(l => new SelectListItem(
                        $"{l.Date.ToShortDateString()} - {l.Description?.Substring(0, Math.Min(50, l.Description?.Length ?? 0))}", 
                        l.Id.ToString()));
                }
                else
                {
                    editVM.AllMaintenanceLogs = Enumerable.Empty<SelectListItem>();
                }

                // Зареждане на списъка с устройства
                var devices = await _installedDeviceService.GetAllAsync();
                if (devices != null && devices.Any())
                {
                    editVM.AllInstalledDevices = devices.Select(d => new SelectListItem(
                        $"{d.Brand} {d.Model}", 
                        d.Id.ToString()));
                }
                else
                {
                    editVM.AllInstalledDevices = Enumerable.Empty<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                // Логване на грешката
                Console.WriteLine($"Error in PrePopulateVMAsync: {ex.Message}");
                
                // Задаване на празни колекции, за да не се счупи изгледът
                editVM.AllMaintenanceLogs = Enumerable.Empty<SelectListItem>();
                editVM.AllInstalledDevices = Enumerable.Empty<SelectListItem>();
            }

            return editVM;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> ToggleFixed(int id)
        {
            try
            {
                // Използване на сервиса за промяна на статуса
                await _service.ToggleDeviceFixedStatusAsync(id);
                
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> MarkAllFixed(int logId)
        {
            try
            {
                // Използване на сервиса за маркиране на всички устройства като поправени
                await _service.MarkAllDevicesFixedForLogAsync(logId);
                
                return RedirectToAction("Details", "MaintenanceLog", new { id = logId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // Override Create method to restrict access
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Create(MaintenanceDeviceEditVm editVM)
        {
            return await base.Create(editVM);
        }

        // Override Edit method to restrict access
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Edit(int? id)
        {
            return await base.Edit(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Edit(int id, MaintenanceDeviceEditVm editVM)
        {
            return await base.Edit(id, editVM);
        }

        // Override Delete method to restrict access
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Delete(int? id)
        {
            return await base.Delete(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
} 