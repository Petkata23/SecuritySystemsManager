using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public class ChatMessageRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private ChatMessageRepository _repository;
        private User _sender;
        private User _recipient;
        private User _supportUser;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _sender = new User
            {
                Id = 1,
                UserName = "sender@test.com",
                Email = "sender@test.com",
                FirstName = "Sender",
                LastName = "User"
            };

            _recipient = new User
            {
                Id = 2,
                UserName = "recipient@test.com",
                Email = "recipient@test.com",
                FirstName = "Recipient",
                LastName = "User"
            };

            _supportUser = new User
            {
                Id = 3,
                UserName = "support@test.com",
                Email = "support@test.com",
                FirstName = "Support",
                LastName = "User"
            };

            _context.Users.AddRange(_sender, _recipient, _supportUser);
            _context.SaveChanges();

            _repository = new ChatMessageRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<ChatMessage, ChatMessageDto>();
                cfg.CreateMap<ChatMessageDto, ChatMessage>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task SaveAsync_WithValidChatMessage_ShouldSaveToDatabase()
        {
            // Arrange
            var chatMessageDto = new ChatMessageDto
            {
                Message = "Test message",
                SenderId = _sender.Id,
                SenderName = _sender.UserName,
                RecipientId = _recipient.Id,
                RecipientName = _recipient.UserName,
                Timestamp = DateTime.Now,
                IsFromSupport = false
            };

            // Act
            await _repository.SaveAsync(chatMessageDto);

            // Assert
            var savedMessage = await _context.ChatMessages.FirstOrDefaultAsync();
            Assert.That(savedMessage, Is.Not.Null);
            Assert.That(savedMessage.Message, Is.EqualTo("Test message"));
            Assert.That(savedMessage.SenderId, Is.EqualTo(_sender.Id));
            Assert.That(savedMessage.RecipientId, Is.EqualTo(_recipient.Id));
        }

        [Test]
        public async Task SaveAsync_WithNullSenderName_ShouldSetSenderNameFromDatabase()
        {
            // Arrange
            var chatMessageDto = new ChatMessageDto
            {
                Message = "Test message",
                SenderId = _sender.Id,
                SenderName = null, // Will be set from database
                RecipientId = _recipient.Id,
                Timestamp = DateTime.Now
            };

            // Act
            await _repository.SaveAsync(chatMessageDto);

            // Assert
            var savedMessage = await _context.ChatMessages.FirstOrDefaultAsync();
            Assert.That(savedMessage, Is.Not.Null);
            Assert.That(savedMessage.SenderName, Is.EqualTo(_sender.UserName));
        }

        [Test]
        public async Task GetMessagesByUserIdAsync_ShouldReturnAllMessagesForUser()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Message 1",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Message 2",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Message 3",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _supportUser.Id,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMessagesByUserIdAsync(_sender.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetConversationAsync_ShouldReturnMessagesBetweenTwoUsers()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Message from sender to recipient",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Reply from recipient to sender",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Message to different user",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _supportUser.Id,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetConversationAsync(_sender.Id, _recipient.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetUnreadMessagesAsync_ShouldReturnOnlyUnreadMessages()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Unread message 1",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Read message",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = true,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Unread message 2",
                    SenderId = _supportUser.Id,
                    SenderName = _supportUser.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadMessagesAsync(_recipient.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetUnreadMessagesAsync_ShouldIncludeSupportMessages()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Support message",
                    SenderId = _supportUser.Id,
                    SenderName = _supportUser.UserName,
                    RecipientId = null, // General support message
                    IsRead = false,
                    IsFromSupport = true,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadMessagesAsync(_recipient.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task MarkAsReadAsync_ShouldMarkMessageAsRead()
        {
            // Arrange
            var message = new ChatMessage
            {
                Message = "Test message",
                SenderId = _sender.Id,
                SenderName = _sender.UserName,
                RecipientId = _recipient.Id,
                IsRead = false,
                Timestamp = DateTime.Now
            };

            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            await _repository.MarkAsReadAsync(message.Id);

            // Assert
            var updatedMessage = await _context.ChatMessages.FindAsync(message.Id);
            Assert.That(updatedMessage.IsRead, Is.True);
            Assert.That(updatedMessage.ReadAt, Is.Not.Null);
        }

        [Test]
        public async Task MarkAsReadAsync_WithNonExistentMessage_ShouldNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _repository.MarkAsReadAsync(999));
        }

        [Test]
        public async Task MarkConversationAsReadAsync_ShouldMarkAllMessagesInConversationAsRead()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Message 1",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Message 2",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Message 3",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            await _repository.MarkConversationAsReadAsync(_sender.Id, _recipient.Id);

            // Assert
            var updatedMessages = await _context.ChatMessages
                .Where(m => m.RecipientId == _sender.Id && 
                           (m.SenderId == _recipient.Id || m.SenderId == _sender.Id))
                .ToListAsync();

            Assert.That(updatedMessages.All(m => m.IsRead), Is.True);
            Assert.That(updatedMessages.All(m => m.ReadAt.HasValue), Is.True);
        }

        [Test]
        public async Task GetUnreadCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Unread message 1",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Read message",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = true,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Unread message 2",
                    SenderId = _supportUser.Id,
                    SenderName = _supportUser.UserName,
                    RecipientId = _recipient.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadCountAsync(_recipient.Id);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserConversationsAsync_ShouldReturnConversationsForUser()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Message to recipient",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Reply from recipient",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Message to support",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _supportUser.Id,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserConversationsAsync(_sender.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(c => c.UserId == _recipient.Id), Is.True);
            Assert.That(result.Any(c => c.UserId == _supportUser.Id), Is.True);
        }

        [Test]
        public async Task GetUserConversationsAsync_ShouldOrderByLastMessageTime()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Older message",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Newer message",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _supportUser.Id,
                    Timestamp = DateTime.Now.AddHours(-1)
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserConversationsAsync(_sender.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].LastMessageTime, Is.GreaterThan(result[1].LastMessageTime));
        }

        [Test]
        public async Task GetUserConversationsAsync_ShouldCalculateUnreadCount()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Unread message 1",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Unread message 2",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    IsRead = false,
                    Timestamp = DateTime.Now.AddHours(-1)
                },
                new ChatMessage
                {
                    Message = "Read message",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    IsRead = true,
                    Timestamp = DateTime.Now
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserConversationsAsync(_sender.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UnreadCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnMessage()
        {
            // Arrange
            var message = new ChatMessage
            {
                Message = "Test message",
                SenderId = _sender.Id,
                SenderName = _sender.UserName,
                RecipientId = _recipient.Id,
                Timestamp = DateTime.Now
            };

            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(message.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Message, Is.EqualTo("Test message"));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllMessages()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Message = "Message 1",
                    SenderId = _sender.Id,
                    SenderName = _sender.UserName,
                    RecipientId = _recipient.Id,
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new ChatMessage
                {
                    Message = "Message 2",
                    SenderId = _recipient.Id,
                    SenderName = _recipient.UserName,
                    RecipientId = _sender.Id,
                    Timestamp = DateTime.Now.AddHours(-1)
                }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveMessage()
        {
            // Arrange
            var message = new ChatMessage
            {
                Message = "Test message",
                SenderId = _sender.Id,
                SenderName = _sender.UserName,
                RecipientId = _recipient.Id,
                Timestamp = DateTime.Now
            };

            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(message.Id);

            // Assert
            var deletedMessage = await _context.ChatMessages.FindAsync(message.Id);
            Assert.That(deletedMessage, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.DeleteAsync(999));
        }
    }
}
