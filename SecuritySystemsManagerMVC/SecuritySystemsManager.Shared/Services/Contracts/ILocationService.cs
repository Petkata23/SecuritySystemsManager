using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface ILocationService : IBaseCrudService<LocationDto, ILocationRepository>
    {
        Task<IEnumerable<object>> GetLocationsWithOrdersAsync();
    }
} 