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
    public class MaintenanceDeviceRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private MaintenanceDeviceRepository _repository;
        private User _technician;
        private SecuritySystemOrder _order;
        private InstalledDevice _installedDevice;
        private MaintenanceLog _maintenanceLog;
        private MaintenanceDevice _testMaintenanceDevice;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _technician = new User
            {
                Id = 1,
                UserName = "technician@test.com",
                Email = "technician@test.com",
                FirstName = "Test",
                LastName = "Technician",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _order = new SecuritySystemOrder
            {
                Id = 1,
                Title = "Test Security System",
                Description = "Test security system installation",
                PhoneNumber = "+359888123456",
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

            _maintenanceLog = new MaintenanceLog
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

            _testMaintenanceDevice = new MaintenanceDevice
            {
                Id = 1,
                MaintenanceLogId = _maintenanceLog.Id,
                MaintenanceLog = _maintenanceLog,
                InstalledDeviceId = _installedDevice.Id,
                InstalledDevice = _installedDevice,
                Notes = "Camera lens was dirty",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(_technician);
            _context.Orders.Add(_order);
            _context.InstalledDevices.Add(_installedDevice);
            _context.MaintenanceLogs.Add(_maintenanceLog);
            _context.MaintenanceDevices.Add(_testMaintenanceDevice);
            _context.SaveChanges();

            _repository = new MaintenanceDeviceRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<MaintenanceDevice, MaintenanceDeviceDto>();
                cfg.CreateMap<MaintenanceDeviceDto, MaintenanceDevice>();
                cfg.CreateMap<MaintenanceLog, MaintenanceLogDto>();
                cfg.CreateMap<MaintenanceLogDto, MaintenanceLog>();
                cfg.CreateMap<InstalledDevice, InstalledDeviceDto>();
                cfg.CreateMap<InstalledDeviceDto, InstalledDevice>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>();
                cfg.CreateMap<SecuritySystemOrderDto, SecuritySystemOrder>();
                cfg.CreateMap<Location, LocationDto>();
                cfg.CreateMap<LocationDto, Location>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnMaintenanceDevice()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testMaintenanceDevice.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testMaintenanceDevice.Id));
            Assert.That(result.Notes, Is.EqualTo(_testMaintenanceDevice.Notes));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllMaintenanceDevices()
        {
            // Arrange
            var secondMaintenanceDevice = new MaintenanceDevice
            {
                Id = 2,
                MaintenanceLogId = _maintenanceLog.Id,
                MaintenanceLog = _maintenanceLog,
                InstalledDeviceId = _installedDevice.Id,
                InstalledDevice = _installedDevice,
                Notes = "Battery was low",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.MaintenanceDevices.AddAsync(secondMaintenanceDevice);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewMaintenanceDevice_ShouldSaveToDatabase()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                MaintenanceLogId = _maintenanceLog.Id,
                InstalledDeviceId = _installedDevice.Id,
                Notes = "Sensor was out of alignment"
            };

            // Act
            await _repository.SaveAsync(maintenanceDeviceDto);

            // Assert
            var savedMaintenanceDevice = await _context.MaintenanceDevices.FirstOrDefaultAsync(md => md.Notes == "Sensor was out of alignment");
            Assert.That(savedMaintenanceDevice, Is.Not.Null);
            Assert.That(savedMaintenanceDevice.Notes, Is.EqualTo("Sensor was out of alignment"));
            Assert.That(savedMaintenanceDevice.MaintenanceLogId, Is.EqualTo(_maintenanceLog.Id));
            Assert.That(savedMaintenanceDevice.InstalledDeviceId, Is.EqualTo(_installedDevice.Id));
        }

        [Test]
        public async Task SaveAsync_WithExistingMaintenanceDevice_ShouldUpdateMaintenanceDevice()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                Id = _testMaintenanceDevice.Id,
                MaintenanceLogId = _testMaintenanceDevice.MaintenanceLogId,
                InstalledDeviceId = _testMaintenanceDevice.InstalledDeviceId,
                Notes = "Updated notes"
            };

            // Act
            await _repository.SaveAsync(maintenanceDeviceDto);

            // Assert
            var updatedMaintenanceDevice = await _context.MaintenanceDevices.FindAsync(_testMaintenanceDevice.Id);
            Assert.That(updatedMaintenanceDevice, Is.Not.Null);
            Assert.That(updatedMaintenanceDevice.Notes, Is.EqualTo("Updated notes"));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveMaintenanceDevice()
        {
            // Act
            await _repository.DeleteAsync(_testMaintenanceDevice.Id);

            // Assert
            var deletedMaintenanceDevice = await _context.MaintenanceDevices.FindAsync(_testMaintenanceDevice.Id);
            Assert.That(deletedMaintenanceDevice, Is.Null);
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
            var maintenanceDevices = new List<MaintenanceDevice>();
            for (int i = 2; i <= 10; i++)
            {
                maintenanceDevices.Add(new MaintenanceDevice
                {
                    Id = i,
                    MaintenanceLogId = _maintenanceLog.Id,
                    MaintenanceLog = _maintenanceLog,
                    InstalledDeviceId = _installedDevice.Id,
                    InstalledDevice = _installedDevice,
                    Notes = $"Maintenance notes {i}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.MaintenanceDevices.AddRangeAsync(maintenanceDevices);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
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
        public async Task SaveAsync_WithMaintenanceDeviceHavingAllFields_ShouldSaveCorrectly()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                MaintenanceLogId = _maintenanceLog.Id,
                InstalledDeviceId = _installedDevice.Id,
                Notes = "Complete maintenance notes with detailed information"
            };

            // Act
            await _repository.SaveAsync(maintenanceDeviceDto);

            // Assert
            var savedMaintenanceDevice = await _context.MaintenanceDevices.FirstOrDefaultAsync(md => md.Notes == "Complete maintenance notes with detailed information");
            Assert.That(savedMaintenanceDevice, Is.Not.Null);
            Assert.That(savedMaintenanceDevice.Notes, Is.EqualTo("Complete maintenance notes with detailed information"));
            Assert.That(savedMaintenanceDevice.MaintenanceLogId, Is.EqualTo(_maintenanceLog.Id));
            Assert.That(savedMaintenanceDevice.InstalledDeviceId, Is.EqualTo(_installedDevice.Id));
        }

        [Test]
        public async Task SaveAsync_WithMultipleMaintenanceDevices_ShouldSaveAllCorrectly()
        {
            // Arrange
            var maintenanceDeviceDtos = new List<MaintenanceDeviceDto>
            {
                new MaintenanceDeviceDto
                {
                    MaintenanceLogId = _maintenanceLog.Id,
                    InstalledDeviceId = _installedDevice.Id,
                    Notes = "Notes 1"
                },
                new MaintenanceDeviceDto
                {
                    MaintenanceLogId = _maintenanceLog.Id,
                    InstalledDeviceId = _installedDevice.Id,
                    Notes = "Notes 2"
                },
                new MaintenanceDeviceDto
                {
                    MaintenanceLogId = _maintenanceLog.Id,
                    InstalledDeviceId = _installedDevice.Id,
                    Notes = "Notes 3"
                }
            };

            // Act
            foreach (var dto in maintenanceDeviceDtos)
            {
                await _repository.SaveAsync(dto);
            }

            // Assert
            var savedMaintenanceDevices = await _context.MaintenanceDevices.ToListAsync();
            Assert.That(savedMaintenanceDevices.Count, Is.EqualTo(4)); // 1 original + 3 new
        }

        [Test]
        public async Task SaveAsync_WithEmptyNotes_ShouldSaveCorrectly()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                MaintenanceLogId = _maintenanceLog.Id,
                InstalledDeviceId = _installedDevice.Id,
                Notes = ""
            };

            // Act
            await _repository.SaveAsync(maintenanceDeviceDto);

            // Assert
            var savedMaintenanceDevice = await _context.MaintenanceDevices.FirstOrDefaultAsync(md => md.Notes == "");
            Assert.That(savedMaintenanceDevice, Is.Not.Null);
            Assert.That(savedMaintenanceDevice.Notes, Is.EqualTo(""));
        }

        [Test]
        public async Task SaveAsync_WithNullNotes_ShouldSaveCorrectly()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                MaintenanceLogId = _maintenanceLog.Id,
                InstalledDeviceId = _installedDevice.Id,
                Notes = null
            };

            // Act
            await _repository.SaveAsync(maintenanceDeviceDto);

            // Assert
            var savedMaintenanceDevice = await _context.MaintenanceDevices.FirstOrDefaultAsync(md => md.Notes == null);
            Assert.That(savedMaintenanceDevice, Is.Not.Null);
            Assert.That(savedMaintenanceDevice.Notes, Is.Null);
        }
    }
} 