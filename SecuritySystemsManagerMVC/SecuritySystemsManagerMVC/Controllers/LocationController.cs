using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class LocationController : BaseCrudController<LocationDto, ILocationRepository, ILocationService, LocationEditVm, LocationDetailsVm>
    {
        public LocationController(IMapper mapper, ILocationService service)
            : base(service, mapper)
        {
        }


        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _service.GetAllAsync();
            return Json(locations);
        }

    }
} 