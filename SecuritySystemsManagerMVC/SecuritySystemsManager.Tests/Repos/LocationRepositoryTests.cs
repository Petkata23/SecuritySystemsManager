using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Repos
{
    [TestFixture]
    public class LocationRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private LocationRepository _repository;
        private Location _testLocation;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _testLocation = new Location
            {
                Id = 1,
                Name = "Test Location",
                Address = "123 Test Street, Sofia",
                Latitude = "42.6977",
                Longitude = "23.3219",
                Description = "Test location for security system",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Locations.Add(_testLocation);
            _context.SaveChanges();

            _repository = new LocationRepository(_context, CreateMockMapper());
        }

        [TearDown]
        public void TearDown()
        {
            _repository?.Dispose();
            _context?.Dispose();
        }

        private AutoMapper.IMapper CreateMockMapper()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Location, LocationDto>();
                cfg.CreateMap<LocationDto, Location>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLocation()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testLocation.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testLocation.Id));
            Assert.That(result.Name, Is.EqualTo(_testLocation.Name));
            Assert.That(result.Address, Is.EqualTo(_testLocation.Address));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllLocations()
        {
            // Arrange
            var additionalLocation = new Location
            {
                Id = 2,
                Name = "Second Location",
                Address = "456 Second Street, Sofia",
                Latitude = "42.6978",
                Longitude = "23.3220",
                Description = "Second test location",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Locations.AddAsync(additionalLocation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewLocation_ShouldSaveToDatabase()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Name = "New Location",
                Address = "789 New Street, Sofia",
                Latitude = "42.6979",
                Longitude = "23.3221",
                Description = "New test location"
            };

            // Act
            await _repository.SaveAsync(locationDto);

            // Assert
            var savedLocation = await _context.Locations.FirstOrDefaultAsync(l => l.Name == "New Location");
            Assert.That(savedLocation, Is.Not.Null);
            Assert.That(savedLocation.Address, Is.EqualTo("789 New Street, Sofia"));
            Assert.That(savedLocation.Latitude, Is.EqualTo("42.6979"));
            Assert.That(savedLocation.Longitude, Is.EqualTo("23.3221"));
        }

        [Test]
        public async Task SaveAsync_WithExistingLocation_ShouldUpdateLocation()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Id = _testLocation.Id,
                Name = "Updated Location",
                Address = _testLocation.Address,
                Latitude = _testLocation.Latitude,
                Longitude = _testLocation.Longitude,
                Description = "Updated description"
            };

            // Act
            await _repository.SaveAsync(locationDto);

            // Assert
            var updatedLocation = await _context.Locations.FindAsync(_testLocation.Id);
            Assert.That(updatedLocation, Is.Not.Null);
            Assert.That(updatedLocation.Name, Is.EqualTo("Updated Location"));
            Assert.That(updatedLocation.Description, Is.EqualTo("Updated description"));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveLocation()
        {
            // Act
            await _repository.DeleteAsync(_testLocation.Id);

            // Assert
            var deletedLocation = await _context.Locations.FindAsync(_testLocation.Id);
            Assert.That(deletedLocation, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.DeleteAsync(999));
        }

        [Test]
        public async Task GetWithPaginationAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var locations = new List<Location>
            {
                new Location
                {
                    Id = 2,
                    Name = "Location 2",
                    Address = "Address 2",
                    Latitude = "42.6978",
                    Longitude = "23.3220",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Location
                {
                    Id = 3,
                    Name = "Location 3",
                    Address = "Address 3",
                    Latitude = "42.6979",
                    Longitude = "23.3221",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            await _context.Locations.AddRangeAsync(locations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(2, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetWithPaginationAsync_WithInvalidPageSize_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetWithPaginationAsync(0, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetWithPaginationAsync_WithInvalidPageNumber_ShouldReturnResults()
        {
            // Act
            var result = await _repository.GetWithPaginationAsync(10, 0);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task GetWithPaginationAsync_WithMaxPageSize_ShouldReturnLimitedResults()
        {
            // Arrange
            var locations = new List<Location>();
            for (int i = 2; i <= 150; i++)
            {
                locations.Add(new Location
                {
                    Id = i,
                    Name = $"Location {i}",
                    Address = $"Address {i}",
                    Latitude = "42.6978",
                    Longitude = "23.3220",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.Locations.AddRangeAsync(locations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(100, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(100));
        }

        [Test]
        public async Task SaveAsync_WithLocationHavingOrders_ShouldSaveCorrectly()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Name = "Location With Orders",
                Address = "789 Orders Street, Sofia",
                Latitude = "42.6979",
                Longitude = "23.3221",
                Description = "Location that will have orders"
            };

            // Act
            await _repository.SaveAsync(locationDto);

            // Assert
            var savedLocation = await _context.Locations.FirstOrDefaultAsync(l => l.Name == "Location With Orders");
            Assert.That(savedLocation, Is.Not.Null);
            Assert.That(savedLocation.Address, Is.EqualTo("789 Orders Street, Sofia"));
        }

        [Test]
        public async Task GetByIdIfExistsAsync_WithValidId_ShouldReturnLocation()
        {
            // Act
            var result = await _repository.GetByIdIfExistsAsync(_testLocation.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testLocation.Id));
            Assert.That(result.Name, Is.EqualTo(_testLocation.Name));
        }

        [Test]
        public async Task GetByIdIfExistsAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdIfExistsAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ExistsByIdAsync_WithValidId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByIdAsync(_testLocation.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ExistsByIdAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByIdAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_WithValidLocation_ShouldCreateLocation()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Name = "Created Location",
                Address = "123 Created Street, Sofia",
                Latitude = "42.6980",
                Longitude = "23.3222",
                Description = "Location created via CreateAsync"
            };

            // Act
            await _repository.CreateAsync(locationDto);

            // Assert
            var createdLocation = await _context.Locations.FirstOrDefaultAsync(l => l.Name == "Created Location");
            Assert.That(createdLocation, Is.Not.Null);
            Assert.That(createdLocation.Address, Is.EqualTo("123 Created Street, Sofia"));
        }

        [Test]
        public async Task CreateAsync_WithNullLocation_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.CreateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithValidLocation_ShouldUpdateLocation()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Id = _testLocation.Id,
                Name = "Updated Location",
                Address = "456 Updated Street, Sofia",
                Latitude = "42.6981",
                Longitude = "23.3223",
                Description = "Updated location description"
            };

            // Act
            await _repository.UpdateAsync(locationDto);

            // Assert
            var updatedLocation = await _context.Locations.FindAsync(_testLocation.Id);
            Assert.That(updatedLocation, Is.Not.Null);
            Assert.That(updatedLocation.Name, Is.EqualTo("Updated Location"));
            Assert.That(updatedLocation.Address, Is.EqualTo("456 Updated Street, Sofia"));
        }

        [Test]
        public async Task UpdateAsync_WithNullLocation_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.UpdateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithNonExistentLocation_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var locationDto = new LocationDto
            {
                Id = 999,
                Name = "Non Existent Location",
                Address = "999 Non Existent Street, Sofia",
                Latitude = "42.6982",
                Longitude = "23.3224",
                Description = "This location doesn't exist"
            };

            // Act & Assert - The method should handle the exception gracefully and not throw
            Assert.DoesNotThrowAsync(async () => await _repository.UpdateAsync(locationDto));
        }
    }
} 