using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

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
    }
} 