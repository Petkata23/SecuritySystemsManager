using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Security.Claims;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class NotificationController : BaseCrudController<NotificationDto, INotificationRepository, INotificationService, NotificationEditVm, NotificationDetailsVm>
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public NotificationController(IMapper mapper, INotificationService service, IUserService userService)
            : base(service, mapper)
        {
            _userService = userService;
            _notificationService = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
                return View(notifications);
            }
            
            return View(Enumerable.Empty<NotificationDto>());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                await _notificationService.MarkAsReadAsync(id, userId);
                return Json(new { success = true });
            }
            
            return Json(new { success = false });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                await _notificationService.MarkAllAsReadAsync(userId);
                return Json(new { success = true });
            }
            
            return Json(new { success = false });
        }

        /// <summary>
        /// Creates a notification for a user when an order status changes
        /// </summary>
        public async Task<bool> SendOrderStatusChangeNotification(int orderId, int clientId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            return await _notificationService.SendOrderStatusChangeNotificationAsync(orderId, clientId, oldStatus, newStatus);
        }
    }
} 