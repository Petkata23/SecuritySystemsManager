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
    public class ChatMessageServiceTests : BaseServiceTests<ChatMessageDto, IChatMessageRepository, ChatMessageService>
    {
        private Mock<IUserService> _mockUserService;

        [SetUp]
        public override void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            base.Setup();
        }

        protected override ChatMessageService CreateService(IChatMessageRepository repository)
        {
            return new ChatMessageService(repository, _mockUserService.Object);
        }

        protected override ChatMessageDto CreateTestModel(int id = 1)
        {
            return new ChatMessageDto
            {
                Id = id,
                SenderId = 1,
                RecipientId = 2,
                Message = "Test message",
                Timestamp = DateTime.UtcNow,
                IsFromSupport = false,
                IsRead = false
            };
        }

        [Test]
        public async Task GetMessagesByUserIdAsync_WithExistingMessages_ShouldReturnMessages()
        {
            // Arrange
            int userId = 1;
            var expectedMessages = CreateTestModels();
            _mockRepository.Setup(r => r.GetMessagesByUserIdAsync(userId)).ReturnsAsync(expectedMessages);

            // Act
            var result = await _service.GetMessagesByUserIdAsync(userId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedMessages));
            _mockRepository.Verify(r => r.GetMessagesByUserIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetMessagesByUserIdAsync_WithNoMessages_ShouldSendWelcomeMessage()
        {
            // Arrange
            int userId = 1;
            var emptyMessages = new List<ChatMessageDto>();
            var messagesWithWelcome = new List<ChatMessageDto> { CreateTestModel() };
            
            var sequence = new MockSequence();
            _mockRepository.InSequence(sequence).Setup(r => r.GetMessagesByUserIdAsync(userId)).ReturnsAsync(emptyMessages);
            _mockRepository.InSequence(sequence).Setup(r => r.GetMessagesByUserIdAsync(userId)).ReturnsAsync(messagesWithWelcome);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<ChatMessageDto>())).Returns(Task.CompletedTask);
            _mockUserService.Setup(s => s.ExistsByIdAsync(1)).ReturnsAsync(true); // System user exists

            // Act
            var result = await _service.GetMessagesByUserIdAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Empty);
            _mockRepository.Verify(r => r.GetMessagesByUserIdAsync(userId), Times.Exactly(2));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Once);
        }

        [Test]
        public async Task GetConversationAsync_WithValidUsers_ShouldReturnConversation()
        {
            // Arrange
            int userId1 = 1;
            int userId2 = 2;
            var expectedMessages = CreateTestModels();
            _mockRepository.Setup(r => r.GetConversationAsync(userId1, userId2)).ReturnsAsync(expectedMessages);

            // Act
            var result = await _service.GetConversationAsync(userId1, userId2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedMessages));
            _mockRepository.Verify(r => r.GetConversationAsync(userId1, userId2), Times.Once);
        }

        [Test]
        public async Task GetUnreadMessagesAsync_WithValidUserId_ShouldReturnUnreadMessages()
        {
            // Arrange
            int userId = 1;
            var expectedMessages = CreateTestModels();
            _mockRepository.Setup(r => r.GetUnreadMessagesAsync(userId)).ReturnsAsync(expectedMessages);

            // Act
            var result = await _service.GetUnreadMessagesAsync(userId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedMessages));
            _mockRepository.Verify(r => r.GetUnreadMessagesAsync(userId), Times.Once);
        }

        [Test]
        public async Task MarkAsReadAsync_WithValidMessageId_ShouldMarkMessageAsRead()
        {
            // Arrange
            int messageId = 1;
            _mockRepository.Setup(r => r.MarkAsReadAsync(messageId)).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(messageId);

            // Assert
            _mockRepository.Verify(r => r.MarkAsReadAsync(messageId), Times.Once);
        }

        [Test]
        public async Task MarkConversationAsReadAsync_WithValidUsers_ShouldMarkConversationAsRead()
        {
            // Arrange
            int userId1 = 1;
            int userId2 = 2;
            _mockRepository.Setup(r => r.MarkConversationAsReadAsync(userId1, userId2)).Returns(Task.CompletedTask);

            // Act
            await _service.MarkConversationAsReadAsync(userId1, userId2);

            // Assert
            _mockRepository.Verify(r => r.MarkConversationAsReadAsync(userId1, userId2), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_WithValidData_ShouldSendMessage()
        {
            // Arrange
            int senderId = 2; // Not system user
            int? recipientId = 3;
            string message = "Test message";
            _mockUserService.Setup(s => s.ExistsByIdAsync(senderId)).ReturnsAsync(true);
            _mockUserService.Setup(s => s.ExistsByIdAsync(recipientId.Value)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<ChatMessageDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.SendMessageAsync(senderId, recipientId, message);

            // Assert
            _mockUserService.Verify(s => s.ExistsByIdAsync(senderId), Times.Once);
            _mockUserService.Verify(s => s.ExistsByIdAsync(recipientId.Value), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_WithSystemUser_ShouldSendMessageWithoutValidation()
        {
            // Arrange
            int senderId = 1; // System user ID
            int? recipientId = 2;
            string message = "System message";
            _mockUserService.Setup(s => s.ExistsByIdAsync(recipientId.Value)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<ChatMessageDto>())).Returns(Task.CompletedTask);

            // Act
            await _service.SendMessageAsync(senderId, recipientId, message);

            // Assert
            _mockUserService.Verify(s => s.ExistsByIdAsync(senderId), Times.Never); // System user not validated
            _mockUserService.Verify(s => s.ExistsByIdAsync(recipientId.Value), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_WithInvalidSender_ShouldNotSendMessage()
        {
            // Arrange
            int senderId = 999;
            int? recipientId = 2;
            string message = "Test message";
            _mockUserService.Setup(s => s.ExistsByIdAsync(senderId)).ReturnsAsync(false);

            // Act
            await _service.SendMessageAsync(senderId, recipientId, message);

            // Assert
            _mockUserService.Verify(s => s.ExistsByIdAsync(senderId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Never);
        }

        [Test]
        public async Task SendMessageAsync_WithInvalidRecipient_ShouldNotSendMessage()
        {
            // Arrange
            int senderId = 2; // Not system user
            int? recipientId = 999;
            string message = "Test message";
            _mockUserService.Setup(s => s.ExistsByIdAsync(senderId)).ReturnsAsync(true);
            _mockUserService.Setup(s => s.ExistsByIdAsync(recipientId.Value)).ReturnsAsync(false);

            // Act
            await _service.SendMessageAsync(senderId, recipientId, message);

            // Assert
            _mockUserService.Verify(s => s.ExistsByIdAsync(senderId), Times.Once);
            _mockUserService.Verify(s => s.ExistsByIdAsync(recipientId.Value), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Never);
        }

        [Test]
        public async Task GetSupportUserIdsAsync_ShouldReturnSupportUserIds()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Role = new RoleDto { Name = "Admin" } },
                new UserDto { Id = 2, Role = new RoleDto { Name = "Manager" } },
                new UserDto { Id = 3, Role = new RoleDto { Name = "Client" } }
            };
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _service.GetSupportUserIdsAsync();

            // Assert
            Assert.That(result, Is.EqualTo(new List<int> { 1, 2 }));
            _mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task ProcessUserMessageAsync_WithValidData_ShouldProcessMessage()
        {
            // Arrange
            int userId = 1;
            string message = "Test message";
            var user = new UserDto { Id = userId, FirstName = "Test", LastName = "User" };
            _mockUserService.Setup(s => s.GetByIdIfExistsAsync(userId)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<ChatMessageDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ProcessUserMessageAsync(userId, message);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.SenderId, Is.EqualTo(userId));
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.IsFromSupport, Is.False);
            _mockUserService.Verify(s => s.GetByIdIfExistsAsync(userId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Once);
        }

        [Test]
        public async Task ProcessUserMessageAsync_WithInvalidUser_ShouldReturnNull()
        {
            // Arrange
            int userId = 999;
            string message = "Test message";
            _mockUserService.Setup(s => s.GetByIdIfExistsAsync(userId)).ReturnsAsync((UserDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _service.ProcessUserMessageAsync(userId, message));
            
            Assert.That(exception.Message, Is.EqualTo("User not found"));
            _mockUserService.Verify(s => s.GetByIdIfExistsAsync(userId), Times.Once);
        }

        [Test]
        public async Task ProcessSupportMessageAsync_WithValidData_ShouldProcessMessage()
        {
            // Arrange
            int senderId = 1;
            int recipientId = 2;
            string message = "Support message";
            var sender = new UserDto { Id = senderId, FirstName = "Support", LastName = "User" };
            _mockUserService.Setup(s => s.GetByIdIfExistsAsync(senderId)).ReturnsAsync(sender);
            _mockUserService.Setup(s => s.ExistsByIdAsync(senderId)).ReturnsAsync(true);
            _mockUserService.Setup(s => s.ExistsByIdAsync(recipientId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<ChatMessageDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ProcessSupportMessageAsync(senderId, recipientId, message);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.SenderId, Is.EqualTo(senderId));
            Assert.That(result.RecipientId, Is.EqualTo(recipientId));
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.IsFromSupport, Is.True);
            _mockUserService.Verify(s => s.GetByIdIfExistsAsync(senderId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<ChatMessageDto>()), Times.Once);
        }

        [Test]
        public async Task ProcessSupportMessageAsync_WithInvalidSender_ShouldThrowException()
        {
            // Arrange
            int senderId = 999;
            int recipientId = 2;
            string message = "Support message";
            _mockUserService.Setup(s => s.GetByIdIfExistsAsync(senderId)).ReturnsAsync((UserDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _service.ProcessSupportMessageAsync(senderId, recipientId, message));
            
            Assert.That(exception.Message, Is.EqualTo("Support user not found"));
            _mockUserService.Verify(s => s.GetByIdIfExistsAsync(senderId), Times.Once);
        }
    }
} 