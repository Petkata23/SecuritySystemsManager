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
    public class NotificationRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private NotificationRepository _repository;
        private User _recipient;
        private Notification _testNotification;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _recipient = new User
            {
                Id = 1,
                UserName = "recipient@test.com",
                Email = "recipient@test.com",
                FirstName = "Test",
                LastName = "Recipient",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _testNotification = new Notification
            {
                Id = 1,
                RecipientId = _recipient.Id,
                Recipient = _recipient,
                Message = "This is a test notification message",
                DateSent = DateTime.Now,
                IsRead = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(_recipient);
            _context.Notifications.Add(_testNotification);
            _context.SaveChanges();

            _repository = new NotificationRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<Notification, NotificationDto>();
                cfg.CreateMap<NotificationDto, Notification>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnNotification()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testNotification.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testNotification.Id));
            Assert.That(result.Message, Is.EqualTo(_testNotification.Message));
            Assert.That(result.IsRead, Is.EqualTo(_testNotification.IsRead));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllNotifications()
        {
            // Arrange
            var secondNotification = new Notification
            {
                Id = 2,
                RecipientId = _recipient.Id,
                Recipient = _recipient,
                Message = "This is a second test notification",
                DateSent = DateTime.Now,
                IsRead = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Notifications.AddAsync(secondNotification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewNotification_ShouldSaveToDatabase()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                RecipientId = _recipient.Id,
                Message = "This is a new notification",
                IsRead = false
            };

            // Act
            await _repository.SaveAsync(notificationDto);

            // Assert
            var savedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Message == "This is a new notification");
            Assert.That(savedNotification, Is.Not.Null);
            Assert.That(savedNotification.Message, Is.EqualTo("This is a new notification"));
            Assert.That(savedNotification.RecipientId, Is.EqualTo(_recipient.Id));
        }

        [Test]
        public async Task SaveAsync_WithExistingNotification_ShouldUpdateNotification()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = _testNotification.Id,
                RecipientId = _testNotification.RecipientId,
                Message = "Updated notification message",
                IsRead = true
            };

            // Act
            await _repository.SaveAsync(notificationDto);

            // Assert
            var updatedNotification = await _context.Notifications.FindAsync(_testNotification.Id);
            Assert.That(updatedNotification, Is.Not.Null);
            Assert.That(updatedNotification.Message, Is.EqualTo("Updated notification message"));
            Assert.That(updatedNotification.IsRead, Is.True);
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveNotification()
        {
            // Act
            await _repository.DeleteAsync(_testNotification.Id);

            // Assert
            var deletedNotification = await _context.Notifications.FindAsync(_testNotification.Id);
            Assert.That(deletedNotification, Is.Null);
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
            var notifications = new List<Notification>();
            for (int i = 2; i <= 10; i++)
            {
                notifications.Add(new Notification
                {
                    Id = i,
                    RecipientId = _recipient.Id,
                    Recipient = _recipient,
                    Message = $"This is notification {i}",
                    DateSent = DateTime.Now,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.Notifications.AddRangeAsync(notifications);
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
        public async Task SaveAsync_WithNotificationHavingAllFields_ShouldSaveCorrectly()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                RecipientId = _recipient.Id,
                Message = "This notification has all fields filled",
                IsRead = false
            };

            // Act
            await _repository.SaveAsync(notificationDto);

            // Assert
            var savedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Message == "This notification has all fields filled");
            Assert.That(savedNotification, Is.Not.Null);
            Assert.That(savedNotification.Message, Is.EqualTo("This notification has all fields filled"));
            Assert.That(savedNotification.RecipientId, Is.EqualTo(_recipient.Id));
            Assert.That(savedNotification.IsRead, Is.False);
        }
    }
} 