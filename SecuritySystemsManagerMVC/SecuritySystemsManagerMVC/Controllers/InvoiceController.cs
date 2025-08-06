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
            try
            {
                // Simple validation - can stay in controller
                if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
                {
                    return BadRequest(SecuritySystemsManager.Shared.Constants.InvalidPagination);
                }

                var (invoices, totalCount) = await _service.GetFilteredInvoicesAsync(null, null, User, pageSize, pageNumber);
                
                // Simple math - can stay in controller
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var mappedModels = _mapper.Map<IEnumerable<InvoiceDetailsVm>>(invoices);

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = pageNumber;
                // Add flag to indicate if there are any invoices before filtering
                ViewBag.HasInvoicesBeforeFilter = totalCount > 0;

                return View(mappedModels);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterInvoices(string searchTerm = "", string paymentStatus = "", int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            // Simple validation - can stay in controller
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(SecuritySystemsManager.Shared.Constants.InvalidPagination);
            }

            // Get filtered invoices with pagination from service
            var (invoices, totalCount) = await _service.GetFilteredInvoicesAsync(searchTerm, paymentStatus, User, pageSize, pageNumber);
            
            // Simple math - can stay in controller
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<InvoiceDetailsVm>>(invoices);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchTermFilter = searchTerm;
            ViewBag.PaymentStatusFilter = paymentStatus;

            return View(nameof(List), mappedModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesPartial(string searchTerm = "", string paymentStatus = "", int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            // Simple validation - can stay in controller
            if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
            {
                return BadRequest(SecuritySystemsManager.Shared.Constants.InvalidPagination);
            }

            // Get filtered invoices with pagination from service
            var (invoices, totalCount) = await _service.GetFilteredInvoicesAsync(searchTerm, paymentStatus, User, pageSize, pageNumber);
            
            // Simple math - can stay in controller
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var mappedModels = _mapper.Map<IEnumerable<InvoiceDetailsVm>>(invoices);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchTermFilter = searchTerm;
            ViewBag.PaymentStatusFilter = paymentStatus;
            
            // Check if there are invoices before filtering by getting total count without filters
            var (allInvoices, totalInvoicesBeforeFilter) = await _service.GetFilteredInvoicesAsync(null, null, User, 1, 1);
            ViewBag.HasInvoicesBeforeFilter = totalInvoicesBeforeFilter > 0;

            return PartialView("_InvoicesTable", mappedModels);
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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