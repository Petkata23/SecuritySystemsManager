using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationApiController : BaseApiController<LocationDto, ILocationService>
    {
        public LocationApiController(ILocationService service) : base(service)
        {
        }

        [HttpGet]
        public override async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    return await base.GetAll(pageSize, pageNumber);
                }

                if (User.IsInRole("Technician"))
                {
                    var technicianLocations = await _service.GetLocationsForTechnicianAsync(userIdInt, pageSize, pageNumber);
                    return Ok(new { data = technicianLocations });
                }

                var userLocations = await _service.GetLocationsForUserAsync(userIdInt, pageSize, pageNumber);
                return Ok(new { data = userLocations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                var isTechnician = User.IsInRole("Technician");

                IEnumerable<object> locationsWithOrders;
                if (isAdminOrManager)
                {
                    locationsWithOrders = await _service.GetLocationsWithOrdersForCurrentUserAsync(userIdInt, true);
                    Console.WriteLine($"[LocationApi] UserId: {userIdInt}, Role: Admin/Manager, Locations: {locationsWithOrders?.Count() ?? 0}");
                }
                else if (isTechnician)
                {
                    locationsWithOrders = await _service.GetLocationsWithOrdersForTechnicianAsync(userIdInt);
                    Console.WriteLine($"[LocationApi] UserId: {userIdInt}, Role: Technician, Locations: {locationsWithOrders?.Count() ?? 0}");
                }
                else
                {
                    locationsWithOrders = await _service.GetLocationsWithOrdersForCurrentUserAsync(userIdInt, false);
                    Console.WriteLine($"[LocationApi] UserId: {userIdInt}, Role: Client, Locations: {locationsWithOrders?.Count() ?? 0}");
                }
                
                return Ok(locationsWithOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateLocation([FromBody] LocationDto locationData)
        {
            try
            {
                var result = await _service.CreateLocationAjaxAsync(locationData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
