using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize]
    public class LocationController : BaseCrudController<LocationDto, ILocationRepository, ILocationService, LocationEditVm, LocationDetailsVm>
    {
        public LocationController(ILocationService service, IMapper mapper) : base(service, mapper)
        {
        }

        [HttpGet]
        public override async Task<IActionResult> List(int pageSize = 10, int pageNumber = 1)
        {
            try
            {
                ViewBag.Title = "My Locations";
                
                // Get current user ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return RedirectToAction("Login", "Auth");
                }
                
                // Check if user is admin or manager - they can see all locations
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    return await base.List(pageSize, pageNumber);
                }
                
                // For regular users, get only their locations
                var userLocations = await _service.GetLocationsForUserAsync(userIdInt, pageSize, pageNumber);
                var locationViewModels = _mapper.Map<IEnumerable<LocationDetailsVm>>(userLocations);
                return View("List", locationViewModels);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public override async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            ViewBag.Title = "Create Location";
            return await base.Create();
        }

        [HttpGet]
        public override async Task<IActionResult> Edit(int? id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            ViewBag.Title = "Edit Location";
            return await base.Edit(id);
        }

        [HttpGet]
        public override async Task<IActionResult> Details(int id)
        {
            ViewBag.Title = "Location Details";
            return await base.Details(id);
        }

        [HttpGet]
        public override async Task<IActionResult> Delete(int? id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            ViewBag.Title = "Delete Location";
            return await base.Delete(id);
        }

        [HttpPost]
        public override async Task<IActionResult> Create(LocationEditVm editVM)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            return await base.Create(editVM);
        }

        [HttpPost]
        public override async Task<IActionResult> Edit(int id, LocationEditVm editVM)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            return await base.Edit(id, editVM);
        }

        [HttpPost]
        public override async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            
            return await base.Delete(id);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllLocations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return Json(new List<object>());
                }
                
                var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                var locationsWithOrders = await _service.GetLocationsWithOrdersForCurrentUserAsync(userIdInt, isAdminOrManager);
                return Json(locationsWithOrders);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CreateLocationAjax([FromBody] LocationEditVm locationData)
        {
            try
            {
                if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Map ViewModel to DTO
                var locationDto = new LocationDto
                {
                    Name = locationData.Name,
                    Address = locationData.Address,
                    Latitude = locationData.Latitude,
                    Longitude = locationData.Longitude,
                    Description = locationData.Description
                };

                // Call service method
                var result = await _service.CreateLocationAjaxAsync(locationDto);

                return Json(new 
                { 
                    success = result.success, 
                    locationId = result.locationId,
                    locationName = result.locationName,
                    message = result.message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 