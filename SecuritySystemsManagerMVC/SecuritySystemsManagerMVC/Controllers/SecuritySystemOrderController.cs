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
        [Authorize(Roles = "Admin,Manager,Client")]
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
        [Authorize(Roles = "Admin,Manager,Client")]
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
                        .Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.Id.ToString()));

                    ViewBag.AvailableTechnicians = allTechnicians;
                }

                var mappedModel = _mapper.Map<SecuritySystemOrderDetailsVm>(order);
                return View(mappedModel);
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
                TempData["Success"] = "Technician added successfully to the order.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the technician.";
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
                TempData["Success"] = "Technician removed successfully from the order.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while removing the technician.";
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
            try
            {
                // Simple validation - can stay in controller
                if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
                {
                    return BadRequest(Constants.InvalidPagination);
                }

                var (orders, totalCount) = await _service.GetFilteredOrdersAsync(null, null, null, null, User, pageSize, pageNumber);
                
                // Simple math - can stay in controller
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var mappedModels = _mapper.Map<IEnumerable<SecuritySystemOrderDetailsVm>>(orders);

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = pageNumber;
                // Add flag to indicate if there are any orders before filtering
                ViewBag.HasOrdersBeforeFilter = totalCount > 0;

                return View(mappedModels);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterOrders(string searchTerm = "", string startDate = "", string endDate = "", string status = "", int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            // Simple validation - can stay in controller
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(Constants.InvalidPagination);
            }

            // Use service to parse dates
            var (parsedStartDate, parsedEndDate, dateError) = await _service.ParseDateRangeAsync(startDate, endDate);
            if (!string.IsNullOrEmpty(dateError))
            {
                TempData["Error"] = dateError;
                return RedirectToAction(nameof(List));
            }

            // Get filtered orders with pagination from service
            var (orders, totalCount) = await _service.GetFilteredOrdersAsync(searchTerm, parsedStartDate, parsedEndDate, status, User, pageSize, pageNumber);
            
            // Simple math - can stay in controller
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<SecuritySystemOrderDetailsVm>>(orders);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchTermFilter = searchTerm;
            ViewBag.StartDateFilter = startDate;
            ViewBag.EndDateFilter = endDate;
            ViewBag.StatusFilter = status;

            return View(nameof(List), mappedModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrdersPartial(string searchTerm = "", string startDate = "", string endDate = "", string status = "", int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            // Simple validation - can stay in controller
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(Constants.InvalidPagination);
            }

            // Use service to parse dates
            var (parsedStartDate, parsedEndDate, dateError) = await _service.ParseDateRangeAsync(startDate, endDate);
            if (!string.IsNullOrEmpty(dateError))
            {
                return BadRequest(dateError);
            }

            // Get filtered orders with pagination from service
            var (orders, totalCount) = await _service.GetFilteredOrdersAsync(searchTerm, parsedStartDate, parsedEndDate, status, User, pageSize, pageNumber);
            
            // Simple math - can stay in controller
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<SecuritySystemOrderDetailsVm>>(orders);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchTermFilter = searchTerm;
            ViewBag.StartDateFilter = startDate;
            ViewBag.EndDateFilter = endDate;
            ViewBag.StatusFilter = status;
            
            // Check if there are orders before filtering by getting total count without filters
            var (allOrders, totalOrdersBeforeFilter) = await _service.GetFilteredOrdersAsync(null, null, null, null, User, 1, 1);
            ViewBag.HasOrdersBeforeFilter = totalOrdersBeforeFilter > 0;

            return PartialView("_OrdersTable", mappedModels);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GenerateInvoice(int orderId, decimal laborCost)
        {
            try
            {
                // Calculate total amount from form data
                decimal totalAmount = laborCost;
                
                // Get device costs from form data
                var deviceCosts = new Dictionary<string, decimal>();
                var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                foreach (var item in formData)
                {
                    if (item.Key.StartsWith("deviceCosts[") && item.Key.EndsWith("]"))
                    {
                        if (decimal.TryParse(item.Value, out decimal deviceCost))
                        {
                            deviceCosts[item.Key] = deviceCost;
                        }
                    }
                }

                // Use service to generate invoice
                var (success, errorMessage) = await _service.GenerateInvoiceFromOrderAsync(orderId, laborCost, deviceCosts);
                
                if (success)
                {
                    TempData["SuccessMessage"] = errorMessage; // Contains success message
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
} 