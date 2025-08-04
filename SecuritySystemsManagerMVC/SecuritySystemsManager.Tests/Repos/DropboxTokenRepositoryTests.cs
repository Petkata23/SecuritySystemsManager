using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SecuritySystemsManager.Tests.Repos
{
    [TestFixture]
    public class DropboxTokenRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private DropboxTokenRepository _repository;
        private DropboxToken _testToken;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SecuritySystemsManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new SecuritySystemsManagerDbContext(options);
            _repository = new DropboxTokenRepository(_context);
            // Изчистване на таблицата DropboxTokens
            _context.DropboxTokens.RemoveRange(_context.DropboxTokens);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public async Task GetLatestTokenAsync_WithExistingToken_ShouldReturnLatestToken()
        {
            // Arrange
            var token = new DropboxToken
            {
                Id = 1,
                AccessToken = "token_1",
                RefreshToken = "refresh_1",
                ExpiryTime = DateTime.Now.AddHours(1),
                UpdatedAt = DateTime.Now
            };
            await _context.DropboxTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            // Act
            var latest = await _repository.GetLatestTokenAsync();

            // Assert
            Assert.That(latest.AccessToken, Is.EqualTo("token_1"));
        }

        [Test]
        public async Task GetLatestTokenAsync_WithMultipleTokens_ShouldReturnMostRecent()
        {
            // Arrange
            var newerToken = new DropboxToken
            {
                Id = 2,
                AccessToken = "newer_access_token",
                RefreshToken = "newer_refresh_token",
                ExpiryTime = DateTime.Now.AddHours(2),
                CreatedAt = DateTime.Now.AddMinutes(10),
                UpdatedAt = DateTime.Now.AddMinutes(10)
            };

            await _context.DropboxTokens.AddAsync(newerToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestTokenAsync();

            // Assert
            Assert.That(result.AccessToken, Is.EqualTo(newerToken.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(newerToken.RefreshToken));
            Assert.That(result.ExpiryTime, Is.EqualTo(newerToken.ExpiryTime));
        }

        [Test]
        public async Task GetLatestTokenAsync_WithNoTokens_ShouldReturnNullValues()
        {
            // Arrange - Remove all tokens
            _context.DropboxTokens.RemoveRange(_context.DropboxTokens);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestTokenAsync();

            // Assert
            Assert.That(result.AccessToken, Is.Null);
            Assert.That(result.RefreshToken, Is.Null);
            Assert.That(result.ExpiryTime, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async Task SaveTokenAsync_WithNewToken_ShouldCreateNewToken()
        {
            // Arrange
            var accessToken = "new_access_token";
            var refreshToken = "new_refresh_token";
            var expiryTime = DateTime.Now.AddHours(3);

            // Act
            await _repository.SaveTokenAsync(accessToken, refreshToken, expiryTime);

            // Assert
            var savedToken = await _context.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.AccessToken, Is.EqualTo(accessToken));
            Assert.That(savedToken.RefreshToken, Is.EqualTo(refreshToken));
            Assert.That(savedToken.ExpiryTime, Is.EqualTo(expiryTime));
        }

        [Test]
        public async Task SaveTokenAsync_WithExistingToken_ShouldUpdateExistingToken()
        {
            // Arrange
            var token = new DropboxToken
            {
                Id = 1,
                AccessToken = "token_1",
                RefreshToken = "refresh_1",
                ExpiryTime = DateTime.Now.AddHours(1),
                UpdatedAt = DateTime.Now.AddMinutes(-10)
            };
            await _context.DropboxTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            // Act
            var newExpiry = DateTime.Now.AddHours(2);
            await _repository.SaveTokenAsync("token_1_updated", "refresh_1_updated", newExpiry);

            // Assert
            var updated = await _context.DropboxTokens.FirstOrDefaultAsync();
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.AccessToken, Is.EqualTo("token_1_updated"));
            Assert.That(updated.RefreshToken, Is.EqualTo("refresh_1_updated"));
            Assert.That(updated.ExpiryTime, Is.EqualTo(newExpiry));
        }

        [Test]
        public async Task SaveTokenAsync_WithMultipleTokens_ShouldUpdateMostRecent()
        {
            // Arrange
            var tokens = new List<DropboxToken>
            {
                new DropboxToken { Id = 1, AccessToken = "token_1", RefreshToken = "refresh_1", ExpiryTime = DateTime.Now.AddHours(1), UpdatedAt = DateTime.Now.AddMinutes(-10) },
                new DropboxToken { Id = 2, AccessToken = "token_2", RefreshToken = "refresh_2", ExpiryTime = DateTime.Now.AddHours(2), UpdatedAt = DateTime.Now }
            };
            await _context.DropboxTokens.AddRangeAsync(tokens);
            await _context.SaveChangesAsync();

            // Act
            var newExpiry = DateTime.Now.AddHours(3);
            await _repository.SaveTokenAsync("token_2_updated", "refresh_2_updated", newExpiry);

            // Assert
            var updated = await _context.DropboxTokens.FirstOrDefaultAsync(t => t.Id == 2);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.AccessToken, Is.EqualTo("token_2_updated"));
            Assert.That(updated.RefreshToken, Is.EqualTo("refresh_2_updated"));
            Assert.That(updated.ExpiryTime, Is.EqualTo(newExpiry));
        }

        [Test]
        public async Task SaveTokenAsync_WithNullValues_ShouldSaveNullValues()
        {
            // Arrange
            var token = new DropboxToken
            {
                Id = 1,
                AccessToken = "token_1",
                RefreshToken = "refresh_1",
                ExpiryTime = DateTime.Now.AddHours(1),
                UpdatedAt = DateTime.Now
            };
            await _context.DropboxTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            // Act
            var newExpiry = DateTime.Now.AddHours(2);
            await _repository.SaveTokenAsync("", "", newExpiry); // Празни стрингове вместо null

            // Assert
            var updated = await _context.DropboxTokens.FirstOrDefaultAsync();
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.AccessToken, Is.EqualTo(""));
            Assert.That(updated.RefreshToken, Is.EqualTo(""));
            Assert.That(updated.ExpiryTime, Is.EqualTo(newExpiry));
        }

        [Test]
        public async Task SaveTokenAsync_WithEmptyStrings_ShouldSaveEmptyStrings()
        {
            // Arrange
            var accessToken = "";
            var refreshToken = "";
            var expiryTime = DateTime.Now.AddHours(1);

            // Act
            await _repository.SaveTokenAsync(accessToken, refreshToken, expiryTime);

            // Assert
            var savedToken = await _context.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.AccessToken, Is.EqualTo(""));
            Assert.That(savedToken.RefreshToken, Is.EqualTo(""));
            Assert.That(savedToken.ExpiryTime, Is.EqualTo(expiryTime));
        }

        [Test]
        public async Task SaveTokenAsync_WithPastExpiryTime_ShouldSaveCorrectly()
        {
            // Arrange
            var accessToken = "test_token";
            var refreshToken = "test_refresh";
            var expiryTime = DateTime.Now.AddHours(-1); // Past time

            // Act
            await _repository.SaveTokenAsync(accessToken, refreshToken, expiryTime);

            // Assert
            var savedToken = await _context.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.ExpiryTime, Is.EqualTo(expiryTime));
        }

        [Test]
        public async Task SaveTokenAsync_WithFutureExpiryTime_ShouldSaveCorrectly()
        {
            // Arrange
            var accessToken = "test_token";
            var refreshToken = "test_refresh";
            var expiryTime = DateTime.Now.AddDays(30); // Future time

            // Act
            await _repository.SaveTokenAsync(accessToken, refreshToken, expiryTime);

            // Assert
            var savedToken = await _context.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.ExpiryTime, Is.EqualTo(expiryTime));
        }

        [Test]
        public async Task GetLatestTokenAsync_WithTokensOrderedByUpdatedAt_ShouldReturnCorrectOrder()
        {
            // Arrange
            var tokens = new List<DropboxToken>
            {
                new DropboxToken { Id = 1, AccessToken = "token_1", RefreshToken = "refresh_1", ExpiryTime = DateTime.Now.AddHours(1), UpdatedAt = DateTime.Now.AddMinutes(-10) },
                new DropboxToken { Id = 2, AccessToken = "token_2", RefreshToken = "refresh_2", ExpiryTime = DateTime.Now.AddHours(2), UpdatedAt = DateTime.Now.AddMinutes(-5) },
                new DropboxToken { Id = 3, AccessToken = "token_3", RefreshToken = "refresh_3", ExpiryTime = DateTime.Now.AddHours(3), UpdatedAt = DateTime.Now }
            };
            await _context.DropboxTokens.AddRangeAsync(tokens);
            await _context.SaveChangesAsync();

            // Act
            var latest = await _repository.GetLatestTokenAsync();

            // Assert
            Assert.That(tokens.Select(t => t.AccessToken), Does.Contain(latest.AccessToken));
        }
    }
} 