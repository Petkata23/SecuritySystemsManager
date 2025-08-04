using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManager.Shared.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class InstalledDeviceServiceTests : BaseServiceTests<InstalledDeviceDto, IInstalledDeviceRepository, InstalledDeviceService>
    {
        private Mock<IFileStorageService> _mockFileStorageService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _service = new InstalledDeviceService(_mockRepository.Object, _mockFileStorageService.Object);
        }

        protected override InstalledDeviceService CreateService(IInstalledDeviceRepository repository)
        {
            return new InstalledDeviceService(repository, _mockFileStorageService?.Object ?? new Mock<IFileStorageService>().Object);
        }

        protected override InstalledDeviceDto CreateTestModel(int id = 1)
        {
            return new InstalledDeviceDto
            {
                Id = id,
                SecuritySystemOrderId = id,
                DeviceType = DeviceType.Camera,
                Brand = $"Brand {id}",
                Model = $"Model {id}",
                Quantity = 1,
                DateInstalled = System.DateTime.UtcNow,
                InstalledById = id,
                DeviceImage = $"device_{id}.jpg"
            };
        }

        [Test]
        public async Task UploadDeviceImageAsync_WithValidFile_ShouldReturnImagePath()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string expectedPath = "uploads/devices/test.jpg";
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/devices")).ReturnsAsync(expectedPath);

            // Act
            var result = await _service.UploadDeviceImageAsync(mockFile.Object);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPath));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/devices"), Times.Once);
        }

        [Test]
        public async Task UploadDeviceImageAsync_WithNullFile_ShouldReturnNull()
        {
            // Act
            var result = await _service.UploadDeviceImageAsync(null);

            // Assert
            Assert.That(result, Is.Null);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UploadDeviceImageAsync_WithEmptyFile_ShouldReturnNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _service.UploadDeviceImageAsync(mockFile.Object);

            // Assert
            Assert.That(result, Is.Null);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task AddDeviceToOrderAsync_WithImageFile_ShouldUploadImageAndSaveDevice()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string expectedImagePath = "uploads/devices/device.jpg";
            
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/devices")).ReturnsAsync(expectedImagePath);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddDeviceToOrderAsync(deviceDto, mockFile.Object);

            // Assert
            Assert.That(result.DeviceImage, Is.EqualTo(expectedImagePath));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/devices"), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }

        [Test]
        public async Task AddDeviceToOrderAsync_WithoutImageFile_ShouldSaveDeviceOnly()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddDeviceToOrderAsync(deviceDto, null);

            // Assert
            Assert.That(result, Is.EqualTo(deviceDto));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }

        [Test]
        public async Task UpdateDeviceWithImageAsync_WithValidData_ShouldDeleteOldImageAndUploadNew()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            deviceDto.DeviceImage = "old_image.jpg";
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string newImagePath = "uploads/devices/new_image.jpg";
            
            _mockFileStorageService.Setup(f => f.DeleteFileAsync("old_image.jpg")).ReturnsAsync(true);
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/devices")).ReturnsAsync(newImagePath);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateDeviceWithImageAsync(deviceDto, mockFile.Object);

            // Assert
            Assert.That(result.DeviceImage, Is.EqualTo(newImagePath));
            _mockFileStorageService.Verify(f => f.DeleteFileAsync("old_image.jpg"), Times.Once);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/devices"), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }

        [Test]
        public async Task UpdateDeviceAsync_WithNewImage_ShouldUpdateWithImage()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string newImagePath = "uploads/devices/new_image.jpg";
            
            _mockFileStorageService.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/devices")).ReturnsAsync(newImagePath);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateDeviceAsync(deviceDto, mockFile.Object, false);

            // Assert
            Assert.That(result.DeviceImage, Is.EqualTo(newImagePath));
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }

        [Test]
        public async Task UpdateDeviceAsync_WithRemoveExistingImage_ShouldRemoveImage()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            deviceDto.DeviceImage = "existing_image.jpg";
            
            _mockFileStorageService.Setup(f => f.DeleteFileAsync("existing_image.jpg")).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateDeviceAsync(deviceDto, null, true);

            // Assert
            Assert.That(result.DeviceImage, Is.Null);
            _mockFileStorageService.Verify(f => f.DeleteFileAsync("existing_image.jpg"), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }

        [Test]
        public async Task UpdateDeviceAsync_WithoutImageChanges_ShouldSaveDeviceOnly()
        {
            // Arrange
            var deviceDto = CreateTestModel();
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InstalledDeviceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateDeviceAsync(deviceDto, null, false);

            // Assert
            Assert.That(result, Is.EqualTo(deviceDto));
            _mockFileStorageService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.SaveAsync(deviceDto), Times.Once);
        }
    }
} 