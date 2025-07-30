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
                var messages = await _chatMessageService.GetAllAsync();
                var allUsers = await _userService.GetAllAsync();

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
                        LastMessage = messages
                            .Where(m => m.SenderId == g.Key || m.RecipientId == g.Key)
                            .OrderByDescending(m => m.Timestamp)
                            .First()?.Message ?? "",
                        LastMessageTime = messages
                            .Where(m => m.SenderId == g.Key || m.RecipientId == g.Key)
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

                    var conversation = await _chatMessageService.GetMessagesByUserIdAsync(userId.Value);
                    chatMessages = conversation.Select(m => new ChatMessageVm
                    {
                        Id = m.Id,
                        Message = m.Message,
                        Timestamp = m.Timestamp,
                        IsFromSupport = m.IsFromSupport,
                        SenderName = m.IsFromSupport ? "Поддръжка" : m.SenderName ?? "Unknown",
                        SenderId = m.SenderId,
                        IsRead = m.IsRead,
                        ReadAt = m.ReadAt
                    });

                    // Mark messages as read when opening the chat
                    foreach (var message in conversation.Where(m => !m.IsRead && !m.IsFromSupport))
                    {
                        await _chatMessageService.MarkAsReadAsync(message.Id);
                    }
                }
            }

            var viewModel = new ChatAdminPanelVm
            {
                ActiveChats = activeChats,
                CurrentChat = currentChat,
                Messages = chatMessages?.ToList() ?? new List<ChatMessageVm>()
            };

            return View(viewModel);
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
                // Load all needed data upfront
                var allMessages = (await _chatMessageService.GetAllAsync()).ToList();
                var allUsers = (await _userService.GetAllAsync()).ToList();

                var chats = allMessages
                    .Where(m => !m.IsFromSupport)
                    .GroupBy(m => m.SenderId)
                    .Where(g => g.Any() && g.Key != 0)
                    .Select(g =>
                    {
                        var user = allUsers.FirstOrDefault(u => u.Id == g.Key);
                        if (user == null) return null;

                        var userMessages = allMessages
                            .Where(m => m.SenderId == g.Key || m.RecipientId == g.Key)
                            .ToList();

                        return new
                        {
                            userId = g.Key,
                            username = user.Username,
                            userRole = user.Role?.Name ?? "User",
                            lastMessage = userMessages
                                .OrderByDescending(m => m.Timestamp)
                                .First()?.Message ?? "",
                            lastMessageTime = userMessages.Any() ? userMessages.Max(m => m.Timestamp) : DateTime.MinValue,
                            hasUnread = userMessages.Any(m => !m.IsRead && !m.IsFromSupport),
                            unreadCount = userMessages.Count(m => !m.IsRead && !m.IsFromSupport)
                        };
                    })
                    .Where(c => c != null)
                    .OrderByDescending(c => c.lastMessageTime)
                    .ToList();

                return Json(chats);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Възникна грешка при зареждане на активните чатове" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int? recipientId, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be empty");
            }

            var senderId = GetUserId();
            await _chatMessageService.SendMessageAsync(senderId, recipientId, message);

            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public async Task<IActionResult> UnreadMessages()
        {
            var userId = GetUserId();
            var messages = await _chatMessageService.GetUnreadMessagesAsync(userId);

            var messageVMs = messages.Select(m => new ChatMessageVm
            {
                Id = m.Id,
                Message = m.Message,
                Timestamp = m.Timestamp,
                IsFromSupport = m.IsFromSupport,
                SenderName = m.IsFromSupport ? "Поддръжка" : m.SenderName ?? "Unknown",
                SenderId = m.SenderId,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt
            });

            return Json(new { count = messageVMs.Count(), messages = messageVMs });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            await _chatMessageService.MarkAsReadAsync(messageId);
            return Ok();
        }

        [HttpGet]
        [Route("Chat/GetUnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            var messages = await _chatMessageService.GetUnreadMessagesAsync(userId);
            return Json(new { count = messages.Count() });
        }

        [HttpGet]
        [Route("Chat/GetRecentMessages")]
        public async Task<IActionResult> GetRecentMessages(int count = 20)
        {
            var userId = GetUserId();
            var messages = await _chatMessageService.GetMessagesByUserIdAsync(userId);
            
            var recentMessages = messages
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .Select(m => new ChatMessageVm
                {
                    Id = m.Id,
                    Message = m.Message,
                    Timestamp = m.Timestamp,
                    IsFromSupport = m.IsFromSupport,
                    SenderName = m.IsFromSupport ? "Поддръжка" : m.SenderName ?? "Unknown",
                    SenderId = m.SenderId,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt
                });

            return Json(recentMessages);
        }

        [HttpPost]
        [Route("Chat/MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                var messages = await _chatMessageService.GetUnreadMessagesAsync(userId);

                foreach (var message in messages)
                {
                    await _chatMessageService.MarkAsReadAsync(message.Id);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Възникна грешка при маркиране на съобщенията като прочетени" });
            }
        }

        [HttpPost]
        [Route("Chat/MarkAllAsRead/{userId}")]
        public async Task<IActionResult> MarkAllAsReadForUser(int userId)
        {
            try
            {
                var messages = await _chatMessageService.GetMessagesByUserIdAsync(userId);
                foreach (var message in messages.Where(m => !m.IsRead && !m.IsFromSupport))
                {
                    await _chatMessageService.MarkAsReadAsync(message.Id);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Възникна грешка при маркиране на съобщенията като прочетени" });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        [Route("Chat/GetUserInfo/{userId}")]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            try
            {
                var messages = await _chatMessageService.GetMessagesByUserIdAsync(userId);
                var user = await _userService.GetByIdIfExistsAsync(userId);

                if (user == null)
                {
                    return NotFound(new { error = "Потребителят не е намерен" });
                }

                var firstMessage = messages.OrderBy(m => m.Timestamp).FirstOrDefault();
                var lastMessage = messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();

                return Json(new
                {
                    username = user.Username,
                    role = user.Role?.Name ?? "User",
                    firstMessageDate = firstMessage?.Timestamp,
                    lastActivity = lastMessage?.Timestamp,
                    totalMessages = messages.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Възникна грешка при зареждане на информацията за потребителя" });
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
                if (user == null || user.Role?.Name == "Admin" || user.Role?.Name == "Manager")
                {
                    return NotFound(new { error = "Потребителят не е намерен" });
                }

                var messages = await _chatMessageService.GetMessagesByUserIdAsync(userId);
                var chatMessages = messages.Select(m => new ChatMessageVm
                {
                    Id = m.Id,
                    Message = m.Message,
                    Timestamp = m.Timestamp,
                    IsFromSupport = m.IsFromSupport,
                    SenderName = m.IsFromSupport ? "Поддръжка" : m.SenderName ?? "Unknown",
                    SenderId = m.SenderId,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt
                }).OrderBy(m => m.Timestamp);

                // Маркираме съобщенията като прочетени
                foreach (var message in messages.Where(m => !m.IsRead && !m.IsFromSupport))
                {
                    await _chatMessageService.MarkAsReadAsync(message.Id);
                }

                return Json(new
                {
                    messages = chatMessages,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role?.Name ?? "User"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Възникна грешка при зареждане на съобщенията" });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> SendSupportMessage([FromBody] SendMessageVM model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Message))
                {
                    return BadRequest(new { error = "Съобщението не може да бъде празно" });
                }

                var senderId = GetUserId();
                await _chatMessageService.SendMessageAsync(senderId, model.RecipientId, model.Message, true);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Грешка при изпращане на съобщението" });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing UserId claim");
            }
            return userId;
        }
    }
} 