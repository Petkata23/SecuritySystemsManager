using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class LocationService : BaseCrudService<LocationDto, ILocationRepository>, ILocationService
    {
        public LocationService(ILocationRepository repository) : base(repository)
        {
        }
    }
} 