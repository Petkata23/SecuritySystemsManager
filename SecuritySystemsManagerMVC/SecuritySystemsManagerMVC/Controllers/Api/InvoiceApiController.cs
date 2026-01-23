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
    public class InvoiceApiController : BaseApiController<InvoiceDto, IInvoiceService>
    {
        public InvoiceApiController(IInvoiceService service) : base(service)
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

                var invoices = await _service.GetInvoicesByUserRoleAsync(userIdInt, userRole, pageSize, pageNumber);
                var totalCount = await _service.GetInvoicesCountByUserRoleAsync(userIdInt, userRole);
                
                // Log for debugging
                Console.WriteLine($"[InvoiceApi] UserId: {userIdInt}, Role: {userRole}, Invoices: {invoices?.Count() ?? 0}, TotalCount: {totalCount}");

                // Convert to list to ensure proper serialization
                var invoicesList = invoices?.ToList() ?? new List<InvoiceDto>();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    data = invoicesList,
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
                Console.WriteLine($"[InvoiceApi] Error in GetAll: {ex.Message}");
                Console.WriteLine($"[InvoiceApi] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[InvoiceApi] Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("filtered")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1)
        {
            try
            {
                var (invoices, totalCount) = await _service.GetFilteredInvoicesAsync(
                    searchTerm,
                    paymentStatus,
                    User,
                    pageSize,
                    pageNumber);

                // Convert to list to ensure proper serialization
                var invoicesList = invoices?.ToList() ?? new List<InvoiceDto>();
                
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    data = invoicesList,
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
                Console.WriteLine($"[InvoiceApi] Error in GetFiltered: {ex.Message}");
                Console.WriteLine($"[InvoiceApi] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[InvoiceApi] Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
