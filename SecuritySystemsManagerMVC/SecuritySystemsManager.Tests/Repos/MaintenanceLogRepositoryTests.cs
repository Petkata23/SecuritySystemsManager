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
    public class MaintenanceLogRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private MaintenanceLogRepository _repository;
        private User _client;
        private User _technician;
        private Location _location;
        private SecuritySystemOrder _order;
        private InstalledDevice _installedDevice;
        private MaintenanceLog _testLog;

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

            _order = new SecuritySystemOrder
            {
                Id = 1,
                Title = "Test Security System",
                Description = "Test security system installation",
                PhoneNumber = "+359888123456",
                ClientId = _client.Id,
                Client = _client,
                LocationId = _location.Id,
                Location = _location,
                Status = OrderStatus.Completed,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _installedDevice = new InstalledDevice
            {
                Id = 1,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                DeviceType = DeviceType.Camera,
                Brand = "Test Brand",
                Model = "Test Model",
                Quantity = 1,
                DateInstalled = DateTime.Now,
                InstalledById = _technician.Id,
                InstalledBy = _technician,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _testLog = new MaintenanceLog
            {
                Id = 1,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                TechnicianId = _technician.Id,
                Technician = _technician,
                Date = DateTime.Now,
                Description = "Test maintenance log",
                Resolved = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.AddRange(_client, _technician);
            _context.Locations.Add(_location);
            _context.Orders.Add(_order);
            _context.InstalledDevices.Add(_installedDevice);
            _context.MaintenanceLogs.Add(_testLog);
            _context.SaveChanges();

            _repository = new MaintenanceLogRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<MaintenanceLog, MaintenanceLogDto>();
                cfg.CreateMap<MaintenanceLogDto, MaintenanceLog>();
                cfg.CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>();
                cfg.CreateMap<SecuritySystemOrderDto, SecuritySystemOrder>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<InstalledDevice, InstalledDeviceDto>();
                cfg.CreateMap<InstalledDeviceDto, InstalledDevice>();
                cfg.CreateMap<MaintenanceDevice, MaintenanceDeviceDto>();
                cfg.CreateMap<MaintenanceDeviceDto, MaintenanceDevice>();
                cfg.CreateMap<Location, LocationDto>();
                cfg.CreateMap<LocationDto, Location>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLogWithDetails()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testLog.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testLog.Id));
            Assert.That(result.Description, Is.EqualTo(_testLog.Description));
            Assert.That(result.Resolved, Is.EqualTo(_testLog.Resolved));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetLogsByClientIdAsync_ShouldReturnClientLogs()
        {
            // Arrange
            var secondLog = new MaintenanceLog
            {
                Id = 2,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                TechnicianId = _technician.Id,
                Technician = _technician,
                Date = DateTime.Now,
                Description = "Second maintenance log",
                Resolved = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.MaintenanceLogs.AddAsync(secondLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetLogsByClientIdAsync_WithPagination_ShouldReturnLimitedResults()
        {
            // Arrange
            var logs = new List<MaintenanceLog>();
            for (int i = 2; i <= 15; i++)
            {
                logs.Add(new MaintenanceLog
                {
                    Id = i,
                    SecuritySystemOrderId = _order.Id,
                    SecuritySystemOrder = _order,
                    TechnicianId = _technician.Id,
                    Technician = _technician,
                    Date = DateTime.Now,
                    Description = $"Maintenance log {i}",
                    Resolved = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.MaintenanceLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsByClientIdAsync(_client.Id, 5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task GetLogsByTechnicianIdAsync_ShouldReturnTechnicianLogs()
        {
            // Act
            var result = await _repository.GetLogsByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetLogsByTechnicianIdAsync_WithNoLogs_ShouldReturnEmpty()
        {
            // Arrange - Remove all logs
            _context.MaintenanceLogs.RemoveRange(_context.MaintenanceLogs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetLogsCountByClientIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var secondLog = new MaintenanceLog
            {
                Id = 2,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                TechnicianId = _technician.Id,
                Technician = _technician,
                Date = DateTime.Now,
                Description = "Second maintenance log",
                Resolved = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.MaintenanceLogs.AddAsync(secondLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsCountByClientIdAsync(_client.Id);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetLogsCountByTechnicianIdAsync_ShouldReturnCorrectCount()
        {
            // Act
            var result = await _repository.GetLogsCountByTechnicianIdAsync(_technician.Id);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllLogs()
        {
            // Arrange
            var secondLog = new MaintenanceLog
            {
                Id = 2,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                TechnicianId = _technician.Id,
                Technician = _technician,
                Date = DateTime.Now,
                Description = "Second maintenance log",
                Resolved = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.MaintenanceLogs.AddAsync(secondLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewLog_ShouldSaveToDatabase()
        {
            // Arrange
            var logDto = new MaintenanceLogDto
            {
                SecuritySystemOrderId = _order.Id,
                TechnicianId = _technician.Id,
                Date = DateTime.Now,
                Description = "New maintenance log",
                Resolved = false
            };

            // Act
            await _repository.SaveAsync(logDto);

            // Assert
            var savedLog = await _context.MaintenanceLogs.FirstOrDefaultAsync(l => l.Description == "New maintenance log");
            Assert.That(savedLog, Is.Not.Null);
            Assert.That(savedLog.SecuritySystemOrderId, Is.EqualTo(_order.Id));
            Assert.That(savedLog.TechnicianId, Is.EqualTo(_technician.Id));
        }

        [Test]
        public async Task SaveAsync_WithExistingLog_ShouldUpdateLog()
        {
            // Arrange
            var logDto = new MaintenanceLogDto
            {
                Id = _testLog.Id,
                SecuritySystemOrderId = _testLog.SecuritySystemOrderId,
                TechnicianId = _testLog.TechnicianId,
                Date = _testLog.Date,
                Description = "Updated maintenance log",
                Resolved = true
            };

            // Act
            await _repository.SaveAsync(logDto);

            // Assert
            var updatedLog = await _context.MaintenanceLogs.FindAsync(_testLog.Id);
            Assert.That(updatedLog, Is.Not.Null);
            Assert.That(updatedLog.Description, Is.EqualTo("Updated maintenance log"));
            Assert.That(updatedLog.Resolved, Is.True);
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveLog()
        {
            // Act
            await _repository.DeleteAsync(_testLog.Id);

            // Assert
            var deletedLog = await _context.MaintenanceLogs.FindAsync(_testLog.Id);
            Assert.That(deletedLog, Is.Null);
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
            var logs = new List<MaintenanceLog>();
            for (int i = 2; i <= 10; i++)
            {
                logs.Add(new MaintenanceLog
                {
                    Id = i,
                    SecuritySystemOrderId = _order.Id,
                    SecuritySystemOrder = _order,
                    TechnicianId = _technician.Id,
                    Technician = _technician,
                    Date = DateTime.Now,
                    Description = $"Maintenance log {i}",
                    Resolved = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.MaintenanceLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task GetLogsByClientIdAsync_ShouldOrderByDateDescending()
        {
            // Arrange
            var oldLog = new MaintenanceLog
            {
                Id = 2,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                TechnicianId = _technician.Id,
                Technician = _technician,
                Date = DateTime.Now.AddDays(-10),
                Description = "Old maintenance log",
                Resolved = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.MaintenanceLogs.AddAsync(oldLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            // First log should be the newer one (current test log)
            Assert.That(result.First().Id, Is.EqualTo(_testLog.Id));
        }

        [Test]
        public async Task GetLogsByTechnicianIdAsync_WithTechnicianAssignedToOrder_ShouldReturnLogs()
        {
            // Arrange - Add technician to order
            _order.Technicians = new List<User> { _technician };
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLogsByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
} 