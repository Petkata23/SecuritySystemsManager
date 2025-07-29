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
    public class InstalledDeviceController : BaseCrudController<InstalledDeviceDto, IInstalledDeviceRepository, IInstalledDeviceService, InstalledDeviceEditVm, InstalledDeviceDetailsVm>
    {
        protected readonly ISecuritySystemOrderService _orderService;
        protected readonly IUserService _userService;

        public InstalledDeviceController(IMapper mapper, IInstalledDeviceService service, 
            ISecuritySystemOrderService orderService, IUserService userService)
            : base(service, mapper)
        {
            _orderService = orderService;
            _userService = userService;
        }

        protected override async Task<InstalledDeviceEditVm> PrePopulateVMAsync(InstalledDeviceEditVm editVM)
        {
            editVM.AllOrders = (await _orderService.GetAllAsync())
                .Select(o => new SelectListItem(o.Title, o.Id.ToString()));
            
            editVM.DeviceTypeOptions = Enum.GetValues(typeof(DeviceType))
                .Cast<DeviceType>()
                .Select(s => new SelectListItem(s.ToString(), ((int)s).ToString()));
            
            editVM.AllTechnicians = (await _userService.GetAllAsync())
                .Where(u => u.Role?.RoleType == RoleType.Technician)
                .Select(t => new SelectListItem($"{t.FirstName} {t.LastName}", t.Id.ToString()));
            
            return editVM;
        }
        
        public override async Task<IActionResult> Details(int id)
        {
            try
            {
                var device = await _service.GetByIdIfExistsAsync(id);
                if (device == null)
                    return RedirectToAction("Error404", "Error");
                    
                var detailsVM = _mapper.Map<InstalledDeviceDetailsVm>(device);
                
                // Pass the order ID to the view for navigation
                if (device.SecuritySystemOrderId > 0)
                {
                    ViewBag.OrderId = device.SecuritySystemOrderId;
                }
                
                return View(detailsVM);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Edit(int id, InstalledDeviceEditVm editVM)
        {
            if (!ModelState.IsValid)
            {
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
            
            try
            {
                var deviceDto = _mapper.Map<InstalledDeviceDto>(editVM);
                
                // Check if we need to remove the image
                bool removeImage = Request.Form.ContainsKey("RemoveImage") && Request.Form["RemoveImage"] == "true";
                
                // Update device with image handling
                await _service.UpdateDeviceAsync(deviceDto, editVM.DeviceImageFile, removeImage);
                
                TempData["SuccessMessage"] = "Device updated successfully.";
                return RedirectToAction(nameof(Details), new { id = deviceDto.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating device: {ex.Message}");
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Delete(int id)
        {
            var device = await _service.GetByIdIfExistsAsync(id);
            if (device == null)
                return NotFound();
            
            int orderId = device.SecuritySystemOrderId;
            
            await _service.DeleteAsync(id);
            
            TempData["SuccessMessage"] = "Device deleted successfully.";
            
            // Redirect to the order details if available
            if (orderId > 0)
                return RedirectToAction("Details", "SecuritySystemOrder", new { id = orderId });
                
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> AddToOrder(int orderId)
        {
            var order = await _orderService.GetByIdIfExistsAsync(orderId);
            if (order == null)
                return NotFound();
                
            var editVM = new InstalledDeviceEditVm
            {
                SecuritySystemOrderId = orderId,
                DateInstalled = DateTime.Now
            };
            
            // Populate dropdowns
            editVM = await PrePopulateVMAsync(editVM);
            
            // If current user is a technician, pre-select them
            if (User.IsInRole(RoleType.Technician.ToString()))
            {
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    editVM.InstalledById = userId;
                }
            }
            
            ViewBag.IsAddingToOrder = true;
            return View("Create", editVM);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddToOrder(InstalledDeviceEditVm editVM)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate dropdown lists
                editVM = await PrePopulateVMAsync(editVM);
                ViewBag.IsAddingToOrder = true;
                return View("Create", editVM);
            }
            
            // Map view model to DTO
            var deviceDto = _mapper.Map<InstalledDeviceDto>(editVM);
            
            try
            {
                // Add device with image handling in service
                await _service.AddDeviceToOrderAsync(deviceDto, editVM.DeviceImageFile);
                
                TempData["SuccessMessage"] = "Device added successfully.";
                return RedirectToAction("Details", "SecuritySystemOrder", new { id = editVM.SecuritySystemOrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding device: {ex.Message}");
                
                // Re-populate dropdown lists
                editVM = await PrePopulateVMAsync(editVM);
                ViewBag.IsAddingToOrder = true;
                return View("Create", editVM);
            }
        }
    }
} 