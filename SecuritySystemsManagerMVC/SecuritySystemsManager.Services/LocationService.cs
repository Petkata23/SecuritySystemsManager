using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
} 