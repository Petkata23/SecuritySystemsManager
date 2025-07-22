using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatMessageService _chatService;
        private readonly IUserService _userService;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(IChatMessageService chatService, IUserService userService)
        {
            Console.WriteLine("ChatHub: Constructor called");
            _chatService = chatService;
            _userService = userService;
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing UserId claim");
            }
            return userId;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("ChatHub: OnConnectedAsync called");
            Console.WriteLine($"ChatHub: ConnectionId: {Context.ConnectionId}");
            
            try 
            {
                var userId = GetUserId();
                
                // Only notify support staff for regular users
                if (userId > 0)
                {
                    var user = await _userService.GetByIdIfExistsAsync(userId);
                    if (user != null && !(user.Role?.Name == "Admin" || user.Role?.Name == "Manager"))
                    {
                        // Notify support staff about new user
                        var supportUsers = await _chatService.GetSupportUserIdsAsync();
                        foreach (var supportUserId in supportUsers)
                        {
                            await Clients.User(supportUserId.ToString()).SendAsync("ReceiveMessage",
                                new { 
                                    senderId = userId, 
                                    senderName = "System",
                                    message = $"Потребител {user.Username} се присъедини към чата.",
                                    timestamp = DateTime.Now,
                                    isFromSupport = false,
                                    recipientId = (int?)null
                                });
                        }
                    }
                }

                _userConnections[userId.ToString()] = Context.ConnectionId;
                await Clients.All.SendAsync("UserConnected", userId, Context.User?.FindFirst(ClaimTypes.Name)?.Value);
                Console.WriteLine($"ChatHub: User {userId} connected with connection {Context.ConnectionId}");
            }
            catch (Exception)
            {
                // Log the error but don't throw - we don't want to prevent connection
                Console.WriteLine("ChatHub: Error in OnConnectedAsync");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                await Clients.All.SendAsync("UserDisconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty");
            }

            var userId = GetUserId();
            var user = await _userService.GetByIdIfExistsAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Обработка на съобщението чрез сервиса
            var chatMessage = await _chatService.ProcessUserMessageAsync(userId, message);

            // Получаване на всички потребители от поддръжката
            var supportUsers = await _chatService.GetSupportUserIdsAsync();

            var messageData = new { 
                senderId = userId, 
                senderName = user.Username,
                message = message,
                timestamp = DateTime.UtcNow,
                isFromSupport = false,
                recipientId = (int?)null
            };

            // Изпращане на съобщението до всички потребители от поддръжката
            foreach (var supportUserId in supportUsers)
            {
                await Clients.User(supportUserId.ToString()).SendAsync("ReceiveMessage", messageData);
            }

            // НЕ изпращаме обратно до изпращача - той вече е добавил съобщението локално
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task SendSupportMessage(int recipientId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty");
            }

            var senderId = GetUserId();
            var sender = await _userService.GetByIdIfExistsAsync(senderId);
            if (sender == null)
            {
                throw new UnauthorizedAccessException("Support user not found");
            }

            // Обработка на съобщението от поддръжката чрез сервиса
            var chatMessage = await _chatService.ProcessSupportMessageAsync(senderId, recipientId, message);

            // Получаване на всички потребители от поддръжката
            var supportUsers = await _chatService.GetSupportUserIdsAsync();

            var messageData = new { 
                senderId = senderId, 
                senderName = sender.Username,
                recipientId = recipientId,
                message = message,
                timestamp = DateTime.UtcNow,
                isFromSupport = true
            };

            // Изпращане до получателя
            await Clients.User(recipientId.ToString()).SendAsync("ReceiveMessage", messageData);

            // Изпращане до всички потребители от поддръжката (включително изпращача)
            foreach (var supportUserId in supportUsers)
            {
                await Clients.User(supportUserId.ToString()).SendAsync("ReceiveMessage", messageData);
            }
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            Console.WriteLine($"ChatHub: SendMessage called with: {messageDto.Message}");
            Console.WriteLine($"ChatHub: ConnectionId: {Context.ConnectionId}");
            
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            Console.WriteLine($"ChatHub: SenderId: {senderId}, SenderName: {senderName}");

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(senderName) || !int.TryParse(senderId, out int senderIdInt))
            {
                Console.WriteLine("ChatHub: User not authenticated");
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Save message to database
            await _chatService.SendMessageAsync(senderIdInt, messageDto.RecipientId, messageDto.Message);

            // Send to receiver if online
            if (messageDto.RecipientId.HasValue && _userConnections.TryGetValue(messageDto.RecipientId.Value.ToString(), out var receiverConnectionId))
            {
                var messageData = new { 
                    senderId = senderIdInt, 
                    senderName = senderName,
                    message = messageDto.Message,
                    timestamp = DateTime.UtcNow,
                    isFromSupport = false,
                    recipientId = messageDto.RecipientId
                };
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", messageData);
            }

            // Send confirmation to sender
            var savedMessage = new { 
                senderId = senderIdInt, 
                senderName = senderName,
                message = messageDto.Message,
                timestamp = DateTime.UtcNow,
                isFromSupport = false,
                recipientId = messageDto.RecipientId
            };
            await Clients.Caller.SendAsync("MessageSent", savedMessage);
        }

        public async Task MarkAsRead(string conversationUserId)
        {
            var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out int currentUserIdInt) && 
                int.TryParse(conversationUserId, out int conversationUserIdInt))
            {
                await _chatService.MarkConversationAsReadAsync(currentUserIdInt, conversationUserIdInt);
                
                // Notify the other user that messages were read
                if (_userConnections.TryGetValue(conversationUserId, out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessagesRead", currentUserId);
                }
            }
        }

        public async Task Typing(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", senderId);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UserStoppedTyping", senderId);
            }
        }

        public static bool IsUserOnline(string userId)
        {
            return _userConnections.ContainsKey(userId);
        }
    }
} 