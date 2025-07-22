using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManagerMVC.ViewModels.Chat
{
    public class ChatIndexVm
    {
        public List<ChatConversationDto> Conversations { get; set; } = new();
        public int UnreadCount { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
    }
} 