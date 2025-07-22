using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManagerMVC.ViewModels.Chat
{
    public class ChatAdminVm
    {
        public List<ChatMessageDto> AllMessages { get; set; } = new();
        public List<ChatConversationDto> Conversations { get; set; } = new();
    }
} 