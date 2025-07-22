using System;
using System.Collections.Generic;

namespace SecuritySystemsManagerMVC.ViewModels.Chat
{
    public class ChatAdminPanelVm
    {
        public List<ChatUserVm> ActiveChats { get; set; } = new();
        public ChatUserVm? CurrentChat { get; set; }
        public List<ChatMessageVm> Messages { get; set; } = new();
    }

    public class ChatUserVm
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public bool HasUnreadMessages { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatMessageVm
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromSupport { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class SendMessageVM
    {
        public string Message { get; set; } = string.Empty;
        public int? RecipientId { get; set; }
    }
} 