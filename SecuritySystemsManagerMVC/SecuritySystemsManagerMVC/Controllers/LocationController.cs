using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class LocationController : BaseCrudController<LocationDto, ILocationRepository, ILocationService, LocationEditVm, LocationDetailsVm>
    {
        public LocationController(IMapper mapper, ILocationService service)
            : base(service, mapper)
        {
        }
    }
} 