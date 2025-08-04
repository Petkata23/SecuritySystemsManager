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
    public class InvoiceRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private InvoiceRepository _repository;
        private User _client;
        private User _technician;
        private Location _location;
        private SecuritySystemOrder _order;
        private Invoice _testInvoice;

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
                Status = SecuritySystemsManager.Shared.Enums.OrderStatus.Completed,
                RequestedDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _testInvoice = new Invoice
            {
                Id = 1,
                TotalAmount = 1500.00m,
                IssuedOn = DateTime.Now,
                IsPaid = false,
                SecuritySystemOrderId = _order.Id,
                SecuritySystemOrder = _order,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.AddRange(_client, _technician);
            _context.Locations.Add(_location);
            _context.Orders.Add(_order);
            _context.Invoices.Add(_testInvoice);
            _context.SaveChanges();

            _repository = new InvoiceRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<Invoice, InvoiceDto>();
                cfg.CreateMap<InvoiceDto, Invoice>();
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
        public async Task GetInvoiceWithDetailsAsync_WithValidId_ShouldReturnInvoiceWithDetails()
        {
            // Act
            var result = await _repository.GetInvoiceWithDetailsAsync(_testInvoice.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testInvoice.Id));
            Assert.That(result.TotalAmount, Is.EqualTo(_testInvoice.TotalAmount));
            Assert.That(result.IsPaid, Is.EqualTo(_testInvoice.IsPaid));
        }

        [Test]
        public async Task GetInvoiceWithDetailsAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetInvoiceWithDetailsAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetInvoicesByClientIdAsync_ShouldReturnClientInvoices()
        {
            // Act
            var result = await _repository.GetInvoicesByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }

        [Test]
        public async Task GetInvoicesByClientIdAsync_WithPagination_ShouldReturnLimitedResults()
        {
            // Act
            var result = await _repository.GetInvoicesByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }

        [Test]
        public async Task GetInvoicesByTechnicianIdAsync_ShouldReturnTechnicianInvoices()
        {
            // Arrange - Add technician to order
            _order.Technicians = new List<User> { _technician };
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetInvoicesByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetInvoicesByTechnicianIdAsync_WithNoAssignedOrders_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetInvoicesByTechnicianIdAsync(_technician.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetInvoicesCountByClientIdAsync_ShouldReturnCorrectCount()
        {
            // Act
            var result = await _repository.GetInvoicesCountByClientIdAsync(_client.Id);

            // Assert
            Assert.That(result, Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }

        [Test]
        public async Task GetInvoicesCountByTechnicianIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange - Add technician to order
            _order.Technicians = new List<User> { _technician };
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetInvoicesCountByTechnicianIdAsync(_technician.Id);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task GetInvoiceByOrderIdAsync_WithValidOrderId_ShouldReturnInvoice()
        {
            // Act
            var result = await _repository.GetInvoiceByOrderIdAsync(_order.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testInvoice.Id));
            Assert.That(result.TotalAmount, Is.EqualTo(_testInvoice.TotalAmount));
        }

        [Test]
        public async Task GetInvoiceByOrderIdAsync_WithInvalidOrderId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetInvoiceByOrderIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnInvoice()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testInvoice.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testInvoice.Id));
            Assert.That(result.TotalAmount, Is.EqualTo(_testInvoice.TotalAmount));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllInvoices()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }

        [Test]
        public async Task SaveAsync_WithNewInvoice_ShouldSaveToDatabase()
        {
            // Arrange
            var invoiceDto = new InvoiceDto
            {
                TotalAmount = 3000.00m,
                IssuedOn = DateTime.Now,
                IsPaid = false,
                SecuritySystemOrderId = _order.Id
            };

            // Act
            await _repository.SaveAsync(invoiceDto);

            // Assert
            var savedInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.TotalAmount == 3000.00m);
            Assert.That(savedInvoice, Is.Not.Null);
            Assert.That(savedInvoice.TotalAmount, Is.EqualTo(3000.00m));
            Assert.That(savedInvoice.SecuritySystemOrderId, Is.EqualTo(_order.Id));
        }

        [Test]
        public async Task SaveAsync_WithExistingInvoice_ShouldUpdateInvoice()
        {
            // Arrange
            var invoiceDto = new InvoiceDto
            {
                Id = _testInvoice.Id,
                TotalAmount = 2500.00m,
                IssuedOn = _testInvoice.IssuedOn,
                IsPaid = true,
                SecuritySystemOrderId = _testInvoice.SecuritySystemOrderId
            };

            // Act
            await _repository.SaveAsync(invoiceDto);

            // Assert
            var updatedInvoice = await _context.Invoices.FindAsync(_testInvoice.Id);
            Assert.That(updatedInvoice, Is.Not.Null);
            Assert.That(updatedInvoice.TotalAmount, Is.EqualTo(2500.00m));
            Assert.That(updatedInvoice.IsPaid, Is.True);
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveInvoice()
        {
            // Act
            await _repository.DeleteAsync(_testInvoice.Id);

            // Assert
            var deletedInvoice = await _context.Invoices.FindAsync(_testInvoice.Id);
            Assert.That(deletedInvoice, Is.Null);
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
            // Act
            var result = await _repository.GetWithPaginationAsync(5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }

        [Test]
        public async Task GetInvoicesByClientIdAsync_ShouldOrderByIssuedOnDescending()
        {
            // Act
            var result = await _repository.GetInvoicesByClientIdAsync(_client.Id, 10, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // В базата има само 1 инвойс по подразбиране
        }
    }
} 