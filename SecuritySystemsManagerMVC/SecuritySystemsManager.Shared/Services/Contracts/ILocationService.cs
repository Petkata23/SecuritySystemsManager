using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface ILocationService : IBaseCrudService<LocationDto, ILocationRepository>
    {
        Task<IEnumerable<object>> GetLocationsWithOrdersAsync();
        Task<IEnumerable<object>> GetLocationsWithOrdersForUserAsync(int userId);
        Task<IEnumerable<LocationDto>> GetLocationsForUserAsync(int userId, int pageSize = 10, int pageNumber = 1);
        Task<IEnumerable<object>> GetLocationsWithOrdersForCurrentUserAsync(int userId, bool isAdminOrManager);
        Task<(bool success, int? locationId, string locationName, string message)> CreateLocationAjaxAsync(LocationDto locationDto);
        
        // Methods for technicians to see their assigned locations
        Task<IEnumerable<LocationDto>> GetLocationsForTechnicianAsync(int technicianId, int pageSize = 10, int pageNumber = 1);
        Task<IEnumerable<object>> GetLocationsWithOrdersForTechnicianAsync(int technicianId);
    }
} 