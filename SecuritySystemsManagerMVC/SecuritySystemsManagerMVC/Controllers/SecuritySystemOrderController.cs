using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystemsManager.Shared;
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
            
            // Устанавливаем дату по умолчанию на сегодня + 3 дня
            editVM.RequestedDate = DateTime.Now.AddDays(3);
            
            if (User.IsInRole("Client"))
            {
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    editVM.ClientId = userId;
                    editVM.Status = OrderStatus.Pending;
                }
            }
            
            return View(editVM);
        }

        [HttpPost]
        public override async Task<IActionResult> Create(SecuritySystemOrderEditVm editVM)
        {
            if (User.IsInRole("Client"))
            {
                string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    editVM.ClientId = userId;
                    // Автоматически устанавливаем статус Pending для клиентов
                    editVM.Status = OrderStatus.Pending;
                }
            }
            
            return await base.Create(editVM);
        }

        public override async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _service.GetOrderWithAllDetailsAsync(id);
                if (order == null)
                    return RedirectToAction("Error404", "Error");

                // Only get available technicians for admins and managers
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
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

                // Map to view model
                var viewModel = _mapper.Map<SecuritySystemOrderDetailsVm>(order);
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddTechnician(int orderId, int technicianId)
        {
            try
            {
                await _service.AddTechnicianToOrderAsync(orderId, technicianId);
                TempData["Success"] = "Technician successfully assigned to the order.";
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
                TempData["Success"] = "Technician successfully removed from the order.";
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

        [HttpGet]
        public override async Task<IActionResult> List(int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(Constants.InvalidPagination);
            }

            // Get current user ID and role
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Debug information
            Console.WriteLine($"User ID: {userIdStr}, User Role: {userRole}");
            
            // Check all claims
            Console.WriteLine("All claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }
            
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userRole))
            {
                return Unauthorized();
            }
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get orders based on user role
            var orders = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);
            var totalOrders = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            
            // Debug information
            Console.WriteLine($"Orders count: {orders.Count()}, Total orders: {totalOrders}, Total pages: {totalPages}");

            var mappedModels = _mapper.Map<IEnumerable<SecuritySystemOrderDetailsVm>>(orders);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(nameof(List), mappedModels);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GenerateInvoice(int orderId, decimal laborCost)
        {
            try
            {
                // Get the invoice service from DI
                var invoiceService = HttpContext.RequestServices.GetService<IInvoiceService>();
                if (invoiceService == null)
                {
                    TempData["ErrorMessage"] = "Invoice service not available";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }

                // Calculate total amount from form data
                decimal totalAmount = laborCost;
                
                // Get device costs from form data
                var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                foreach (var item in formData)
                {
                    if (item.Key.StartsWith("deviceCosts[") && item.Key.EndsWith("]"))
                    {
                        if (decimal.TryParse(item.Value, out decimal deviceCost))
                        {
                            totalAmount += deviceCost;
                        }
                    }
                }

                // Generate the invoice with calculated amount
                await invoiceService.GenerateInvoiceFromOrderAsync(orderId, totalAmount);
                
                TempData["SuccessMessage"] = $"Invoice generated successfully with total amount: ${totalAmount:F2}";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
} 