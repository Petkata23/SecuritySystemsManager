using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class LocationController : BaseCrudController<LocationDto, ILocationRepository, ILocationService, LocationEditVm, LocationDetailsVm>
    {
        public LocationController(ILocationService service, IMapper mapper) : base(service, mapper)
        {
        }

        [HttpGet]
        public override async Task<IActionResult> List(int pageSize = 10, int pageNumber = 1)
        {
            ViewBag.Title = "Locations";
            return await base.List(pageSize, pageNumber);
        }

        [HttpGet]
        public override async Task<IActionResult> Create()
        {
            ViewBag.Title = "Create Location";
            return await base.Create();
        }

        [HttpGet]
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
        public override async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Title = "Delete Location";
            return await base.Delete(id);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllLocations()
        {
            var locationsWithOrders = await _service.GetLocationsWithOrdersAsync();
            return Json(locationsWithOrders);
        }
    }
} 