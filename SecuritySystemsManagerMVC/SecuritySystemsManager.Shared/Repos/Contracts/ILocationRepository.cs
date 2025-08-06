using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface ILocationRepository : IBaseRepository<LocationDto>
    {
        // Data access methods for location queries
        Task<IEnumerable<LocationDto>> GetLocationsForUserAsync(int userId);
        Task<IEnumerable<LocationDto>> GetLocationsForTechnicianAsync(int technicianId);
        Task<IEnumerable<LocationDto>> GetLocationsWithOrdersAsync();
    }
} 