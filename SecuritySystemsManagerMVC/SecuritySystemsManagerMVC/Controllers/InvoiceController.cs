using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class InvoiceController : BaseCrudController<InvoiceDto, IInvoiceRepository, IInvoiceService, InvoiceEditVm, InvoiceDetailsVm>
    {
        protected readonly ISecuritySystemOrderService _orderService;

        public InvoiceController(IMapper mapper, IInvoiceService service, ISecuritySystemOrderService orderService)
            : base(service, mapper)
        {
            _orderService = orderService;
        }

        protected override async Task<InvoiceEditVm> PrePopulateVMAsync(InvoiceEditVm editVM)
        {
            editVM.AllOrders = (await _orderService.GetAllAsync())
                .Select(o => new SelectListItem(o.Title, o.Id.ToString()));
            
            return editVM;
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

            // Get invoices based on user role
            var invoices = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);
            var totalInvoices = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);
            var totalPages = (int)Math.Ceiling((double)totalInvoices / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<InvoiceDetailsVm>>(invoices);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(nameof(List), mappedModels);
        }

        [HttpGet]
        public override async Task<IActionResult> Details(int id)
        {
            try
            {
                var invoice = await _service.GetInvoiceWithDetailsAsync(id);
                if (invoice == null)
                {
                    return RedirectToAction("Error404", "Error");
                }

                var mappedModel = _mapper.Map<InvoiceDetailsVm>(invoice);
                return View(mappedModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Create()
        {
            return await base.Create();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public override async Task<IActionResult> Create(InvoiceEditVm editVM)
        {
            return await base.Create(editVM);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int? id)
        {
            return await base.Edit(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public override async Task<IActionResult> Edit(int id, InvoiceEditVm editVM)
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

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                await _service.MarkAsPaidAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsUnpaid(int id)
        {
            try
            {
                await _service.MarkAsUnpaidAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
} 