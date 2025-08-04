using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class LocationServiceTests : BaseServiceTests<LocationDto, ILocationRepository, LocationService>
    {
        private Mock<ISecuritySystemOrderRepository> _mockOrderRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockOrderRepository = new Mock<ISecuritySystemOrderRepository>();
            _service = new LocationService(_mockRepository.Object, _mockOrderRepository.Object);
        }

        protected override LocationService CreateService(ILocationRepository repository)
        {
            return new LocationService(repository, _mockOrderRepository?.Object ?? new Mock<ISecuritySystemOrderRepository>().Object);
        }

        protected override LocationDto CreateTestModel(int id = 1)
        {
            return new LocationDto
            {
                Id = id,
                Name = $"Location {id}",
                Address = $"Address {id}",
                Latitude = $"42.123{id}",
                Longitude = $"23.456{id}",
                Description = $"Description for location {id}"
            };
        }

        [Test]
        public async Task GetLocationsWithOrdersAsync_ShouldReturnLocationsWithOrders()
        {
            // Arrange
            var locations = CreateTestModels();
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, LocationId = 1, Title = "Order 1", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Pending, RequestedDate = System.DateTime.UtcNow },
                new SecuritySystemOrderDto { Id = 2, LocationId = 1, Title = "Order 2", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.InProgress, RequestedDate = System.DateTime.UtcNow }
            };
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

            // Act
            var result = await _service.GetLocationsWithOrdersAsync();

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersAsync_WithNoLocations_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyLocations = new List<LocationDto>();
            var orders = new List<SecuritySystemOrderDto>();
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyLocations);
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

            // Act
            var result = await _service.GetLocationsWithOrdersAsync();

            // Assert
            Assert.That(result, Is.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersAsync_WithNoOrders_ShouldReturnLocationsWithoutOrders()
        {
            // Arrange
            var locations = CreateTestModels();
            var emptyOrders = new List<SecuritySystemOrderDto>();
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyOrders);

            // Act
            var result = await _service.GetLocationsWithOrdersAsync();

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsForUserAsync_WithValidUserId_ShouldReturnUserLocations()
        {
            // Arrange
            int userId = 1;
            int pageSize = 10;
            int pageNumber = 1;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = userId, LocationId = 1 },
                new SecuritySystemOrderDto { Id = 2, ClientId = userId, LocationId = 2 }
            };
            var locations = CreateTestModels();
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsForUserAsync(userId, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsForUserAsync_WithNoUserOrders_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            int pageSize = 10;
            int pageNumber = 1;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = 2, LocationId = 1 }, // Different user
                new SecuritySystemOrderDto { Id = 2, ClientId = 3, LocationId = 2 }  // Different user
            };
            var locations = CreateTestModels();
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsForUserAsync(userId, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsForUserAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            int userId = 1;
            int pageSize = 2;
            int pageNumber = 2;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = userId, LocationId = 1 },
                new SecuritySystemOrderDto { Id = 2, ClientId = userId, LocationId = 2 },
                new SecuritySystemOrderDto { Id = 3, ClientId = userId, LocationId = 3 },
                new SecuritySystemOrderDto { Id = 4, ClientId = userId, LocationId = 4 }
            };
            var locations = CreateTestModels();
            locations.Add(CreateTestModel(4));
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsForUserAsync(userId, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersForUserAsync_WithValidUserId_ShouldReturnUserLocationsWithOrders()
        {
            // Arrange
            int userId = 1;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = userId, LocationId = 1, Title = "Order 1", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Pending, RequestedDate = System.DateTime.UtcNow }
            };
            var locations = CreateTestModels();
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsWithOrdersForUserAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersForUserAsync_WithNoUserOrders_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = 2, LocationId = 1, Title = "Order 1", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Pending, RequestedDate = System.DateTime.UtcNow }
            };
            var locations = CreateTestModels();
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsWithOrdersForUserAsync(userId);

            // Assert
            Assert.That(result, Is.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersForCurrentUserAsync_WithAdminUser_ShouldReturnAllLocations()
        {
            // Arrange
            int userId = 1;
            bool isAdminOrManager = true;
            var locations = CreateTestModels();
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, LocationId = 1, Title = "Order 1", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Pending, RequestedDate = System.DateTime.UtcNow }
            };
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

            // Act
            var result = await _service.GetLocationsWithOrdersForCurrentUserAsync(userId, isAdminOrManager);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLocationsWithOrdersForCurrentUserAsync_WithRegularUser_ShouldReturnUserLocations()
        {
            // Arrange
            int userId = 1;
            bool isAdminOrManager = false;
            var orders = new List<SecuritySystemOrderDto>
            {
                new SecuritySystemOrderDto { Id = 1, ClientId = userId, LocationId = 1, Title = "Order 1", Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Pending, RequestedDate = System.DateTime.UtcNow }
            };
            var locations = CreateTestModels();
            
            _mockOrderRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetLocationsWithOrdersForCurrentUserAsync(userId, isAdminOrManager);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var locationDto = CreateTestModel();
            var savedLocations = new List<LocationDto> { locationDto };
            
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<LocationDto>())).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(savedLocations);

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.True);
            Assert.That(result.locationId, Is.EqualTo(locationDto.Id));
            Assert.That(result.locationName, Is.EqualTo(locationDto.Name));
            Assert.That(result.message, Is.EqualTo("Location created successfully"));
            _mockRepository.Verify(r => r.SaveAsync(locationDto), Times.Once);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithEmptyName_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Name = "";

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Location name is required"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithNullName_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Name = null;

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Location name is required"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithEmptyAddress_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Address = "";

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Address is required"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithNullAddress_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Address = null;

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Address is required"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithEmptyLatitude_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Latitude = "";

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Please select a location on the map"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithNullLatitude_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Latitude = null;

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Please select a location on the map"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithEmptyLongitude_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Longitude = "";

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Please select a location on the map"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WithNullLongitude_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            locationDto.Longitude = null;

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Please select a location on the map"));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LocationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WhenSaveThrowsException_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<LocationDto>())).ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Does.Contain("An error occurred"));
            _mockRepository.Verify(r => r.SaveAsync(locationDto), Times.Once);
        }

        [Test]
        public async Task CreateLocationAjaxAsync_WhenLocationNotFoundAfterSave_ShouldReturnFailure()
        {
            // Arrange
            var locationDto = CreateTestModel();
            var emptyLocations = new List<LocationDto>();
            
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<LocationDto>())).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyLocations);

            // Act
            var result = await _service.CreateLocationAjaxAsync(locationDto);

            // Assert
            Assert.That(result.success, Is.False);
            Assert.That(result.message, Is.EqualTo("Failed to retrieve created location"));
            _mockRepository.Verify(r => r.SaveAsync(locationDto), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }
    }
} 