using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManagerMVC.ViewModels.Chat
{
    public class ChatConversationVm
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherUserName { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public bool IsOtherUserOnline { get; set; }
        
        // Helper property for the view
        public ChatUserInfo OtherUser => new ChatUserInfo
        {
            Username = OtherUserName,
            Role = new ChatUserRole { Name = "User" } // Default role
        };
    }

    public class ChatUserInfo
    {
        public string Username { get; set; } = string.Empty;
        public ChatUserRole Role { get; set; } = new();
    }

    public class ChatUserRole
    {
        public string Name { get; set; } = string.Empty;
    }
} 