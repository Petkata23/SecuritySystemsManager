using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class MaintenanceLogServiceTests : BaseServiceTests<MaintenanceLogDto, IMaintenanceLogRepository, MaintenanceLogService>
    {
        private Mock<ISecuritySystemOrderService> _mockOrderService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockOrderService = new Mock<ISecuritySystemOrderService>();
            _service = new MaintenanceLogService(_mockRepository.Object, _mockOrderService.Object);
        }

        protected override MaintenanceLogService CreateService(IMaintenanceLogRepository repository)
        {
            return new MaintenanceLogService(repository, _mockOrderService?.Object ?? new Mock<ISecuritySystemOrderService>().Object);
        }

        protected override MaintenanceLogDto CreateTestModel(int id = 1)
        {
            return new MaintenanceLogDto
            {
                Id = id,
                SecuritySystemOrderId = id,
                TechnicianId = id,
                Date = System.DateTime.UtcNow,
                Description = $"Maintenance description {id}",
                Resolved = false
            };
        }

        [Test]
        public async Task WhenSaveAsync_WithValidMaintenanceLogData_ThenSaveAsync()
        {
            // Arrange
            var maintenanceLogDto = new MaintenanceLogDto
            {
                SecuritySystemOrderId = 1,
                TechnicianId = 2,
                Date = System.DateTime.UtcNow,
                Description = "Camera maintenance",
                Resolved = false
            };

            // Act
            await _service.SaveAsync(maintenanceLogDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(maintenanceLogDto), Times.Once());
        }

        [Test]
        public async Task WhenSaveAsync_WithNull_ThenThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<System.ArgumentNullException>(async () => await _service.SaveAsync(null));

            _mockRepository.Verify(x => x.SaveAsync(null), Times.Never);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(22)]
        [TestCase(131)]
        public async Task WhenDeleteAsync_WithCorrectId_ThenCallDeleteAsyncInRepository(int id)
        {
            // Act
            await _service.DeleteAsync(id);

            // Assert
            _mockRepository.Verify(x => x.DeleteAsync(It.Is<int>(i => i.Equals(id))), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(22)]
        [TestCase(131)]
        public async Task WhenGetByIdAsync_WithValidMaintenanceLogId_ThenReturnMaintenanceLog(int maintenanceLogId)
        {
            // Arrange
            var maintenanceLogDto = new MaintenanceLogDto
            {
                Id = maintenanceLogId,
                SecuritySystemOrderId = 1,
                TechnicianId = 2,
                Date = System.DateTime.UtcNow,
                Description = "System check",
                Resolved = true
            };

            _mockRepository.Setup(x => x.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(maintenanceLogId))))
                .ReturnsAsync(maintenanceLogDto);

            // Act
            var maintenanceLogResult = await _service.GetByIdIfExistsAsync(maintenanceLogId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(maintenanceLogId), Times.Once());
            Assert.That(maintenanceLogResult, Is.EqualTo(maintenanceLogDto));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(102021)]
        public async Task WhenGetByIdAsync_WithInvalidMaintenanceLogId_ThenReturnDefault(int maintenanceLogId)
        {
            // Arrange
            var maintenanceLog = (MaintenanceLogDto)default;

            _mockRepository.Setup(s => s.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(maintenanceLogId))))
                .ReturnsAsync(maintenanceLog);

            // Act
            var maintenanceLogResult = await _service.GetByIdIfExistsAsync(maintenanceLogId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(maintenanceLogId), Times.Once());
            Assert.That(maintenanceLogResult, Is.EqualTo(maintenanceLog));
        }

        [Test]
        public async Task WhenUpdateAsync_WithValidData_ThenSaveAsync()
        {
            // Arrange
            var maintenanceLogDto = new MaintenanceLogDto
            {
                Id = 1,
                SecuritySystemOrderId = 1,
                TechnicianId = 2,
                Date = System.DateTime.UtcNow,
                Description = "Updated maintenance description",
                Resolved = true
            };

            _mockRepository.Setup(s => s.SaveAsync(It.Is<MaintenanceLogDto>(x => x.Equals(maintenanceLogDto))))
                .Verifiable();

            // Act
            await _service.SaveAsync(maintenanceLogDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(maintenanceLogDto), Times.Once());
        }

        [Test]
        public async Task WhenGetAllAsync_ThenReturnAllMaintenanceLogs()
        {
            // Arrange
            var maintenanceLogList = new List<MaintenanceLogDto>
            {
                new MaintenanceLogDto { Id = 1, Description = "Camera maintenance", Resolved = false },
                new MaintenanceLogDto { Id = 2, Description = "Alarm system check", Resolved = true },
                new MaintenanceLogDto { Id = 3, Description = "Access control update", Resolved = false }
            };

            _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(maintenanceLogList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            _mockRepository.Verify(x => x.GetAllAsync(), Times.Once());
            Assert.That(result, Is.EqualTo(maintenanceLogList));
        }

        [Test]
        public async Task GetLogsByUserRoleAsync_WithAdminRole_ShouldReturnAllLogs()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedLogs = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedLogs);

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetLogsByUserRoleAsync_WithManagerRole_ShouldReturnAllLogs()
        {
            // Arrange
            int userId = 1;
            string userRole = "Manager";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedLogs = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedLogs);

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetLogsByUserRoleAsync_WithClientRole_ShouldReturnClientLogs()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedLogs = CreateTestModels();
            _mockRepository.Setup(r => r.GetLogsByClientIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedLogs);

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
            _mockRepository.Verify(r => r.GetLogsByClientIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetLogsByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianLogs()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedLogs = CreateTestModels();
            _mockRepository.Setup(r => r.GetLogsByTechnicianIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedLogs);

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
            _mockRepository.Verify(r => r.GetLogsByTechnicianIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetLogsByUserRoleAsync_WithUnknownRole_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";
            int pageSize = 10;
            int pageNumber = 1;

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Theory]
        [TestCase(1, "Admin", 10, 1)]
        [TestCase(2, "Manager", 20, 2)]
        [TestCase(3, "Client", 5, 3)]
        [TestCase(4, "Technician", 15, 1)]
        public async Task GetLogsByUserRoleAsync_WithDifferentUserRoles_ShouldCallCorrectRepositoryMethod(int userId, string userRole, int pageSize, int pageNumber)
        {
            // Arrange
            var expectedLogs = CreateTestModels();
            
            if (userRole == "Admin" || userRole == "Manager")
            {
                _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedLogs);
            }
            else if (userRole == "Client")
            {
                _mockRepository.Setup(r => r.GetLogsByClientIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedLogs);
            }
            else if (userRole == "Technician")
            {
                _mockRepository.Setup(r => r.GetLogsByTechnicianIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedLogs);
            }

            // Act
            var result = await _service.GetLogsByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
        }

        [Test]
        public async Task GetLogsCountByUserRoleAsync_WithAdminRole_ShouldReturnAllLogsCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            var allLogs = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allLogs);

            // Act
            var result = await _service.GetLogsCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetLogsCountByUserRoleAsync_WithClientRole_ShouldReturnClientLogsCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            int expectedCount = 5;
            _mockRepository.Setup(r => r.GetLogsCountByClientIdAsync(userId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetLogsCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
            _mockRepository.Verify(r => r.GetLogsCountByClientIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetLogsCountByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianLogsCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            int expectedCount = 3;
            _mockRepository.Setup(r => r.GetLogsCountByTechnicianIdAsync(userId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetLogsCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
            _mockRepository.Verify(r => r.GetLogsCountByTechnicianIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetLogsCountByUserRoleAsync_WithUnknownRole_ShouldReturnZero()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";

            // Act
            var result = await _service.GetLogsCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Theory]
        [TestCase(1, "Admin")]
        [TestCase(2, "Manager")]
        [TestCase(3, "Client")]
        [TestCase(4, "Technician")]
        public async Task GetLogsCountByUserRoleAsync_WithDifferentUserRoles_ShouldCallCorrectRepositoryMethod(int userId, string userRole)
        {
            // Arrange
            int expectedCount = 5;
            
            if (userRole == "Admin" || userRole == "Manager")
            {
                var allLogs = CreateTestModels();
                _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allLogs);
            }
            else if (userRole == "Client")
            {
                _mockRepository.Setup(r => r.GetLogsCountByClientIdAsync(userId)).ReturnsAsync(expectedCount);
            }
            else if (userRole == "Technician")
            {
                _mockRepository.Setup(r => r.GetLogsCountByTechnicianIdAsync(userId)).ReturnsAsync(expectedCount);
            }

            // Act
            var result = await _service.GetLogsCountByUserRoleAsync(userId, userRole);

            // Assert
            if (userRole == "Admin" || userRole == "Manager")
            {
                Assert.That(result, Is.EqualTo(3));
            }
            else
            {
                Assert.That(result, Is.EqualTo(expectedCount));
            }
        }

        [Test]
        public async Task PrepareMaintenanceLogForOrderAsync_WithValidOrderId_ShouldReturnMaintenanceLog()
        {
            // Arrange
            int orderId = 1;
            int? technicianId = 2;
            var order = new SecuritySystemOrderDto { Id = orderId };
            _mockOrderService.Setup(o => o.GetByIdIfExistsAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _service.PrepareMaintenanceLogForOrderAsync(orderId, technicianId);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.TechnicianId, Is.EqualTo(technicianId.Value));
            Assert.That(result.SecuritySystemOrder, Is.EqualTo(order));
            _mockOrderService.Verify(o => o.GetByIdIfExistsAsync(orderId), Times.Once);
        }

        [Test]
        public void PrepareMaintenanceLogForOrderAsync_WithInvalidOrderId_ShouldThrowArgumentException()
        {
            // Arrange
            int orderId = 999;
            _mockOrderService.Setup(o => o.GetByIdIfExistsAsync(orderId)).ReturnsAsync((SecuritySystemOrderDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.PrepareMaintenanceLogForOrderAsync(orderId));
            Assert.That(exception.Message, Does.Contain($"Order with ID {orderId} not found"));
            _mockOrderService.Verify(o => o.GetByIdIfExistsAsync(orderId), Times.Once);
        }

        [Test]
        public async Task PrepareMaintenanceLogForOrderAsync_WithoutTechnicianId_ShouldReturnMaintenanceLogWithoutTechnician()
        {
            // Arrange
            int orderId = 1;
            var order = new SecuritySystemOrderDto { Id = orderId };
            _mockOrderService.Setup(o => o.GetByIdIfExistsAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _service.PrepareMaintenanceLogForOrderAsync(orderId);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.TechnicianId, Is.EqualTo(0));
            Assert.That(result.SecuritySystemOrder, Is.EqualTo(order));
            _mockOrderService.Verify(o => o.GetByIdIfExistsAsync(orderId), Times.Once);
        }

        [Theory]
        [TestCase(1, 2)]
        [TestCase(5, 10)]
        [TestCase(100, null)]
        public async Task PrepareMaintenanceLogForOrderAsync_WithDifferentOrderIds_ShouldReturnCorrectMaintenanceLog(int orderId, int? technicianId)
        {
            // Arrange
            var order = new SecuritySystemOrderDto { Id = orderId };
            _mockOrderService.Setup(o => o.GetByIdIfExistsAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _service.PrepareMaintenanceLogForOrderAsync(orderId, technicianId);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.SecuritySystemOrder, Is.EqualTo(order));
            if (technicianId.HasValue)
            {
                Assert.That(result.TechnicianId, Is.EqualTo(technicianId.Value));
            }
            else
            {
                Assert.That(result.TechnicianId, Is.EqualTo(0));
            }
            _mockOrderService.Verify(o => o.GetByIdIfExistsAsync(orderId), Times.Once);
        }

        [Test]
        public async Task WhenExistsByIdAsync_WithValidId_ThenReturnTrue()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(x => x.ExistsByIdAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(x => x.ExistsByIdAsync(id), Times.Once());
        }

        [Test]
        public async Task WhenExistsByIdAsync_WithInvalidId_ThenReturnFalse()
        {
            // Arrange
            int id = 999;
            _mockRepository.Setup(x => x.ExistsByIdAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(x => x.ExistsByIdAsync(id), Times.Once());
        }

        [Theory]
        [TestCase(10, 1)]
        [TestCase(20, 2)]
        [TestCase(5, 3)]
        public async Task WhenGetWithPaginationAsync_WithValidParameters_ThenReturnPaginatedMaintenanceLogs(int pageSize, int pageNumber)
        {
            // Arrange
            var maintenanceLogList = new List<MaintenanceLogDto>
            {
                new MaintenanceLogDto { Id = 1, Description = "Camera maintenance" },
                new MaintenanceLogDto { Id = 2, Description = "Alarm system check" }
            };

            _mockRepository.Setup(x => x.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(maintenanceLogList);

            // Act
            var result = await _service.GetWithPaginationAsync(pageSize, pageNumber);

            // Assert
            _mockRepository.Verify(x => x.GetWithPaginationAsync(pageSize, pageNumber), Times.Once());
            Assert.That(result, Is.EqualTo(maintenanceLogList));
        }
    }
} 