using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class DropboxTokenManagerTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IServiceScope> _mockServiceScope;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<IDropboxTokenRepository> _mockTokenRepository;
        private DropboxTokenManager _tokenManager;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockTokenRepository = new Mock<IDropboxTokenRepository>();

            // Setup configuration
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.Value).Returns("test_value");

            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns("test_app_key");
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns("test_app_secret");
            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns("test_access_token");
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns("test_refresh_token");
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(DateTime.UtcNow.AddHours(1).ToString("O"));

            // Setup service provider
            _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockServiceScopeFactory.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IDropboxTokenRepository)))
                .Returns(_mockTokenRepository.Object);

            _tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenTokenIsValid_ReturnsCachedToken()
        {
            // Arrange
            var validToken = "valid_access_token";
            var expiryTime = DateTime.UtcNow.AddHours(2);

            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns(validToken);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(expiryTime.ToString("O"));

            _tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act
            var result = await _tokenManager.GetAccessTokenAsync();

            // Assert
            Assert.That(result, Is.EqualTo(validToken));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenTokenIsExpired_RefreshesToken()
        {
            // Arrange
            var expiredToken = "expired_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(-10); // Expired

            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns(expiredToken);
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns(refreshToken);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(expiryTime.ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((expiredToken, refreshToken, expiryTime));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Invalid Dropbox app credentials"));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenTokenFromDatabaseIsValid_ReturnsDatabaseToken()
        {
            // Arrange
            var dbToken = "test_access_token";
            var refreshToken = "test_refresh_token";
            var expiryTime = DateTime.UtcNow.AddHours(2);

            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(DateTime.UtcNow.AddMinutes(-10).ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((dbToken, refreshToken, expiryTime));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Invalid Dropbox app credentials"));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenNoValidTokenExists_ThrowsException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(DateTime.UtcNow.AddMinutes(-10).ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync(((string?)null, (string?)null, DateTime.UtcNow.AddMinutes(-10)));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Refresh token is missing"));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenTokenIsAboutToExpire_RefreshesToken()
        {
            // Arrange
            var token = "about_to_expire_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(3); // About to expire (within 5 minutes)

            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns(token);
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns(refreshToken);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(expiryTime.ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((token, refreshToken, expiryTime));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Invalid Dropbox app credentials"));
        }

        [Test]
        public void Constructor_WithValidConfiguration_InitializesCorrectly()
        {
            // Arrange & Act
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Assert
            Assert.That(tokenManager, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DropboxTokenManager(null, _mockServiceProvider.Object));
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DropboxTokenManager(_mockConfiguration.Object, null));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenRefreshTokenIsMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns((string)null);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(DateTime.UtcNow.AddMinutes(-10).ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync(((string?)null, (string?)null, DateTime.UtcNow.AddMinutes(-10)));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Refresh token is missing"));
        }

        [Test]
        public async Task GetAccessTokenAsync_WhenTokenRefreshFails_ThrowsException()
        {
            // Arrange
            var expiredToken = "expired_token";
            var refreshToken = "valid_refresh_token";
            var expiryTime = DateTime.UtcNow.AddMinutes(-10);

            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns(expiredToken);
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns(refreshToken);
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns(expiryTime.ToString("O"));

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync((expiredToken, refreshToken, expiryTime));

            // Create a new instance to avoid cached state
            var tokenManager = new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenManager.GetAccessTokenAsync());
            Assert.That(exception.Message, Contains.Substring("Invalid Dropbox app credentials"));
        }

        [Test]
        public async Task GetAccessTokenAsync_WithEmptyConfigurationValues_HandlesGracefully()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Dropbox:AppKey"]).Returns("");
            _mockConfiguration.Setup(x => x["Dropbox:AppSecret"]).Returns("");
            _mockConfiguration.Setup(x => x["Dropbox:AccessToken"]).Returns("");
            _mockConfiguration.Setup(x => x["Dropbox:RefreshToken"]).Returns("");
            _mockConfiguration.Setup(x => x["Dropbox:TokenExpiry"]).Returns("");

            _mockTokenRepository.Setup(x => x.GetLatestTokenAsync())
                .ReturnsAsync(((string?)null, (string?)null, DateTime.UtcNow.AddMinutes(-10)));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new DropboxTokenManager(_mockConfiguration.Object, _mockServiceProvider.Object));
        }
    }
} 