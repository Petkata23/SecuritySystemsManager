using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        protected readonly INotificationService _notificationService;

        public SecuritySystemOrderController(IMapper mapper, ISecuritySystemOrderService service, 
            IUserService userService, ILocationService locationService, INotificationService notificationService)
            : base(service, mapper)
        {
            _userService = userService;
            _locationService = locationService;
            _notificationService = notificationService;
        }

        protected override async Task<SecuritySystemOrderEditVm> PrePopulateVMAsync(SecuritySystemOrderEditVm editVM)
        {
            editVM.AllClients = (await _userService.GetAllAsync())
                .Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.Id.ToString()));

            editVM.AllLocations = (await _locationService.GetAllAsync())
                .Select(l => new SelectListItem(l.Name, l.Id.ToString()));
            
            // Populate status options
            editVM.StatusOptions = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new SelectListItem(s.ToString(), ((int)s).ToString()));
            
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

        public override async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetByIdIfExistsAsync(id);
            if (order == null)
                return NotFound();

            // Only get available technicians for admins and managers
            if (User.IsInRole(RoleType.Admin.ToString()) || User.IsInRole(RoleType.Manager.ToString()))
            {
                // Get all technicians (users with Technician role)
                var allTechnicians = (await _userService.GetAllAsync())
                    .Where(u => u.Role?.RoleType == RoleType.Technician)
                    .ToList();

                // Get technicians that are not already assigned to this order
                var availableTechnicians = allTechnicians
                    .Where(t => order.Technicians == null || !order.Technicians.Any(at => at.Id == t.Id))
                    .ToList();

                // Pass available technicians to the view
                ViewBag.AvailableTechnicians = availableTechnicians;
                
                // Pass authorization info to the view
                ViewBag.CanManageTechnicians = true;
            }
            else
            {
                ViewBag.CanManageTechnicians = false;
            }

            // Continue with the base implementation
            var result = await base.Details(id);
            
            // Re-apply ViewBag values after base implementation
            if (User.IsInRole(RoleType.Admin.ToString()) || User.IsInRole(RoleType.Manager.ToString()))
            {
                ViewBag.AvailableTechnicians = ViewBag.AvailableTechnicians;
                ViewBag.CanManageTechnicians = true;
            }
            
            return result;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddTechnician(int orderId, int technicianId)
        {
            try
            {
                await _service.AddTechnicianToOrderAsync(orderId, technicianId);
                TempData["SuccessMessage"] = "Technician successfully assigned to the order.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning technician: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveTechnician(int orderId, int technicianId)
        {
            try
            {
                await _service.RemoveTechnicianFromOrderAsync(orderId, technicianId);
                TempData["SuccessMessage"] = "Technician successfully removed from the order.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error removing technician: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int? id)
        {
            return await base.Edit(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int id, SecuritySystemOrderEditVm editVM)
        {
            // Get the original order to check if status has changed
            var originalOrder = await _service.GetByIdIfExistsAsync(id);
            if (originalOrder != null && originalOrder.Status != editVM.Status)
            {
                // Status has changed, we need to send a notification
                var result = await base.Edit(id, editVM);
                
                // Send notification to client about status change
                await _notificationService.SendOrderStatusChangeNotificationAsync(
                    id, 
                    editVM.ClientId, 
                    originalOrder.Status, 
                    editVM.Status);
                
                return result;
            }
            
            return await base.Edit(id, editVM);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Delete(int? id)
        {
            return await base.Delete(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
} 