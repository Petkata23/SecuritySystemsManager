using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests : BaseServiceTests<NotificationDto, INotificationRepository, NotificationService>
    {
        private Mock<IUserRepository> _mockUserRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockUserRepository = new Mock<IUserRepository>();
            _service = new NotificationService(_mockRepository.Object, _mockUserRepository.Object);
        }

        protected override NotificationService CreateService(INotificationRepository repository)
        {
            return new NotificationService(repository, _mockUserRepository?.Object ?? new Mock<IUserRepository>().Object);
        }

        protected override NotificationDto CreateTestModel(int id = 1)
        {
            return new NotificationDto
            {
                Id = id,
                RecipientId = id,
                Message = $"Message for notification {id}",
                DateSent = System.DateTime.UtcNow,
                IsRead = false
            };
        }

        [Test]
        public async Task WhenSaveAsync_WithValidNotificationData_ThenSaveAsync()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                RecipientId = 1,
                Message = "Test notification message",
                DateSent = System.DateTime.UtcNow,
                IsRead = false
            };

            // Act
            await _service.SaveAsync(notificationDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(notificationDto), Times.Once());
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
        public async Task WhenGetByIdAsync_WithValidNotificationId_ThenReturnNotification(int notificationId)
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = notificationId,
                RecipientId = 1,
                Message = "Test notification",
                DateSent = System.DateTime.UtcNow,
                IsRead = false
            };

            _mockRepository.Setup(x => x.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(notificationId))))
                .ReturnsAsync(notificationDto);

            // Act
            var notificationResult = await _service.GetByIdIfExistsAsync(notificationId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(notificationId), Times.Once());
            Assert.That(notificationResult, Is.EqualTo(notificationDto));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(102021)]
        public async Task WhenGetByIdAsync_WithInvalidNotificationId_ThenReturnDefault(int notificationId)
        {
            // Arrange
            var notification = (NotificationDto)default;

            _mockRepository.Setup(s => s.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(notificationId))))
                .ReturnsAsync(notification);

            // Act
            var notificationResult = await _service.GetByIdIfExistsAsync(notificationId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(notificationId), Times.Once());
            Assert.That(notificationResult, Is.EqualTo(notification));
        }

        [Test]
        public async Task WhenUpdateAsync_WithValidData_ThenSaveAsync()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = 1,
                RecipientId = 1,
                Message = "Updated notification message",
                DateSent = System.DateTime.UtcNow,
                IsRead = true
            };

            _mockRepository.Setup(s => s.SaveAsync(It.Is<NotificationDto>(x => x.Equals(notificationDto))))
                .Verifiable();

            // Act
            await _service.SaveAsync(notificationDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(notificationDto), Times.Once());
        }

        [Test]
        public async Task WhenGetAllAsync_ThenReturnAllNotifications()
        {
            // Arrange
            var notificationList = new List<NotificationDto>
            {
                new NotificationDto { Id = 1, Message = "First notification", IsRead = false },
                new NotificationDto { Id = 2, Message = "Second notification", IsRead = true },
                new NotificationDto { Id = 3, Message = "Third notification", IsRead = false }
            };

            _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(notificationList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            _mockRepository.Verify(x => x.GetAllAsync(), Times.Once());
            Assert.That(result, Is.EqualTo(notificationList));
        }

        [Test]
        public async Task GetNotificationsForUserAsync_WithValidUserId_ShouldReturnNotifications()
        {
            // Arrange
            int userId = 1;
            var allNotifications = CreateTestModels();
            allNotifications[0].RecipientId = userId;
            allNotifications[1].RecipientId = userId;
            allNotifications[2].RecipientId = 2; // Different user
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allNotifications);

            // Act
            var result = await _service.GetNotificationsForUserAsync(userId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(n => n.RecipientId == userId), Is.True);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetNotificationsForUserAsync_WithNoNotifications_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            var emptyNotifications = new List<NotificationDto>();
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyNotifications);

            // Act
            var result = await _service.GetNotificationsForUserAsync(userId);

            // Assert
            Assert.That(result, Is.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetNotificationsForUserAsync_WithNoMatchingNotifications_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            var notifications = CreateTestModels();
            notifications[0].RecipientId = 2; // Different user
            notifications[1].RecipientId = 3; // Different user
            notifications[2].RecipientId = 4; // Different user
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(notifications);

            // Act
            var result = await _service.GetNotificationsForUserAsync(userId);

            // Assert
            Assert.That(result, Is.Empty);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task GetNotificationsForUserAsync_WithDifferentUserIds_ShouldReturnCorrectNotifications(int userId)
        {
            // Arrange
            var allNotifications = CreateTestModels();
            allNotifications[0].RecipientId = userId;
            allNotifications[1].RecipientId = userId;
            allNotifications[2].RecipientId = userId + 1; // Different user
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allNotifications);

            // Act
            var result = await _service.GetNotificationsForUserAsync(userId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(n => n.RecipientId == userId), Is.True);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task MarkAsReadAsync_WithValidNotificationAndUser_ShouldMarkAsRead()
        {
            // Arrange
            int notificationId = 1;
            int userId = 1;
            var notification = CreateTestModel(notificationId);
            notification.RecipientId = userId;
            notification.IsRead = false;
            
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(notificationId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.IsRead)), Times.Once);
        }

        [Test]
        public async Task MarkAsReadAsync_WithInvalidUser_ShouldNotMarkAsRead()
        {
            // Arrange
            int notificationId = 1;
            int userId = 1;
            int differentUserId = 2;
            var notification = CreateTestModel(notificationId);
            notification.RecipientId = differentUserId;
            
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            // Act
            await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(notificationId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Test]
        public async Task MarkAsReadAsync_WithNonExistentNotification_ShouldNotMarkAsRead()
        {
            // Arrange
            int notificationId = 999;
            int userId = 1;
            
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync((NotificationDto)null);

            // Act
            await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(notificationId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Test]
        public async Task MarkAsReadAsync_WithAlreadyReadNotification_ShouldNotMarkAsRead()
        {
            // Arrange
            int notificationId = 1;
            int userId = 1;
            var notification = CreateTestModel(notificationId);
            notification.RecipientId = userId;
            notification.IsRead = true; // Already read
            
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            // Act
            await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(notificationId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Theory]
        [TestCase(1, 1)]
        [TestCase(5, 2)]
        [TestCase(100, 10)]
        public async Task MarkAsReadAsync_WithDifferentNotificationAndUserIds_ShouldMarkAsRead(int notificationId, int userId)
        {
            // Arrange
            var notification = CreateTestModel(notificationId);
            notification.RecipientId = userId;
            notification.IsRead = false;
            
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(notificationId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.IsRead)), Times.Once);
        }

        [Test]
        public async Task MarkAllAsReadAsync_WithValidUserId_ShouldMarkAllAsRead()
        {
            // Arrange
            int userId = 1;
            var notifications = CreateTestModels();
            notifications[0].RecipientId = userId;
            notifications[0].IsRead = false;
            notifications[1].RecipientId = userId;
            notifications[1].IsRead = false;
            notifications[2].RecipientId = userId;
            notifications[2].IsRead = true; // Already read
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(notifications);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Exactly(2));
        }

        [Test]
        public async Task MarkAllAsReadAsync_WithNoUnreadNotifications_ShouldNotSaveAny()
        {
            // Arrange
            int userId = 1;
            var notifications = CreateTestModels();
            notifications[0].RecipientId = userId;
            notifications[0].IsRead = true; // Already read
            notifications[1].RecipientId = userId;
            notifications[1].IsRead = true; // Already read
            notifications[2].RecipientId = userId;
            notifications[2].IsRead = true; // Already read
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(notifications);

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Test]
        public async Task MarkAllAsReadAsync_WithNoNotifications_ShouldNotSaveAny()
        {
            // Arrange
            int userId = 1;
            var emptyNotifications = new List<NotificationDto>();
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyNotifications);

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task MarkAllAsReadAsync_WithDifferentUserIds_ShouldMarkAllUnreadNotifications(int userId)
        {
            // Arrange
            var notifications = CreateTestModels();
            notifications[0].RecipientId = userId;
            notifications[0].IsRead = false;
            notifications[1].RecipientId = userId;
            notifications[1].IsRead = false;
            notifications[2].RecipientId = userId + 1; // Different user
            notifications[2].IsRead = false;
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(notifications);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendOrderStatusChangeNotificationAsync_WithValidData_ShouldCreateNotification()
        {
            // Arrange
            int orderId = 1;
            int clientId = 1;
            var client = new UserDto { Id = clientId, Username = "testuser" };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, OrderStatus.Pending, OrderStatus.InProgress);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.RecipientId == clientId)), Times.Once);
        }

        [Test]
        public async Task SendOrderStatusChangeNotificationAsync_WithInvalidClient_ShouldReturnFalse()
        {
            // Arrange
            int orderId = 1;
            int clientId = 999;
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((UserDto)null);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, OrderStatus.Pending, OrderStatus.InProgress);

            // Assert
            Assert.That(result, Is.False);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<NotificationDto>()), Times.Never);
        }

        [Test]
        public async Task SendOrderStatusChangeNotificationAsync_WithSameStatus_ShouldCreateNotification()
        {
            // Arrange
            int orderId = 1;
            int clientId = 1;
            var client = new UserDto { Id = clientId, Username = "testuser" };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, OrderStatus.Pending, OrderStatus.Pending);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.RecipientId == clientId)), Times.Once);
        }

        [Test]
        public async Task SendOrderStatusChangeNotificationAsync_WithZeroOrderId_ShouldCreateNotification()
        {
            // Arrange
            int orderId = 0;
            int clientId = 1;
            var client = new UserDto { Id = clientId, Username = "testuser" };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, OrderStatus.Pending, OrderStatus.InProgress);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.RecipientId == clientId)), Times.Once);
        }

        [Test]
        public async Task SendOrderStatusChangeNotificationAsync_WithNegativeOrderId_ShouldCreateNotification()
        {
            // Arrange
            int orderId = -1;
            int clientId = 1;
            var client = new UserDto { Id = clientId, Username = "testuser" };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, OrderStatus.Pending, OrderStatus.InProgress);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.RecipientId == clientId)), Times.Once);
        }

        [Theory]
        [TestCase(1, 1, OrderStatus.Pending, OrderStatus.InProgress)]
        [TestCase(5, 10, OrderStatus.InProgress, OrderStatus.Completed)]
        [TestCase(100, 50, OrderStatus.Completed, OrderStatus.Cancelled)]
        public async Task SendOrderStatusChangeNotificationAsync_WithDifferentOrderStatuses_ShouldCreateNotification(int orderId, int clientId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            // Arrange
            var client = new UserDto { Id = clientId, Username = "testuser" };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SendOrderStatusChangeNotificationAsync(orderId, clientId, oldStatus, newStatus);

            // Assert
            Assert.That(result, Is.True);
            _mockUserRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.Is<NotificationDto>(n => n.RecipientId == clientId)), Times.Once);
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
        public async Task WhenGetWithPaginationAsync_WithValidParameters_ThenReturnPaginatedNotifications(int pageSize, int pageNumber)
        {
            // Arrange
            var notificationList = new List<NotificationDto>
            {
                new NotificationDto { Id = 1, Message = "First notification" },
                new NotificationDto { Id = 2, Message = "Second notification" }
            };

            _mockRepository.Setup(x => x.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(notificationList);

            // Act
            var result = await _service.GetWithPaginationAsync(pageSize, pageNumber);

            // Assert
            _mockRepository.Verify(x => x.GetWithPaginationAsync(pageSize, pageNumber), Times.Once());
            Assert.That(result, Is.EqualTo(notificationList));
        }
    }
} 