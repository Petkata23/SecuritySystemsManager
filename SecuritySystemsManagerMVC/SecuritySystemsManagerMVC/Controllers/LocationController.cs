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
                
                // Check if user is technician - they can see locations where they are assigned
                if (User.IsInRole("Technician"))
                {
                    var technicianLocations = await _service.GetLocationsForTechnicianAsync(userIdInt, pageSize, pageNumber);
                    var technicianLocationViewModels = _mapper.Map<IEnumerable<LocationDetailsVm>>(technicianLocations);
                    return View("List", technicianLocationViewModels);
                }
                
                // For regular users (clients), get only their locations
                var userLocations = await _service.GetLocationsForUserAsync(userIdInt, pageSize, pageNumber);
                var userLocationViewModels = _mapper.Map<IEnumerable<LocationDetailsVm>>(userLocations);
                return View("List", userLocationViewModels);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Create()
        {
            ViewBag.Title = "Create Location";
            return await base.Create();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int? id)
        {
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
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Title = "Delete Location";
            return await base.Delete(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Create(LocationEditVm editVM)
        {
            return await base.Create(editVM);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int id, LocationEditVm editVM)
        {
            return await base.Edit(id, editVM);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Delete(int id)
        {
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
                var isTechnician = User.IsInRole("Technician");
                
                if (isAdminOrManager)
                {
                    var locationsWithOrders = await _service.GetLocationsWithOrdersForCurrentUserAsync(userIdInt, true);
                    return Json(locationsWithOrders);
                }
                else if (isTechnician)
                {
                    var technicianLocationsWithOrders = await _service.GetLocationsWithOrdersForTechnicianAsync(userIdInt);
                    return Json(technicianLocationsWithOrders);
                }
                else
                {
                    var locationsWithOrders = await _service.GetLocationsWithOrdersForCurrentUserAsync(userIdInt, false);
                    return Json(locationsWithOrders);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<JsonResult> CreateLocationAjax([FromBody] LocationEditVm locationData)
        {
            try
            {

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