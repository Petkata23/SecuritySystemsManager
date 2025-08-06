using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.Models;
using SecuritySystemsManagerMVC.ViewModels.Chat;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatMessageService _chatMessageService;
        private readonly IUserService _userService;

        public ChatController(IChatMessageService chatService, IUserService userService)
        {
            _chatMessageService = chatService;
            _userService = userService;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Admin(int? userId = null)
        {
            return await AdminPanel(userId);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AdminPanel(int? userId = null)
        {
            try
            {
                // Use service to get active chats
                var messages = await _chatMessageService.GetActiveChatsAsync();
                var allUsers = await _userService.GetAllAsync();

                // Get all messages for finding last message
                var allMessages = await _chatMessageService.GetAllAsync();

                // Simple grouping logic - can stay in controller
                var activeChats = messages
                    .Where(m => !m.IsFromSupport) // Exclude support messages
                    .GroupBy(m => m.SenderId)
                    .Where(g => g.Any() && g.Key != 0) // Exclude system messages
                    .Select(g =>
                    {
                        var user = allUsers.FirstOrDefault(u => u.Id == g.Key);
                        var username = user?.Username ?? g.First().SenderName ?? "Unknown";
                        
                        return new ChatUserVm
                        {
                            UserId = g.Key,
                            Username = username,
                            LastMessage = allMessages
                                .Where(m => (m.SenderId == g.Key && !m.IsFromSupport) || 
                                           (m.RecipientId == g.Key && m.IsFromSupport) ||
                                           (m.SenderId == g.Key && m.IsFromSupport))
                                .OrderByDescending(m => m.Timestamp)
                                .First()?.Message ?? "",
                            LastMessageTime = allMessages
                                .Where(m => (m.SenderId == g.Key && !m.IsFromSupport) || 
                                           (m.RecipientId == g.Key && m.IsFromSupport) ||
                                           (m.SenderId == g.Key && m.IsFromSupport))
                                .Max(m => m.Timestamp),
                            HasUnreadMessages = g.Any(m => !m.IsRead && !m.IsFromSupport),
                            UserRole = user?.Role?.Name ?? "User",
                            UnreadCount = g.Count(m => !m.IsRead && !m.IsFromSupport),
                            IsOnline = false // Will be updated by SignalR
                        };
                    })
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToList();

                ChatUserVm currentChat = null;
                IEnumerable<ChatMessageVm> chatMessages = null;

                if (userId.HasValue)
                {
                    var user = await _userService.GetByIdIfExistsAsync(userId.Value);
                    if (user != null && user.Role?.Name != "Admin" && user.Role?.Name != "Manager")
                    {
                        currentChat = new ChatUserVm
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            UserRole = user.Role?.Name ?? "User"
                        };

                        // Use service to get conversation
                        var conversation = await _chatMessageService.GetChatConversationAsync(userId.Value);
                        chatMessages = conversation.Select(m => new ChatMessageVm
                        {
                            Id = m.Id,
                            Message = m.Message,
                            Timestamp = m.Timestamp,
                            IsFromSupport = m.IsFromSupport,
                            SenderName = m.IsFromSupport ? "Support" : m.SenderName ?? "Unknown",
                            SenderId = m.SenderId,
                            IsRead = m.IsRead,
                            ReadAt = m.ReadAt
                        });
                    }
                }

                var model = new ChatAdminPanelVm
                {
                    ActiveChats = activeChats,
                    CurrentChat = currentChat,
                    Messages = chatMessages?.ToList() ?? new List<ChatMessageVm>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetActiveChats()
        {
            try
            {
                // Use service to get active chats
                var messages = await _chatMessageService.GetActiveChatsAsync();
                var allUsers = await _userService.GetAllAsync();

                // Get all messages for finding last message
                var allMessages = await _chatMessageService.GetAllAsync();

                // Simple grouping logic - can stay in controller
                var activeChats = messages
                    .Where(m => !m.IsFromSupport) // Exclude support messages
                    .GroupBy(m => m.SenderId)
                    .Where(g => g.Any() && g.Key != 0) // Exclude system messages
                    .Select(g =>
                    {
                        var user = allUsers.FirstOrDefault(u => u.Id == g.Key);
                        var username = user?.Username ?? g.First().SenderName ?? "Unknown";
                        
                        return new ChatUserVm
                        {
                            UserId = g.Key,
                            Username = username,
                            LastMessage = allMessages
                                .Where(m => (m.SenderId == g.Key && !m.IsFromSupport) || 
                                           (m.RecipientId == g.Key && m.IsFromSupport) ||
                                           (m.SenderId == g.Key && m.IsFromSupport))
                                .OrderByDescending(m => m.Timestamp)
                                .First()?.Message ?? "",
                            LastMessageTime = allMessages
                                .Where(m => (m.SenderId == g.Key && !m.IsFromSupport) || 
                                           (m.RecipientId == g.Key && m.IsFromSupport) ||
                                           (m.SenderId == g.Key && m.IsFromSupport))
                                .Max(m => m.Timestamp),
                            HasUnreadMessages = g.Any(m => !m.IsRead && !m.IsFromSupport),
                            UserRole = user?.Role?.Name ?? "User",
                            UnreadCount = g.Count(m => !m.IsRead && !m.IsFromSupport),
                            IsOnline = false // Will be updated by SignalR
                        };
                    })
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToList();

                return Json(activeChats);
            }
            catch (Exception ex)
            {
                return Json(new List<ChatUserVm>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int? recipientId, string message)
        {
            try
            {
                var userId = GetUserId();
                await _chatMessageService.SendMessageAsync(userId, recipientId, message);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> UnreadMessages()
        {
            try
            {
                var userId = GetUserId();
                var unreadMessages = await _chatMessageService.GetUnreadMessagesAsync(userId);
                
                var result = unreadMessages.Select(m => new
                {
                    id = m.Id,
                    message = m.Message,
                    timestamp = m.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                    isFromSupport = m.IsFromSupport,
                    senderName = m.IsFromSupport ? "Support" : m.SenderName ?? "Unknown"
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            try
            {
                await _chatMessageService.MarkAsReadAsync(messageId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        [Route("Chat/GetUnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _chatMessageService.GetUnreadMessagesCountAsync(userId);
                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        [Route("Chat/GetRecentMessages")]
        public async Task<IActionResult> GetRecentMessages(int count = 20)
        {
            try
            {
                var userId = GetUserId();
                var recentMessages = await _chatMessageService.GetRecentMessagesForUserAsync(userId, count);
                
                var result = recentMessages.Select(m => new
                {
                    id = m.Id,
                    message = m.Message,
                    timestamp = m.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                    isFromSupport = m.IsFromSupport,
                    senderName = m.IsFromSupport ? "Support" : m.SenderName ?? "Unknown",
                    senderId = m.SenderId,
                    isRead = m.IsRead
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [Route("Chat/MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                await _chatMessageService.MarkAllMessagesAsReadAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Chat/MarkAllAsRead/{userId}")]
        public async Task<IActionResult> MarkAllAsReadForUser(int userId)
        {
            try
            {
                await _chatMessageService.MarkAllMessagesAsReadForUserAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        [Route("Chat/GetUserInfo/{userId}")]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            try
            {
                var user = await _userService.GetByIdIfExistsAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var messages = await _chatMessageService.GetMessagesByUserIdAsync(userId);
                var lastMessage = messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                var firstMessage = messages.OrderBy(m => m.Timestamp).FirstOrDefault();

                var result = new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role?.Name ?? "User"
                    },
                    lastMessage = lastMessage?.Message ?? "",
                    lastMessageTime = lastMessage?.Timestamp.ToString("dd/MM/yyyy HH:mm") ?? "",
                    hasUnreadMessages = messages.Any(m => !m.IsRead && !m.IsFromSupport),
                    unreadCount = messages.Count(m => !m.IsRead && !m.IsFromSupport),
                    firstMessageDate = firstMessage?.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss") ?? null,
                    totalMessages = messages.Count()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        [Route("Chat/GetChatMessages/{userId}")]
        public async Task<IActionResult> GetChatMessages(int userId)
        {
            try
            {
                var user = await _userService.GetByIdIfExistsAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var conversation = await _chatMessageService.GetChatMessagesForUserAsync(userId);
                
                var messages = conversation.Select(m => new
                {
                    id = m.Id,
                    message = m.Message,
                    timestamp = m.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                    isFromSupport = m.IsFromSupport,
                    senderName = m.IsFromSupport ? "Support" : m.SenderName ?? "Unknown",
                    senderId = m.SenderId,
                    isRead = m.IsRead,
                    readAt = m.ReadAt?.ToString("dd/MM/yyyy HH:mm")
                });

                var result = new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role?.Name ?? "User"
                    },
                    messages = messages
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> SendSupportMessage([FromBody] SendMessageVM model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Message))
                {
                    return Json(new { success = false, error = "Message cannot be empty" });
                }

                var senderId = GetUserId();
                await _chatMessageService.ProcessSupportMessageAsync(senderId, model.RecipientId ?? 0, model.Message);
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new InvalidOperationException("User ID not found in claims");
        }
    }
} 