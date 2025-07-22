using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IChatMessageRepository : IBaseRepository<ChatMessageDto>
    {
        Task<IEnumerable<ChatMessageDto>> GetMessagesByUserIdAsync(int userId);
        Task<IEnumerable<ChatMessageDto>> GetConversationAsync(int userId1, int userId2);
        Task<IEnumerable<ChatMessageDto>> GetUnreadMessagesAsync(int userId);
        Task MarkAsReadAsync(int messageId);
        Task MarkConversationAsReadAsync(int userId1, int userId2);
    }
} 