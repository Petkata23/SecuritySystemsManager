using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System;
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
        public override async Task<IActionResult> Details(int id)
        {
            var invoice = await ((IInvoiceService)_service).GetInvoiceWithDetailsAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var mappedModel = _mapper.Map<InvoiceDetailsVm>(invoice);
            return View(mappedModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                await ((IInvoiceService)_service).MarkAsPaidAsync(id);
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
                await ((IInvoiceService)_service).MarkAsUnpaidAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
} 