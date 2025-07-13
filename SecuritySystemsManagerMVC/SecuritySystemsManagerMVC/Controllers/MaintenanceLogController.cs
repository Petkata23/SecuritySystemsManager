using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize]
    public class MaintenanceLogController : BaseCrudController<MaintenanceLogDto, IMaintenanceLogRepository, IMaintenanceLogService, MaintenanceLogEditVm, MaintenanceLogDetailsVm>
    {
        protected readonly ISecuritySystemOrderService _orderService;
        protected readonly IUserService _userService;

        public MaintenanceLogController(IMapper mapper, IMaintenanceLogService service, 
            ISecuritySystemOrderService orderService, IUserService userService)
            : base(service, mapper)
        {
            _orderService = orderService;
            _userService = userService;
        }

        protected override async Task<MaintenanceLogEditVm> PrePopulateVMAsync(MaintenanceLogEditVm editVM)
        {
            editVM.AllOrders = (await _orderService.GetAllAsync())
                .Select(o => new SelectListItem(o.Title, o.Id.ToString()));

            editVM.AllTechnicians = (await _userService.GetAllAsync())
                .Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.Id.ToString()));
            
            return editVM;
        }

        // GET: MaintenanceLog/CreateForOrder/5
        [HttpGet]
        [Route("MaintenanceLog/CreateForOrder/{orderId}")]
        public async Task<IActionResult> CreateForOrder(int orderId)
        {
            try
            {
                // Определяне на текущия потребител ако е техник
                int? technicianId = null;
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string userRole = User.FindFirstValue(ClaimTypes.Role);
                
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && userRole == "Technician")
                {
                    technicianId = userId;
                }
                
                // Използване на сервиса за подготовка на модела
                var maintenanceLogDto = await _service.PrepareMaintenanceLogForOrderAsync(orderId, technicianId);
                
                // Маппинг към ViewModel и попълване на допълнителни данни
                var vm = _mapper.Map<MaintenanceLogEditVm>(maintenanceLogDto);
                vm = await PrePopulateVMAsync(vm);
                
                ViewBag.OrderTitle = maintenanceLogDto.SecuritySystemOrder?.Title;
                return View("Create", vm);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public override async Task<IActionResult> Details(int id)
        {
            var model = await this._service.GetByIdIfExistsAsync(id);
            if (model == default)
            {
                return BadRequest(SecuritySystemsManager.Shared.Constants.InvalidId);
            }
            
            // Set the order ID in ViewBag for the link back to the order
            ViewBag.OrderId = model.SecuritySystemOrderId;
            
            var mappedModel = _mapper.Map<MaintenanceLogDetailsVm>(model);
            return View(mappedModel);
        }

        [HttpGet]
        public override async Task<IActionResult> List(int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(SecuritySystemsManager.Shared.Constants.InvalidPagination);
            }

            // Get current user ID and role
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userRole))
            {
                return Unauthorized();
            }
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get logs based on user role
            var logs = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);
            var totalLogs = await _service.GetLogsCountByUserRoleAsync(userId, userRole);
            var totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<MaintenanceLogDetailsVm>>(logs);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(nameof(List), mappedModels);
        }
    }
} 