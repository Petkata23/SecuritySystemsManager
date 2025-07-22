using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class ChatMessageService : BaseCrudService<ChatMessageDto, IChatMessageRepository>, IChatMessageService
    {
        private readonly IUserService _userService;
        private const int SYSTEM_USER_ID = 1; // System user should be the first user in the database
        private static readonly string[] WelcomeMessages = new[]
        {
            "Здравейте! 👋 Добре дошли в нашата система за поддръжка. С какво можем да Ви помогнем?",
            "Здравейте! 🌟 Аз съм Вашият асистент днес. Как мога да Ви бъда полезен/а?",
            "Добре дошли! 💫 На Ваше разположение съм за всякакви въпроси и съдействие.",
            "Здравейте и добре дошли! 🤝 С удоволствие ще Ви помогна с каквото е необходимо.",
            "Приветствам Ви! ✨ Тук съм, за да отговоря на всички Ваши въпроси и да Ви съдействам."
        };

        private static readonly Random Random = new Random();

        public ChatMessageService(IChatMessageRepository repository, IUserService userService) : base(repository)
        {
            _userService = userService;
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesByUserIdAsync(int userId)
        {
            var messages = await _repository.GetMessagesByUserIdAsync(userId);

            // Check if this is the first time the user is chatting
            if (!messages.Any())
            {
                // Send welcome message
                var welcomeMessage = WelcomeMessages[Random.Next(WelcomeMessages.Length)];
                await SendMessageAsync(SYSTEM_USER_ID, userId, welcomeMessage, true);

                // Refresh messages to include welcome message
                messages = await _repository.GetMessagesByUserIdAsync(userId);
            }

            return messages;
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversationAsync(int userId1, int userId2)
        {
            return await _repository.GetConversationAsync(userId1, userId2);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetUnreadMessagesAsync(int userId)
        {
            return await _repository.GetUnreadMessagesAsync(userId);
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            await _repository.MarkAsReadAsync(messageId);
        }

        public async Task MarkConversationAsReadAsync(int userId1, int userId2)
        {
            await _repository.MarkConversationAsReadAsync(userId1, userId2);
        }

        public async Task SendMessageAsync(int senderId, int? recipientId, string message, bool isFromSupport = false)
        {
            // Verify sender exists (except for system user)
            if (senderId != SYSTEM_USER_ID && !await _userService.ExistsByIdAsync(senderId)) return;

            // Verify recipient exists if specified
            if (recipientId.HasValue && !await _userService.ExistsByIdAsync(recipientId.Value)) return;

            var chatMessage = new ChatMessageDto
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsFromSupport = isFromSupport || senderId == SYSTEM_USER_ID,
                IsRead = false
            };

            await _repository.SaveAsync(chatMessage);
        }

        public async Task<List<int>> GetSupportUserIdsAsync()
        {
            var users = await _userService.GetAllAsync();
            return users
                .Where(u => u.Role?.Name == "Admin" || u.Role?.Name == "Manager")
                .Select(u => u.Id)
                .ToList();
        }

        public async Task<ChatMessageDto> ProcessUserMessageAsync(int userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty");
            }

            var user = await _userService.GetByIdIfExistsAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Save the message to the database
            await SendMessageAsync(userId, null, message);

            // Return the saved message
            return new ChatMessageDto
            {
                SenderId = userId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsFromSupport = false,
                IsRead = false
            };
        }

        public async Task<ChatMessageDto> ProcessSupportMessageAsync(int senderId, int recipientId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty");
            }

            var sender = await _userService.GetByIdIfExistsAsync(senderId);
            if (sender == null)
            {
                throw new UnauthorizedAccessException("Support user not found");
            }

            // Save the message to the database
            await SendMessageAsync(senderId, recipientId, message, true);

            // Return the saved message
            return new ChatMessageDto
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsFromSupport = true,
                IsRead = false
            };
        }
    }
} 