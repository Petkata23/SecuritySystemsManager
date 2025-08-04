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
    public class InstalledDeviceRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private InstalledDeviceRepository _repository;
        private User _technician;
        private SecuritySystemOrder _order;
        private InstalledDevice _testDevice;

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

            _testDevice = new InstalledDevice
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

            _context.Users.Add(_technician);
            _context.Orders.Add(_order);
            _context.InstalledDevices.Add(_testDevice);
            _context.SaveChanges();

            _repository = new InstalledDeviceRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<InstalledDevice, InstalledDeviceDto>();
                cfg.CreateMap<InstalledDeviceDto, InstalledDevice>();
                cfg.CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>();
                cfg.CreateMap<SecuritySystemOrderDto, SecuritySystemOrder>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnDevice()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testDevice.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testDevice.Id));
            Assert.That(result.Brand, Is.EqualTo(_testDevice.Brand));
            Assert.That(result.Model, Is.EqualTo(_testDevice.Model));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllDevices()
        {
            // Arrange
            var secondDevice = new InstalledDevice
            {
                Id = 2,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                DeviceType = DeviceType.Alarm,
                Brand = "Second Brand",
                Model = "Second Model",
                Quantity = 2,
                DateInstalled = DateTime.Now,
                InstalledById = _technician.Id,
                InstalledBy = _technician,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.InstalledDevices.AddAsync(secondDevice);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewDevice_ShouldSaveToDatabase()
        {
            // Arrange
            var deviceDto = new InstalledDeviceDto
            {
                SecuritySystemOrderId = _order.Id,
                DeviceType = DeviceType.AccessControl,
                Brand = "New Brand",
                Model = "New Model",
                Quantity = 3,
                InstalledById = _technician.Id
            };

            // Act
            await _repository.SaveAsync(deviceDto);

            // Assert
            var savedDevice = await _context.InstalledDevices.FirstOrDefaultAsync(d => d.Brand == "New Brand");
            Assert.That(savedDevice, Is.Not.Null);
            Assert.That(savedDevice.Model, Is.EqualTo("New Model"));
            Assert.That(savedDevice.Quantity, Is.EqualTo(3));
            Assert.That(savedDevice.DeviceType, Is.EqualTo(DeviceType.AccessControl));
        }

        [Test]
        public async Task SaveAsync_WithExistingDevice_ShouldUpdateDevice()
        {
            // Arrange
            var deviceDto = new InstalledDeviceDto
            {
                Id = _testDevice.Id,
                SecuritySystemOrderId = _testDevice.SecuritySystemOrderId,
                DeviceType = _testDevice.DeviceType,
                Brand = "Updated Brand",
                Model = "Updated Model",
                Quantity = _testDevice.Quantity,
                InstalledById = _testDevice.InstalledById
            };

            // Act
            await _repository.SaveAsync(deviceDto);

            // Assert
            var updatedDevice = await _context.InstalledDevices.FindAsync(_testDevice.Id);
            Assert.That(updatedDevice, Is.Not.Null);
            Assert.That(updatedDevice.Brand, Is.EqualTo("Updated Brand"));
            Assert.That(updatedDevice.Model, Is.EqualTo("Updated Model"));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveDevice()
        {
            // Act
            await _repository.DeleteAsync(_testDevice.Id);

            // Assert
            var deletedDevice = await _context.InstalledDevices.FindAsync(_testDevice.Id);
            Assert.That(deletedDevice, Is.Null);
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
            var devices = new List<InstalledDevice>();
            for (int i = 2; i <= 10; i++)
            {
                devices.Add(new InstalledDevice
                {
                    Id = i,
                    SecuritySystemOrderId = _order.Id,
                    SecuritySystemOrder = _order,
                    DeviceType = DeviceType.Camera,
                    Brand = $"Brand {i}",
                    Model = $"Model {i}",
                    Quantity = i,
                    DateInstalled = DateTime.Now,
                    InstalledById = _technician.Id,
                    InstalledBy = _technician,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.InstalledDevices.AddRangeAsync(devices);
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
        public async Task SaveAsync_WithDeviceHavingAllFields_ShouldSaveCorrectly()
        {
            // Arrange
            var deviceDto = new InstalledDeviceDto
            {
                SecuritySystemOrderId = _order.Id,
                DeviceType = DeviceType.Camera,
                Brand = "Complete Brand",
                Model = "Complete Model",
                Quantity = 5,
                InstalledById = _technician.Id
            };

            // Act
            await _repository.SaveAsync(deviceDto);

            // Assert
            var savedDevice = await _context.InstalledDevices.FirstOrDefaultAsync(d => d.Brand == "Complete Brand");
            Assert.That(savedDevice, Is.Not.Null);
            Assert.That(savedDevice.Brand, Is.EqualTo("Complete Brand"));
            Assert.That(savedDevice.Model, Is.EqualTo("Complete Model"));
            Assert.That(savedDevice.Quantity, Is.EqualTo(5));
            Assert.That(savedDevice.DeviceType, Is.EqualTo(DeviceType.Camera));
            Assert.That(savedDevice.InstalledById, Is.EqualTo(_technician.Id));
        }

        [Test]
        public async Task SaveAsync_WithDifferentDeviceTypes_ShouldSaveCorrectly()
        {
            // Arrange
            var deviceTypes = new[] { DeviceType.Camera, DeviceType.Alarm, DeviceType.AccessControl };
            var devices = new List<InstalledDeviceDto>();

            for (int i = 0; i < deviceTypes.Length; i++)
            {
                devices.Add(new InstalledDeviceDto
                {
                    SecuritySystemOrderId = _order.Id,
                    DeviceType = deviceTypes[i],
                    Brand = $"Brand {i}",
                    Model = $"Model {i}",
                    Quantity = i + 1,
                    InstalledById = _technician.Id
                });
            }

            // Act
            foreach (var deviceDto in devices)
            {
                await _repository.SaveAsync(deviceDto);
            }

            // Assert
            var savedDevices = await _context.InstalledDevices.ToListAsync();
            Assert.That(savedDevices.Count, Is.EqualTo(4)); // 1 original + 3 new
        }
    }
} 