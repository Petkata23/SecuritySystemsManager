using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class LocationRepository : BaseRepository<Location, LocationDto>, ILocationRepository
    {
        public LocationRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<IEnumerable<LocationDto>> GetLocationsForUserAsync(int userId)
        {
            // Data access: Get locations associated with user's orders
            var userLocationIds = await _context.Orders
                .Where(o => o.ClientId == userId && o.LocationId.HasValue)
                .Select(o => o.LocationId.Value)
                .Distinct()
                .ToListAsync();

            var locations = await _dbSet
                .Where(l => userLocationIds.Contains(l.Id))
                .ToListAsync();

            return locations.Select(MapToModel);
        }

        public async Task<IEnumerable<LocationDto>> GetLocationsForTechnicianAsync(int technicianId)
        {
            // Data access: Get locations where technician is assigned
            var technicianLocationIds = await _context.Orders
                .Where(o => o.Technicians.Any(t => t.Id == technicianId) && o.LocationId.HasValue)
                .Select(o => o.LocationId.Value)
                .Distinct()
                .ToListAsync();

            var locations = await _dbSet
                .Where(l => technicianLocationIds.Contains(l.Id))
                .ToListAsync();

            return locations.Select(MapToModel);
        }

        public async Task<IEnumerable<LocationDto>> GetLocationsWithOrdersAsync()
        {
            // Data access: Get all locations that have orders
            var locationIdsWithOrders = await _context.Orders
                .Where(o => o.LocationId.HasValue)
                .Select(o => o.LocationId.Value)
                .Distinct()
                .ToListAsync();

            var locations = await _dbSet
                .Where(l => locationIdsWithOrders.Contains(l.Id))
                .ToListAsync();

            return locations.Select(MapToModel);
        }
    }
} 