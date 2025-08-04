using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class DropboxStorageServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IServiceScope> _mockServiceScope;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<IDropboxTokenRepository> _mockTokenRepository;
        private DropboxTokenManager _tokenManager;
        private DropboxStorageService _storageService;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockTokenRepository = new Mock<IDropboxTokenRepository>();

            // Setup service provider
            _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockServiceScopeFactory.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IDropboxTokenRepository)))
                .Returns(_mockTokenRepository.Object);

            _mockConfiguration.Setup(x => x["Dropbox:RootFolder"]).Returns("TestRootFolder");
            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns("test_app_key");
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns("test_app_secret");

            // Setup token repository to return valid tokens
            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync(("test_access_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Create real token manager instance
            _tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);
            _storageService = new DropboxStorageService(_mockConfiguration.Object, _tokenManager);
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var service = new DropboxStorageService(_mockConfiguration.Object, _tokenManager);

            // Assert
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DropboxStorageService(null, _tokenManager));
        }

        [Test]
        public void Constructor_WithNullTokenManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DropboxStorageService(_mockConfiguration.Object, null));
        }

        [Test]
        public void Constructor_WithDefaultRootFolder_UsesDefaultValue()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:RootFolder"]).Returns((string)null);

            // Act
            var service = new DropboxStorageService(_mockConfiguration.Object, _tokenManager);

            // Assert
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public async Task UploadFileAsync_WithNullFile_ReturnsNull()
        {
            // Arrange
            IFormFile nullFile = null;

            // Act
            var result = await _storageService.UploadFileAsync(nullFile, "test-folder");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UploadFileAsync_WithEmptyFile_ReturnsNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _storageService.UploadFileAsync(mockFile.Object, "test-folder");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UploadFileAsync_WithValidFile_ThrowsExceptionDueToDropboxClient()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act & Assert
            Assert.ThrowsAsync<Dropbox.Api.BadInputException>(async () => await _storageService.UploadFileAsync(mockFile.Object, "test-folder"));
        }

        [Test]
        public async Task DeleteFileAsync_WithNullPath_ReturnsFalse()
        {
            // Arrange
            string nullPath = null;

            // Act
            var result = await _storageService.DeleteFileAsync(nullPath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteFileAsync_WithEmptyPath_ReturnsFalse()
        {
            // Arrange
            string emptyPath = "";

            // Act
            var result = await _storageService.DeleteFileAsync(emptyPath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteFileAsync_WithWhitespacePath_ReturnsFalse()
        {
            // Arrange
            string whitespacePath = "   ";

            // Act
            var result = await _storageService.DeleteFileAsync(whitespacePath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteFileAsync_WithProxyUrl_ReturnsTrue()
        {
            // Arrange
            string proxyUrl = "/imageproxy?url=test.jpg";

            // Act
            var result = await _storageService.DeleteFileAsync(proxyUrl);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteFileAsync_WithRegularPath_ReturnsFalse()
        {
            // Arrange
            string regularPath = "/test/file.jpg";

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _storageService.DeleteFileAsync(regularPath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UploadFileAsync_WithValidFileAndNullFolder_HandlesGracefully()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act & Assert
            Assert.ThrowsAsync<Dropbox.Api.BadInputException>(async () => await _storageService.UploadFileAsync(mockFile.Object, null));
        }

        [Test]
        public async Task UploadFileAsync_WithValidFileAndEmptyFolder_HandlesGracefully()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act & Assert
            Assert.ThrowsAsync<Dropbox.Api.BadInputException>(async () => await _storageService.UploadFileAsync(mockFile.Object, ""));
        }

        [Test]
        public async Task UploadFileAsync_WithFileHavingNullFileName_HandlesGracefully()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns((string)null);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act & Assert
            Assert.ThrowsAsync<Dropbox.Api.BadInputException>(async () => await _storageService.UploadFileAsync(mockFile.Object, "test-folder"));
        }

        [Test]
        public async Task UploadFileAsync_WithFileHavingEmptyFileName_HandlesGracefully()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act & Assert
            Assert.ThrowsAsync<Dropbox.Api.BadInputException>(async () => await _storageService.UploadFileAsync(mockFile.Object, "test-folder"));
        }

        [Test]
        public async Task DeleteFileAsync_WithDropboxUrlContainingFileName_ReturnsFalse()
        {
            // Arrange
            string dropboxUrl = "https://www.dropbox.com/s/test/file.jpg?dl=0";

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _storageService.DeleteFileAsync(dropboxUrl);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteFileAsync_WithDropboxUrlContainingQueryParameters_ReturnsFalse()
        {
            // Arrange
            string dropboxUrl = "https://www.dropbox.com/s/test/file.jpg?dl=0&rev=123";

            _mockTokenRepository.Setup(t => t.GetLatestTokenAsync())
                .ReturnsAsync(("test_token", "test_refresh_token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _storageService.DeleteFileAsync(dropboxUrl);

            // Assert
            Assert.That(result, Is.False);
        }
    }
} 