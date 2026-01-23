using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderApiController : BaseApiController<SecuritySystemOrderDto, ISecuritySystemOrderService>
    {
        public OrderApiController(ISecuritySystemOrderService service) : base(service)
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

                // Determine user role - check for Admin/Manager first, then Technician, then Client
                string userRole = "Client";
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    userRole = User.IsInRole("Admin") ? "Admin" : "Manager";
                }
                else if (User.IsInRole("Technician"))
                {
                    userRole = "Technician";
                }

                var orders = await _service.GetOrdersByUserRoleAsync(userIdInt, userRole, pageSize, pageNumber);
                var totalCount = await _service.GetOrdersCountByUserRoleAsync(userIdInt, userRole);
                
                // Log for debugging
                Console.WriteLine($"[OrderApi] UserId: {userIdInt}, Role: {userRole}, Orders: {orders?.Count() ?? 0}, TotalCount: {totalCount}");

                // Convert to list to ensure proper serialization
                var ordersList = orders?.ToList() ?? new List<SecuritySystemOrderDto>();
                
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    data = ordersList,
                    pagination = new
                    {
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderApi] Error in GetAll: {ex.Message}");
                Console.WriteLine($"[OrderApi] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OrderApi] Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("filtered")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1)
        {
            try
            {
                DateTime? parsedStartDate = null;
                DateTime? parsedEndDate = null;

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
                {
                    parsedStartDate = start;
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
                {
                    parsedEndDate = end;
                }

                var (orders, totalCount) = await _service.GetFilteredOrdersAsync(
                    searchTerm,
                    parsedStartDate,
                    parsedEndDate,
                    status,
                    User,
                    pageSize,
                    pageNumber);

                // Convert to list to ensure proper serialization
                var ordersList = orders?.ToList() ?? new List<SecuritySystemOrderDto>();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    data = ordersList,
                    pagination = new
                    {
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderApi] Error in GetFiltered: {ex.Message}");
                Console.WriteLine($"[OrderApi] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OrderApi] Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
