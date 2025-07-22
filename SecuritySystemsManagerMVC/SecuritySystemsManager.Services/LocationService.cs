using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class LocationService : BaseCrudService<LocationDto, ILocationRepository>, ILocationService
    {
        private readonly ISecuritySystemOrderRepository _orderRepository;

        public LocationService(ILocationRepository repository, ISecuritySystemOrderRepository orderRepository) : base(repository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<object>> GetLocationsWithOrdersAsync()
        {
            // Get all locations
            var locations = await GetAllAsync();
            
            // Get all orders
            var orders = await _orderRepository.GetAllAsync();
            
            // Create a list to store location data with orders
            var locationData = new List<object>();
            
            foreach (var location in locations)
            {
                // Get orders for this location
                var locationOrders = orders.Where(o => o.LocationId == location.Id).ToList();
                
                // Create a simplified object for the map
                var locationInfo = new
                {
                    id = location.Id,
                    name = location.Name,
                    address = location.Address,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    orders = locationOrders.Select(o => new
                    {
                        id = o.Id,
                        title = o.Title,
                        status = o.Status.ToString(),
                        requestedDate = o.RequestedDate
                    }).ToList()
                };
                
                locationData.Add(locationInfo);
            }
            
            return locationData;
        }

        public async Task<IEnumerable<LocationDto>> GetLocationsForUserAsync(int userId, int pageSize = 10, int pageNumber = 1)
        {
            // Get locations that are associated with orders for this specific user
            var userLocations = await _orderRepository.GetAllAsync();
            var userLocationIds = userLocations
                .Where(o => o.ClientId == userId && o.LocationId.HasValue)
                .Select(o => o.LocationId.Value)
                .Distinct()
                .ToList();

            // Get the actual location data for these IDs
            var locations = await GetAllAsync();
            var filteredLocations = locations
                .Where(l => userLocationIds.Contains(l.Id))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return filteredLocations;
        }

        public async Task<IEnumerable<object>> GetLocationsWithOrdersForUserAsync(int userId)
        {
            // Get locations that are associated with orders for this specific user
            var userOrders = await _orderRepository.GetAllAsync();
            var userLocationIds = userOrders
                .Where(o => o.ClientId == userId && o.LocationId.HasValue)
                .Select(o => o.LocationId.Value)
                .Distinct()
                .ToList();

            // Get the actual location data for these IDs
            var locations = await GetAllAsync();
            var userLocations = locations.Where(l => userLocationIds.Contains(l.Id)).ToList();
            
            // Create a list to store location data with orders
            var locationData = new List<object>();
            
            foreach (var location in userLocations)
            {
                // Get orders for this location that belong to this user
                var locationOrders = userOrders.Where(o => o.LocationId == location.Id && o.ClientId == userId).ToList();
                
                // Create a simplified object for the map
                var locationInfo = new
                {
                    id = location.Id,
                    name = location.Name,
                    address = location.Address,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    orders = locationOrders.Select(o => new
                    {
                        id = o.Id,
                        title = o.Title,
                        status = o.Status.ToString(),
                        requestedDate = o.RequestedDate
                    }).ToList()
                };
                
                locationData.Add(locationInfo);
            }
            
            return locationData;
        }

        public async Task<IEnumerable<object>> GetLocationsWithOrdersForCurrentUserAsync(int userId, bool isAdminOrManager)
        {
            if (isAdminOrManager)
            {
                return await GetLocationsWithOrdersAsync();
            }
            
            return await GetLocationsWithOrdersForUserAsync(userId);
        }
    }
} 