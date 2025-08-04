using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Repos
{
    [TestFixture]
    public class SecuritySystemOrderRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private SecuritySystemOrderRepository _repository;
        private User _client;
        private User _technician;
        private Location _location;
        private SecuritySystemOrder _testOrder;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _client = new User
            {
                Id = 1,
                UserName = "client@test.com",
                Email = "client@test.com",
                FirstName = "Test",
                LastName = "Client",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _technician = new User
            {
                Id = 2,
                UserName = "technician@test.com",
                Email = "technician@test.com",
                FirstName = "Test",
                LastName = "Technician",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _location = new Location
            {
                Id = 1,
                Name = "Test Location",
                Address = "123 Test Street",
                Latitude = "42.6977",
                Longitude = "23.3219",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _testOrder = new SecuritySystemOrder
            {
                Id = 1,
                Title = "Test Security System",
                Description = "Test security system installation",
                PhoneNumber = "+359888123456",
                ClientId = _client.Id,
                Client = _client,
                LocationId = _location.Id,
                Location = _location,
                Status = OrderStatus.Pending,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.AddRange(_client, _technician);
            _context.Locations.Add(_location);
            _context.Orders.Add(_testOrder);
            _context.SaveChanges();

            _repository = new SecuritySystemOrderRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>();
                cfg.CreateMap<SecuritySystemOrderDto, SecuritySystemOrder>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<Location, LocationDto>();
                cfg.CreateMap<LocationDto, Location>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetOrderWithTechniciansAsync_WithValidOrderId_ShouldReturnOrder()
        {
            // Act
            var result = await _repository.GetOrderWithTechniciansAsync(_testOrder.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testOrder.Id));
            Assert.That(result.Title, Is.EqualTo(_testOrder.Title));
        }

        [Test]
        public async Task GetOrderWithTechniciansAsync_WithInvalidOrderId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetOrderWithTechniciansAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddTechnicianToOrderAsync_WithValidIds_ShouldAddTechnician()
        {
            // Act
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Assert
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == _testOrder.Id);
            
            Assert.That(order.Technicians, Is.Not.Null);
            Assert.That(order.Technicians.Count, Is.EqualTo(1));
            Assert.That(order.Technicians.First().Id, Is.EqualTo(_technician.Id));
        }

        [Test]
        public async Task AddTechnicianToOrderAsync_WithInvalidOrderId_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _repository.AddTechnicianToOrderAsync(999, _technician.Id));
            
            Assert.That(exception.Message, Is.EqualTo("Order not found."));
        }

        [Test]
        public async Task AddTechnicianToOrderAsync_WithInvalidTechnicianId_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _repository.AddTechnicianToOrderAsync(_testOrder.Id, 999));
            
            Assert.That(exception.Message, Is.EqualTo("Technician not found."));
        }

        [Test]
        public async Task AddTechnicianToOrderAsync_WithAlreadyAssignedTechnician_ShouldNotAddAgain()
        {
            // Arrange - Add technician first time
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Act - Try to add same technician again
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Assert
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == _testOrder.Id);
            
            Assert.That(order.Technicians.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task RemoveTechnicianFromOrderAsync_WithValidIds_ShouldRemoveTechnician()
        {
            // Arrange - Add technician first
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Act
            await _repository.RemoveTechnicianFromOrderAsync(_testOrder.Id, _technician.Id);

            // Assert
            var order = await _context.Orders
                .Include(o => o.Technicians)
                .FirstOrDefaultAsync(o => o.Id == _testOrder.Id);
            
            Assert.That(order.Technicians.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task RemoveTechnicianFromOrderAsync_WithInvalidOrderId_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _repository.RemoveTechnicianFromOrderAsync(999, _technician.Id));
            
            Assert.That(exception.Message, Is.EqualTo("Order not found."));
        }

        [Test]
        public async Task RemoveTechnicianFromOrderAsync_WithUnassignedTechnician_ShouldNotThrowException()
        {
            // Act & Assert - Should not throw exception
            Assert.DoesNotThrowAsync(async () => 
                await _repository.RemoveTechnicianFromOrderAsync(_testOrder.Id, _technician.Id));
        }

        [Test]
        public async Task GetOrdersByClientIdAsync_ShouldReturnClientOrders()
        {
            // Arrange
            var secondOrder = new SecuritySystemOrder
            {
                Id = 2,
                Title = "Second Order",
                Description = "Second test order",
                PhoneNumber = "+359888123457",
                ClientId = _client.Id,
                Status = OrderStatus.InProgress,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Orders.AddAsync(secondOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOrdersByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetOrdersByClientIdAsync_WithPagination_ShouldReturnLimitedResults()
        {
            // Arrange
            var orders = new List<SecuritySystemOrder>();
            for (int i = 2; i <= 15; i++)
            {
                orders.Add(new SecuritySystemOrder
                {
                    Id = i,
                    Title = $"Order {i}",
                    Description = $"Description {i}",
                    PhoneNumber = $"+35988812345{i}",
                    ClientId = _client.Id,
                    Status = OrderStatus.Pending,
                    RequestedDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOrdersByClientIdAsync(_client.Id, 5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task GetOrdersByTechnicianIdAsync_ShouldReturnTechnicianOrders()
        {
            // Arrange - Add technician to order
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Act
            var result = await _repository.GetOrdersByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrdersByTechnicianIdAsync_WithNoAssignedOrders_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetOrdersByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetOrdersCountByClientIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var secondOrder = new SecuritySystemOrder
            {
                Id = 2,
                Title = "Second Order",
                Description = "Second test order",
                PhoneNumber = "+359888123457",
                ClientId = _client.Id,
                Status = OrderStatus.InProgress,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Orders.AddAsync(secondOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOrdersCountByClientIdAsync(_client.Id);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetOrdersCountByTechnicianIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange - Add technician to order
            await _repository.AddTechnicianToOrderAsync(_testOrder.Id, _technician.Id);

            // Act
            var result = await _repository.GetOrdersCountByTechnicianIdAsync(_technician.Id);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrderWithAllDetailsAsync_WithValidOrderId_ShouldReturnOrderWithDetails()
        {
            // Act
            var result = await _repository.GetOrderWithAllDetailsAsync(_testOrder.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testOrder.Id));
            Assert.That(result.Title, Is.EqualTo(_testOrder.Title));
        }

        [Test]
        public async Task GetOrderWithAllDetailsAsync_WithInvalidOrderId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetOrderWithAllDetailsAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnOrder()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testOrder.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testOrder.Id));
            Assert.That(result.Title, Is.EqualTo(_testOrder.Title));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllOrders()
        {
            // Arrange
            var secondOrder = new SecuritySystemOrder
            {
                Id = 2,
                Title = "Second Order",
                Description = "Second test order",
                PhoneNumber = "+359888123457",
                ClientId = _client.Id,
                Status = OrderStatus.InProgress,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Orders.AddAsync(secondOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewOrder_ShouldSaveToDatabase()
        {
            // Arrange
            var orderDto = new SecuritySystemOrderDto
            {
                Title = "New Order",
                Description = "New test order",
                PhoneNumber = "+359888123458",
                ClientId = _client.Id,
                Status = OrderStatus.Pending
            };

            // Act
            await _repository.SaveAsync(orderDto);

            // Assert
            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Title == "New Order");
            Assert.That(savedOrder, Is.Not.Null);
            Assert.That(savedOrder.Description, Is.EqualTo("New test order"));
            Assert.That(savedOrder.ClientId, Is.EqualTo(_client.Id));
        }

        [Test]
        public async Task SaveAsync_WithExistingOrder_ShouldUpdateOrder()
        {
            // Arrange
            var orderDto = new SecuritySystemOrderDto
            {
                Id = _testOrder.Id,
                Title = "Updated Order",
                Description = _testOrder.Description,
                PhoneNumber = _testOrder.PhoneNumber,
                ClientId = _testOrder.ClientId,
                Status = OrderStatus.Completed
            };

            // Act
            await _repository.SaveAsync(orderDto);

            // Assert
            var updatedOrder = await _context.Orders.FindAsync(_testOrder.Id);
            Assert.That(updatedOrder, Is.Not.Null);
            Assert.That(updatedOrder.Title, Is.EqualTo("Updated Order"));
            Assert.That(updatedOrder.Status, Is.EqualTo(OrderStatus.Completed));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveOrder()
        {
            // Act
            await _repository.DeleteAsync(_testOrder.Id);

            // Assert
            var deletedOrder = await _context.Orders.FindAsync(_testOrder.Id);
            Assert.That(deletedOrder, Is.Null);
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
            var orders = new List<SecuritySystemOrder>();
            for (int i = 2; i <= 10; i++)
            {
                orders.Add(new SecuritySystemOrder
                {
                    Id = i,
                    Title = $"Order {i}",
                    Description = $"Description {i}",
                    PhoneNumber = $"+35988812345{i}",
                    ClientId = _client.Id,
                    Status = OrderStatus.Pending,
                    RequestedDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
        }
    }
} 