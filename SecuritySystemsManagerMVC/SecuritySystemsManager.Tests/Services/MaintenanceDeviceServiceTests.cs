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
    public class MaintenanceDeviceServiceTests : BaseServiceTests<MaintenanceDeviceDto, IMaintenanceDeviceRepository, MaintenanceDeviceService>
    {
        private Mock<IInstalledDeviceService> _mockInstalledDeviceService;
        private Mock<IMaintenanceLogRepository> _mockMaintenanceLogRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockInstalledDeviceService = new Mock<IInstalledDeviceService>();
            _mockMaintenanceLogRepository = new Mock<IMaintenanceLogRepository>();
            _service = new MaintenanceDeviceService(_mockRepository.Object, _mockInstalledDeviceService.Object, _mockMaintenanceLogRepository.Object);
        }

        protected override MaintenanceDeviceService CreateService(IMaintenanceDeviceRepository repository)
        {
            return new MaintenanceDeviceService(repository, _mockInstalledDeviceService?.Object ?? new Mock<IInstalledDeviceService>().Object, _mockMaintenanceLogRepository?.Object ?? new Mock<IMaintenanceLogRepository>().Object);
        }

        protected override MaintenanceDeviceDto CreateTestModel(int id = 1)
        {
            return new MaintenanceDeviceDto
            {
                Id = id,
                MaintenanceLogId = id,
                InstalledDeviceId = id,
                Notes = $"Notes for device {id}",
                IsFixed = false
            };
        }

        [Test]
        public async Task WhenSaveAsync_WithValidMaintenanceDeviceData_ThenSaveAsync()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                MaintenanceLogId = 1,
                InstalledDeviceId = 2,
                Notes = "Device needs repair",
                IsFixed = false
            };

            // Act
            await _service.SaveAsync(maintenanceDeviceDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(maintenanceDeviceDto), Times.Once());
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
        public async Task WhenGetByIdAsync_WithValidMaintenanceDeviceId_ThenReturnMaintenanceDevice(int maintenanceDeviceId)
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                Id = maintenanceDeviceId,
                MaintenanceLogId = 1,
                InstalledDeviceId = 2,
                Notes = "Device maintenance required",
                IsFixed = false
            };

            _mockRepository.Setup(x => x.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(maintenanceDeviceId))))
                .ReturnsAsync(maintenanceDeviceDto);

            // Act
            var maintenanceDeviceResult = await _service.GetByIdIfExistsAsync(maintenanceDeviceId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(maintenanceDeviceId), Times.Once());
            Assert.That(maintenanceDeviceResult, Is.EqualTo(maintenanceDeviceDto));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(102021)]
        public async Task WhenGetByIdAsync_WithInvalidMaintenanceDeviceId_ThenReturnDefault(int maintenanceDeviceId)
        {
            // Arrange
            var maintenanceDevice = (MaintenanceDeviceDto)default;

            _mockRepository.Setup(s => s.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(maintenanceDeviceId))))
                .ReturnsAsync(maintenanceDevice);

            // Act
            var maintenanceDeviceResult = await _service.GetByIdIfExistsAsync(maintenanceDeviceId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(maintenanceDeviceId), Times.Once());
            Assert.That(maintenanceDeviceResult, Is.EqualTo(maintenanceDevice));
        }

        [Test]
        public async Task WhenUpdateAsync_WithValidData_ThenSaveAsync()
        {
            // Arrange
            var maintenanceDeviceDto = new MaintenanceDeviceDto
            {
                Id = 1,
                MaintenanceLogId = 1,
                InstalledDeviceId = 2,
                Notes = "Updated maintenance notes",
                IsFixed = true
            };

            _mockRepository.Setup(s => s.SaveAsync(It.Is<MaintenanceDeviceDto>(x => x.Equals(maintenanceDeviceDto))))
                .Verifiable();

            // Act
            await _service.SaveAsync(maintenanceDeviceDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(maintenanceDeviceDto), Times.Once());
        }

        [Test]
        public async Task WhenGetAllAsync_ThenReturnAllMaintenanceDevices()
        {
            // Arrange
            var maintenanceDeviceList = new List<MaintenanceDeviceDto>
            {
                new MaintenanceDeviceDto { Id = 1, Notes = "Camera repair needed", IsFixed = false },
                new MaintenanceDeviceDto { Id = 2, Notes = "Alarm system check", IsFixed = true },
                new MaintenanceDeviceDto { Id = 3, Notes = "Access control maintenance", IsFixed = false }
            };

            _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(maintenanceDeviceList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            _mockRepository.Verify(x => x.GetAllAsync(), Times.Once());
            Assert.That(result, Is.EqualTo(maintenanceDeviceList));
        }

        [Test]
        public async Task PrepareMaintenanceDeviceAsync_WithValidDeviceId_ShouldReturnMaintenanceDevice()
        {
            // Arrange
            int deviceId = 1;
            var installedDevice = new InstalledDeviceDto { Id = deviceId };
            
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(true);
            _mockInstalledDeviceService.Setup(s => s.GetByIdIfExistsAsync(deviceId)).ReturnsAsync(installedDevice);

            // Act
            var result = await _service.PrepareMaintenanceDeviceAsync(deviceId);

            // Assert
            Assert.That(result.InstalledDeviceId, Is.EqualTo(deviceId));
            Assert.That(result.InstalledDevice, Is.EqualTo(installedDevice));
            Assert.That(result.IsFixed, Is.False);
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
            _mockInstalledDeviceService.Verify(s => s.GetByIdIfExistsAsync(deviceId), Times.Once);
        }

        [Test]
        public void PrepareMaintenanceDeviceAsync_WithInvalidDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = 999;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.PrepareMaintenanceDeviceAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void PrepareMaintenanceDeviceAsync_WithZeroDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = 0;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.PrepareMaintenanceDeviceAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void PrepareMaintenanceDeviceAsync_WithNegativeDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = -1;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.PrepareMaintenanceDeviceAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task PrepareMaintenanceDeviceAsync_WithDifferentDeviceIds_ShouldReturnCorrectMaintenanceDevice(int deviceId)
        {
            // Arrange
            var installedDevice = new InstalledDeviceDto { Id = deviceId };
            
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(true);
            _mockInstalledDeviceService.Setup(s => s.GetByIdIfExistsAsync(deviceId)).ReturnsAsync(installedDevice);

            // Act
            var result = await _service.PrepareMaintenanceDeviceAsync(deviceId);

            // Assert
            Assert.That(result.InstalledDeviceId, Is.EqualTo(deviceId));
            Assert.That(result.InstalledDevice, Is.EqualTo(installedDevice));
            Assert.That(result.IsFixed, Is.False);
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
            _mockInstalledDeviceService.Verify(s => s.GetByIdIfExistsAsync(deviceId), Times.Once);
        }

        [Test]
        public async Task AddDeviceToMaintenanceLogAsync_WithValidData_ShouldAddDeviceToLog()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int logId = 1;
            var log = new MaintenanceLogDto { Id = logId };
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddDeviceToMaintenanceLogAsync(maintenanceDevice, logId);

            // Assert
            Assert.That(result.MaintenanceLogId, Is.EqualTo(logId));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(maintenanceDevice), Times.Once);
        }

        [Test]
        public void AddDeviceToMaintenanceLogAsync_WithInvalidLogId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int logId = 999;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddDeviceToMaintenanceLogAsync(maintenanceDevice, logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Test]
        public void AddDeviceToMaintenanceLogAsync_WithZeroLogId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int logId = 0;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddDeviceToMaintenanceLogAsync(maintenanceDevice, logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Test]
        public void AddDeviceToMaintenanceLogAsync_WithNegativeLogId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int logId = -1;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddDeviceToMaintenanceLogAsync(maintenanceDevice, logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Theory]
        [TestCase(1, 1)]
        [TestCase(5, 10)]
        [TestCase(100, 50)]
        public async Task AddDeviceToMaintenanceLogAsync_WithDifferentLogIds_ShouldAddDeviceToCorrectLog(int logId, int deviceId)
        {
            // Arrange
            var maintenanceDevice = CreateTestModel(deviceId);
            var log = new MaintenanceLogDto { Id = logId };
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddDeviceToMaintenanceLogAsync(maintenanceDevice, logId);

            // Assert
            Assert.That(result.MaintenanceLogId, Is.EqualTo(logId));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(maintenanceDevice), Times.Once);
        }

        [Test]
        public async Task AddInstalledDeviceToMaintenanceAsync_WithValidData_ShouldAddDeviceToMaintenance()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int deviceId = 1;
            
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDevice, deviceId);

            // Assert
            Assert.That(result.InstalledDeviceId, Is.EqualTo(deviceId));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(maintenanceDevice), Times.Once);
        }

        [Test]
        public void AddInstalledDeviceToMaintenanceAsync_WithInvalidDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int deviceId = 999;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDevice, deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void AddInstalledDeviceToMaintenanceAsync_WithZeroDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int deviceId = 0;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDevice, deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void AddInstalledDeviceToMaintenanceAsync_WithNegativeDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            var maintenanceDevice = CreateTestModel();
            int deviceId = -1;
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDevice, deviceId));
            Assert.That(exception.Message, Does.Contain($"Device with ID {deviceId} not found"));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
        }

        [Theory]
        [TestCase(1, 1)]
        [TestCase(5, 10)]
        [TestCase(100, 50)]
        public async Task AddInstalledDeviceToMaintenanceAsync_WithDifferentDeviceIds_ShouldAddDeviceToMaintenance(int deviceId, int maintenanceDeviceId)
        {
            // Arrange
            var maintenanceDevice = CreateTestModel(maintenanceDeviceId);
            
            _mockInstalledDeviceService.Setup(s => s.ExistsByIdAsync(deviceId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddInstalledDeviceToMaintenanceAsync(maintenanceDevice, deviceId);

            // Assert
            Assert.That(result.InstalledDeviceId, Is.EqualTo(deviceId));
            _mockInstalledDeviceService.Verify(s => s.ExistsByIdAsync(deviceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(maintenanceDevice), Times.Once);
        }

        [Test]
        public async Task ToggleDeviceFixedStatusAsync_WithValidDeviceId_ShouldToggleStatus()
        {
            // Arrange
            int deviceId = 1;
            var device = CreateTestModel(deviceId);
            device.IsFixed = false;
            
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.ToggleDeviceFixedStatusAsync(deviceId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<MaintenanceDeviceDto>(d => d.IsFixed)), Times.Once);
        }

        [Test]
        public async Task ToggleDeviceFixedStatusAsync_WithAlreadyFixedDevice_ShouldToggleToUnfixed()
        {
            // Arrange
            int deviceId = 1;
            var device = CreateTestModel(deviceId);
            device.IsFixed = true; // Already fixed
            
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.ToggleDeviceFixedStatusAsync(deviceId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<MaintenanceDeviceDto>(d => !d.IsFixed)), Times.Once);
        }

        [Test]
        public void ToggleDeviceFixedStatusAsync_WithInvalidDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync((MaintenanceDeviceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.ToggleDeviceFixedStatusAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Maintenance device with ID {deviceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void ToggleDeviceFixedStatusAsync_WithZeroDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = 0;
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync((MaintenanceDeviceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.ToggleDeviceFixedStatusAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Maintenance device with ID {deviceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
        }

        [Test]
        public void ToggleDeviceFixedStatusAsync_WithNegativeDeviceId_ShouldThrowArgumentException()
        {
            // Arrange
            int deviceId = -1;
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync((MaintenanceDeviceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.ToggleDeviceFixedStatusAsync(deviceId));
            Assert.That(exception.Message, Does.Contain($"Maintenance device with ID {deviceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task ToggleDeviceFixedStatusAsync_WithDifferentDeviceIds_ShouldToggleStatus(int deviceId)
        {
            // Arrange
            var device = CreateTestModel(deviceId);
            device.IsFixed = false;
            
            _mockRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.ToggleDeviceFixedStatusAsync(deviceId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<MaintenanceDeviceDto>(d => d.IsFixed)), Times.Once);
        }

        [Test]
        public async Task MarkAllDevicesFixedForLogAsync_WithValidLogId_ShouldMarkAllDevicesFixed()
        {
            // Arrange
            int logId = 1;
            var log = new MaintenanceLogDto { Id = logId };
            var devices = CreateTestModels();
            devices[0].MaintenanceLogId = logId;
            devices[0].IsFixed = false;
            devices[1].MaintenanceLogId = logId;
            devices[1].IsFixed = false;
            devices[2].MaintenanceLogId = 2; // Different log
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(devices);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllDevicesFixedForLogAsync(logId);

            // Assert
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>()), Times.Exactly(2));
        }

        [Test]
        public async Task MarkAllDevicesFixedForLogAsync_WithAlreadyFixedDevices_ShouldKeepDevicesFixed()
        {
            // Arrange
            int logId = 1;
            var log = new MaintenanceLogDto { Id = logId };
            var devices = CreateTestModels();
            devices[0].MaintenanceLogId = logId;
            devices[0].IsFixed = true; // Already fixed
            devices[1].MaintenanceLogId = logId;
            devices[1].IsFixed = true; // Already fixed
            devices[2].MaintenanceLogId = 2; // Different log
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(devices);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllDevicesFixedForLogAsync(logId);

            // Assert
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>()), Times.Exactly(2));
        }

        [Test]
        public async Task MarkAllDevicesFixedForLogAsync_WithNoDevicesForLog_ShouldNotSaveAny()
        {
            // Arrange
            int logId = 1;
            var log = new MaintenanceLogDto { Id = logId };
            var devices = CreateTestModels();
            devices[0].MaintenanceLogId = 2; // Different log
            devices[1].MaintenanceLogId = 3; // Different log
            devices[2].MaintenanceLogId = 4; // Different log
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(devices);

            // Act
            await _service.MarkAllDevicesFixedForLogAsync(logId);

            // Assert
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>()), Times.Never);
        }

        [Test]
        public void MarkAllDevicesFixedForLogAsync_WithInvalidLogId_ShouldThrowArgumentException()
        {
            // Arrange
            int logId = 999;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.MarkAllDevicesFixedForLogAsync(logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Test]
        public void MarkAllDevicesFixedForLogAsync_WithZeroLogId_ShouldThrowArgumentException()
        {
            // Arrange
            int logId = 0;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.MarkAllDevicesFixedForLogAsync(logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Test]
        public void MarkAllDevicesFixedForLogAsync_WithNegativeLogId_ShouldThrowArgumentException()
        {
            // Arrange
            int logId = -1;
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((MaintenanceLogDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<System.ArgumentException>(() => _service.MarkAllDevicesFixedForLogAsync(logId));
            Assert.That(exception.Message, Does.Contain($"Maintenance log with ID {logId} not found"));
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task MarkAllDevicesFixedForLogAsync_WithDifferentLogIds_ShouldMarkAllDevicesFixed(int logId)
        {
            // Arrange
            var log = new MaintenanceLogDto { Id = logId };
            var devices = CreateTestModels();
            devices[0].MaintenanceLogId = logId;
            devices[0].IsFixed = false;
            devices[1].MaintenanceLogId = logId;
            devices[1].IsFixed = false;
            devices[2].MaintenanceLogId = logId + 1; // Different log
            
            _mockMaintenanceLogRepository.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(devices);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllDevicesFixedForLogAsync(logId);

            // Assert
            _mockMaintenanceLogRepository.Verify(r => r.GetByIdAsync(logId), Times.Once);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<MaintenanceDeviceDto>()), Times.Exactly(2));
        }

        [Test]
        public async Task GetDevicesByMaintenanceLogIdAsync_WithValidLogId_ShouldReturnDevices()
        {
            // Arrange
            int logId = 1;
            var allDevices = CreateTestModels();
            allDevices[0].MaintenanceLogId = logId;
            allDevices[1].MaintenanceLogId = logId;
            allDevices[2].MaintenanceLogId = 2; // Different log
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allDevices);

            // Act
            var result = await _service.GetDevicesByMaintenanceLogIdAsync(logId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(d => d.MaintenanceLogId == logId), Is.True);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetDevicesByMaintenanceLogIdAsync_WithNoDevicesForLog_ShouldReturnEmptyList()
        {
            // Arrange
            int logId = 1;
            var allDevices = CreateTestModels();
            allDevices[0].MaintenanceLogId = 2; // Different log
            allDevices[1].MaintenanceLogId = 3; // Different log
            allDevices[2].MaintenanceLogId = 4; // Different log
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allDevices);

            // Act
            var result = await _service.GetDevicesByMaintenanceLogIdAsync(logId);

            // Assert
            Assert.That(result, Is.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetDevicesByMaintenanceLogIdAsync_WithNoDevices_ShouldReturnEmptyList()
        {
            // Arrange
            int logId = 1;
            var emptyDevices = new List<MaintenanceDeviceDto>();
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyDevices);

            // Act
            var result = await _service.GetDevicesByMaintenanceLogIdAsync(logId);

            // Assert
            Assert.That(result, Is.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task GetDevicesByMaintenanceLogIdAsync_WithDifferentLogIds_ShouldReturnCorrectDevices(int logId)
        {
            // Arrange
            var allDevices = CreateTestModels();
            allDevices[0].MaintenanceLogId = logId;
            allDevices[1].MaintenanceLogId = logId;
            allDevices[2].MaintenanceLogId = logId + 1; // Different log
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allDevices);

            // Act
            var result = await _service.GetDevicesByMaintenanceLogIdAsync(logId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(d => d.MaintenanceLogId == logId), Is.True);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
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
        public async Task WhenGetWithPaginationAsync_WithValidParameters_ThenReturnPaginatedMaintenanceDevices(int pageSize, int pageNumber)
        {
            // Arrange
            var maintenanceDeviceList = new List<MaintenanceDeviceDto>
            {
                new MaintenanceDeviceDto { Id = 1, Notes = "Camera repair" },
                new MaintenanceDeviceDto { Id = 2, Notes = "Alarm system" }
            };

            _mockRepository.Setup(x => x.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(maintenanceDeviceList);

            // Act
            var result = await _service.GetWithPaginationAsync(pageSize, pageNumber);

            // Assert
            _mockRepository.Verify(x => x.GetWithPaginationAsync(pageSize, pageNumber), Times.Once());
            Assert.That(result, Is.EqualTo(maintenanceDeviceList));
        }
    }
} 