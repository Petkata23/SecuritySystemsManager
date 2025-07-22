using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IChatMessageService : IBaseCrudService<ChatMessageDto, IChatMessageRepository>
    {
        Task<IEnumerable<ChatMessageDto>> GetMessagesByUserIdAsync(int userId);
        Task<IEnumerable<ChatMessageDto>> GetConversationAsync(int userId1, int userId2);
        Task<IEnumerable<ChatMessageDto>> GetUnreadMessagesAsync(int userId);
        Task MarkAsReadAsync(int messageId);
        Task MarkConversationAsReadAsync(int userId1, int userId2);
        Task SendMessageAsync(int senderId, int? recipientId, string message, bool isFromSupport = false);
        Task<List<int>> GetSupportUserIdsAsync();
        Task<ChatMessageDto> ProcessUserMessageAsync(int userId, string message);
        Task<ChatMessageDto> ProcessSupportMessageAsync(int senderId, int recipientId, string message);
    }
} 