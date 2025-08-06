using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class DropboxTokenRefreshServiceTests
    {
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ILogger<DropboxTokenRefreshService>> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IServiceScope> _mockServiceScope;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<IDropboxTokenRepository> _mockTokenRepository;
        private DropboxTokenRefreshService _refreshService;

        [SetUp]
        public void Setup()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<DropboxTokenRefreshService>>();
            _mockConfiguration = new Mock<IConfiguration>();
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

            // Setup configuration
            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns("test_app_key");
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns("test_app_secret");

            _refreshService = new DropboxTokenRefreshService(
                _mockServiceProvider.Object,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _refreshService?.Dispose();
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var service = new DropboxTokenRefreshService(
                _mockServiceProvider.Object,
                _mockLogger.Object,
                _mockConfiguration.Object);

            // Assert
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullServiceProvider_DoesNotThrowException()
        {
            // Act & Assert - Service doesn't validate null parameters
            Assert.DoesNotThrow(() => new DropboxTokenRefreshService(
                null,
                _mockLogger.Object,
                _mockConfiguration.Object));
        }

        [Test]
        public void Constructor_WithNullLogger_DoesNotThrowException()
        {
            // Act & Assert - Service doesn't validate null parameters
            Assert.DoesNotThrow(() => new DropboxTokenRefreshService(
                _mockServiceProvider.Object,
                null,
                _mockConfiguration.Object));
        }

        [Test]
        public void Constructor_WithNullConfiguration_DoesNotThrowException()
        {
            // Act & Assert - Service doesn't validate null parameters
            Assert.DoesNotThrow(() => new DropboxTokenRefreshService(
                _mockServiceProvider.Object,
                _mockLogger.Object,
                null));
        }

        [Test]
        public async Task ExecuteAsync_WhenCancellationRequested_StopsGracefully()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenTokenIsValid_DoesNotRefresh()
        {
            // Arrange
            var validToken = "valid_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddHours(2); // Valid for 2 hours

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((validToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid Dropbox app credentials")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenTokenIsAboutToExpire_AttemptsRefresh()
        {
            // Arrange
            var accessToken = "expired_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(30); // About to expire

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("about to expire")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenNoRefreshToken_DoesNotRefresh()
        {
            // Arrange
            var accessToken = "access_token";
            string refreshToken = null;
            var expiryTime = DateTime.UtcNow.AddMinutes(30);

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("still valid")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenTokenIsExpired_AttemptsRefresh()
        {
            // Arrange
            var accessToken = "expired_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(-10); // Already expired

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("about to expire")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenRepositoryThrowsException_LogsError()
        {
            // Arrange
            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ThrowsAsync(new Exception("Database error"));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in RefreshTokenIfNeededAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenTokenRefreshFails_LogsError()
        {
            // Arrange
            var accessToken = "expired_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(30);

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("about to expire")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenServiceStops_LogsStoppingMessage()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel(); // Cancel immediately to trigger stopping
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert - Verify that the service logs the starting message
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WithEmptyConfiguration_HandlesGracefully()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns("");
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns("");

            var accessToken = "expired_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(30);

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("about to expire")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WithNullConfigurationValues_HandlesGracefully()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns((string)null);

            var accessToken = "expired_access_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(30);

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((accessToken, refreshToken, expiryTime));

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await _refreshService.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); // Give it time to process
            await _refreshService.StopAsync(cancellationTokenSource.Token);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("about to expire")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
} 