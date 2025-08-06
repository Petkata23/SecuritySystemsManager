using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManager.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class SecuritySystemOrderServiceTests : BaseServiceTests<SecuritySystemOrderDto, ISecuritySystemOrderRepository, SecuritySystemOrderService>
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IInvoiceService> _mockInvoiceService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockInvoiceService = new Mock<IInvoiceService>();
            _service = new SecuritySystemOrderService(_mockRepository.Object, _mockUserRepository.Object, _mockInvoiceService.Object);
        }

        protected override SecuritySystemOrderService CreateService(ISecuritySystemOrderRepository repository)
        {
            return new SecuritySystemOrderService(repository, _mockUserRepository?.Object ?? new Mock<IUserRepository>().Object, _mockInvoiceService?.Object ?? new Mock<IInvoiceService>().Object);
        }

        protected override SecuritySystemOrderDto CreateTestModel(int id = 1)
        {
            return new SecuritySystemOrderDto
            {
                Id = id,
                Title = $"Order {id}",
                Description = $"Description for order {id}",
                PhoneNumber = $"Phone {id}",
                ClientId = id,
                LocationId = id,
                Status = OrderStatus.Pending,
                RequestedDate = System.DateTime.UtcNow
            };
        }

        [Test]
        public async Task AddTechnicianToOrderAsync_WithValidData_ShouldAddTechnician()
        {
            // Arrange
            int orderId = 1;
            int technicianId = 2;
            var order = CreateTestModel(orderId);
            var technician = new UserDto { Id = technicianId, Username = "technician" };
            
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockUserRepository.Setup(r => r.GetByIdAsync(technicianId)).ReturnsAsync(technician);
            _mockRepository.Setup(r => r.AddTechnicianToOrderAsync(orderId, technicianId)).Returns(Task.CompletedTask);

            // Act
            await _service.AddTechnicianToOrderAsync(orderId, technicianId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
            _mockUserRepository.Verify(r => r.GetByIdAsync(technicianId), Times.Once);
            _mockRepository.Verify(r => r.AddTechnicianToOrderAsync(orderId, technicianId), Times.Once);
        }

        [Test]
        public void AddTechnicianToOrderAsync_WithInvalidOrderId_ShouldThrowArgumentException()
        {
            // Arrange
            int orderId = 999;
            int technicianId = 2;
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((SecuritySystemOrderDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.AddTechnicianToOrderAsync(orderId, technicianId));
            Assert.That(exception.Message, Does.Contain("Order not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        }

        [Test]
        public void AddTechnicianToOrderAsync_WithInvalidTechnicianId_ShouldThrowArgumentException()
        {
            // Arrange
            int orderId = 1;
            int technicianId = 999;
            var order = CreateTestModel(orderId);
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockUserRepository.Setup(r => r.GetByIdAsync(technicianId)).ReturnsAsync((UserDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.AddTechnicianToOrderAsync(orderId, technicianId));
            Assert.That(exception.Message, Does.Contain("Technician not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
            _mockUserRepository.Verify(r => r.GetByIdAsync(technicianId), Times.Once);
        }

        [Test]
        public async Task RemoveTechnicianFromOrderAsync_WithValidData_ShouldRemoveTechnician()
        {
            // Arrange
            int orderId = 1;
            int technicianId = 2;
            var order = CreateTestModel(orderId);
            var technician = new UserDto { Id = technicianId, Username = "technician" };
            
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockUserRepository.Setup(r => r.GetByIdAsync(technicianId)).ReturnsAsync(technician);
            _mockRepository.Setup(r => r.RemoveTechnicianFromOrderAsync(orderId, technicianId)).Returns(Task.CompletedTask);

            // Act
            await _service.RemoveTechnicianFromOrderAsync(orderId, technicianId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
            _mockUserRepository.Verify(r => r.GetByIdAsync(technicianId), Times.Once);
            _mockRepository.Verify(r => r.RemoveTechnicianFromOrderAsync(orderId, technicianId), Times.Once);
        }

        [Test]
        public void RemoveTechnicianFromOrderAsync_WithInvalidOrderId_ShouldThrowArgumentException()
        {
            // Arrange
            int orderId = 999;
            int technicianId = 2;
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((SecuritySystemOrderDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveTechnicianFromOrderAsync(orderId, technicianId));
            Assert.That(exception.Message, Does.Contain("Order not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        }

        [Test]
        public void RemoveTechnicianFromOrderAsync_WithInvalidTechnicianId_ShouldThrowArgumentException()
        {
            // Arrange
            int orderId = 1;
            int technicianId = 999;
            var order = CreateTestModel(orderId);
            _mockRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockUserRepository.Setup(r => r.GetByIdAsync(technicianId)).ReturnsAsync((UserDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveTechnicianFromOrderAsync(orderId, technicianId));
            Assert.That(exception.Message, Does.Contain("Technician not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
            _mockUserRepository.Verify(r => r.GetByIdAsync(technicianId), Times.Once);
        }

        [Test]
        public async Task GetOrdersByUserRoleAsync_WithAdminRole_ShouldReturnAllOrders()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedOrders);

            // Act
            var result = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedOrders));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetOrdersByUserRoleAsync_WithManagerRole_ShouldReturnAllOrders()
        {
            // Arrange
            int userId = 1;
            string userRole = "Manager";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedOrders);

            // Act
            var result = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedOrders));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetOrdersByUserRoleAsync_WithClientRole_ShouldReturnClientOrders()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetOrdersByClientIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedOrders);

            // Act
            var result = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedOrders));
            _mockRepository.Verify(r => r.GetOrdersByClientIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetOrdersByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianOrders()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetOrdersByTechnicianIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedOrders);

            // Act
            var result = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedOrders));
            _mockRepository.Verify(r => r.GetOrdersByTechnicianIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetOrdersByUserRoleAsync_WithUnknownRole_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";
            int pageSize = 10;
            int pageNumber = 1;

            // Act
            var result = await _service.GetOrdersByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetOrdersCountByUserRoleAsync_WithAdminRole_ShouldReturnAllOrdersCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            var allOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allOrders);

            // Act
            var result = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetOrdersCountByUserRoleAsync_WithManagerRole_ShouldReturnAllOrdersCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Manager";
            var allOrders = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allOrders);

            // Act
            var result = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetOrdersCountByUserRoleAsync_WithClientRole_ShouldReturnClientOrdersCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            _mockRepository.Setup(r => r.GetOrdersCountByClientIdAsync(userId)).ReturnsAsync(3);

            // Act
            var result = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetOrdersCountByClientIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetOrdersCountByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianOrdersCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            _mockRepository.Setup(r => r.GetOrdersCountByTechnicianIdAsync(userId)).ReturnsAsync(3);

            // Act
            var result = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetOrdersCountByTechnicianIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetOrdersCountByUserRoleAsync_WithUnknownRole_ShouldReturnZero()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";

            // Act
            var result = await _service.GetOrdersCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetOrderWithAllDetailsAsync_WithValidOrderId_ShouldReturnOrderWithDetails()
        {
            // Arrange
            int orderId = 1;
            var expectedOrder = CreateTestModel(orderId);
            _mockRepository.Setup(r => r.GetOrderWithAllDetailsAsync(orderId)).ReturnsAsync(expectedOrder);

            // Act
            var result = await _service.GetOrderWithAllDetailsAsync(orderId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedOrder));
            _mockRepository.Verify(r => r.GetOrderWithAllDetailsAsync(orderId), Times.Once);
        }
    }
} 