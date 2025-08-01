using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class ChatMessageRepository : BaseRepository<ChatMessage, ChatMessageDto>, IChatMessageRepository
    {
        public ChatMessageRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesByUserIdAsync(int userId)
        {
            var messages = await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return MapToEnumerableOfModel(messages);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversationAsync(int userId1, int userId2)
        {
            var messages = await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => 
                    (m.SenderId == userId1 && m.RecipientId == userId2) ||
                    (m.SenderId == userId2 && m.RecipientId == userId1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return MapToEnumerableOfModel(messages);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetUnreadMessagesAsync(int userId)
        {
            var messages = await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => 
                    (m.RecipientId == userId || m.RecipientId == null) && 
                    !m.IsRead &&
                    m.SenderId != userId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            return MapToEnumerableOfModel(messages);
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            var message = await _dbSet.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkConversationAsReadAsync(int userId1, int userId2)
        {
            var messages = await _dbSet
                .Where(m => 
                    ((m.SenderId == userId1 && m.RecipientId == userId2) ||
                     (m.SenderId == userId2 && m.RecipientId == userId1)) &&
                    !m.IsRead &&
                    m.RecipientId == userId1)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _dbSet
                .CountAsync(m => 
                    (m.RecipientId == userId || m.RecipientId == null) && 
                    !m.IsRead &&
                    m.SenderId != userId);
        }

        public async Task<List<ChatConversationDto>> GetUserConversationsAsync(int userId)
        {
            var conversations = await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Where(g => g.Key.HasValue)
                .Select(g => new ChatConversationDto
                {
                    UserId = g.Key.Value,
                    UserName = g.First().SenderId == userId ? 
                        (g.First().Recipient != null ? g.First().Recipient.UserName : "Unknown") : 
                        (g.First().Sender != null ? g.First().Sender.UserName : "Unknown"),
                    LastMessage = g.OrderByDescending(m => m.Timestamp).First().Message,
                    LastMessageTime = g.Max(m => m.Timestamp),
                    UnreadCount = g.Count(m => !m.IsRead && m.RecipientId == userId),
                    IsOnline = false // TODO: Implement online status
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            return conversations;
        }

        public async Task SaveAsync(ChatMessageDto chatMessageDto)
        {
            var chatMessage = MapToEntity(chatMessageDto);
            
            // Ensure SenderName and RecipientName are set
            if (string.IsNullOrEmpty(chatMessage.SenderName))
            {
                var sender = await _context.Set<User>().FindAsync(chatMessage.SenderId);
                chatMessage.SenderName = sender?.UserName ?? "Unknown";
            }
            
            if (chatMessage.RecipientId.HasValue && string.IsNullOrEmpty(chatMessage.RecipientName))
            {
                var recipient = await _context.Set<User>().FindAsync(chatMessage.RecipientId.Value);
                chatMessage.RecipientName = recipient?.UserName ?? "Unknown";
            }
            
            await _dbSet.AddAsync(chatMessage);
            await _context.SaveChangesAsync();
        }
    }
} 