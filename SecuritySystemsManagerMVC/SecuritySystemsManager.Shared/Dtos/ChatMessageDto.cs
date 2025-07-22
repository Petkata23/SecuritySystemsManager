using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class ChatMessageDto : BaseDto
    {
        public string Message { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromSupport { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }

        // Helper properties for UI
        public bool IsFromCurrentUser { get; set; }
        public string FormattedSentAt { get; set; }
    }

    public class SendMessageDto
    {
        public string Message { get; set; }
        public int? RecipientId { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
    }

    public class ChatConversationDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }
} 