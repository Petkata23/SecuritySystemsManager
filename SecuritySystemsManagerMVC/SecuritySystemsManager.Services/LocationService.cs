using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

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
            // Business logic: Get locations with orders for map display
            var locations = await _repository.GetLocationsWithOrdersAsync();
            var orders = await _orderRepository.GetAllAsync();
            
            var locationData = new List<object>();
            
            foreach (var location in locations)
            {
                var locationOrders = orders.Where(o => o.LocationId == location.Id).ToList();
                
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
            // Business logic: Get user locations with pagination
            var allUserLocations = await _repository.GetLocationsForUserAsync(userId);
            return allUserLocations
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<IEnumerable<object>> GetLocationsWithOrdersForUserAsync(int userId)
        {
            // Business logic: Get user locations with orders for map display
            var userLocations = await _repository.GetLocationsForUserAsync(userId);
            var userOrders = await _orderRepository.GetAllAsync();
            
            var locationData = new List<object>();
            
            foreach (var location in userLocations)
            {
                var locationOrders = userOrders.Where(o => o.LocationId == location.Id && o.ClientId == userId).ToList();
                
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
            else
            {
                return await GetLocationsWithOrdersForUserAsync(userId);
            }
        }

        public async Task<(bool success, int? locationId, string locationName, string message)> CreateLocationAjaxAsync(LocationDto locationDto)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(locationDto.Name))
                {
                    return (false, null, null, "Location name is required");
                }

                if (string.IsNullOrWhiteSpace(locationDto.Address))
                {
                    return (false, null, null, "Address is required");
                }

                // Validate coordinates
                if (string.IsNullOrWhiteSpace(locationDto.Latitude) || string.IsNullOrWhiteSpace(locationDto.Longitude))
                {
                    return (false, null, null, "Please select a location on the map");
                }

                // Set timestamps
                locationDto.CreatedAt = DateTime.Now;
                locationDto.UpdatedAt = DateTime.Now;

                // Save the location
                await SaveAsync(locationDto);

                // Get the saved location to return the ID
                var savedLocation = await _repository.GetAllAsync();
                var createdLocation = savedLocation
                    .Where(l => l.Name == locationDto.Name && l.Address == locationDto.Address)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefault();

                if (createdLocation != null)
                {
                    return (true, createdLocation.Id, createdLocation.Name, "Location created successfully");
                }
                else
                {
                    return (false, null, null, "Failed to retrieve created location");
                }
            }
            catch (Exception ex)
            {
                return (false, null, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IEnumerable<LocationDto>> GetLocationsForTechnicianAsync(int technicianId, int pageSize = 10, int pageNumber = 1)
        {
            // Business logic: Get technician locations with pagination
            var allTechnicianLocations = await _repository.GetLocationsForTechnicianAsync(technicianId);
            return allTechnicianLocations
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<IEnumerable<object>> GetLocationsWithOrdersForTechnicianAsync(int technicianId)
        {
            // Business logic: Get technician locations with orders for map display
            var technicianLocations = await _repository.GetLocationsForTechnicianAsync(technicianId);
            var technicianOrders = await _orderRepository.GetAllAsync();
            var filteredTechnicianOrders = technicianOrders
                .Where(o => o.Technicians != null && o.Technicians.Any(t => t.Id == technicianId))
                .ToList();
            
            var locationData = new List<object>();
            
            foreach (var location in technicianLocations)
            {
                var locationOrders = filteredTechnicianOrders.Where(o => o.LocationId == location.Id).ToList();
                
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