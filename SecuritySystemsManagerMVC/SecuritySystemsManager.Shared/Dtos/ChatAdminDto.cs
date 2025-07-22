using System;
using System.Collections.Generic;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class ChatUserDto
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

    public class ChatUserInfoDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? FirstMessageDate { get; set; }
        public int TotalMessages { get; set; }
        public double AverageRating { get; set; }
        public List<ChatRatingDto> Ratings { get; set; } = new();
    }

    public class ChatRatingDto
    {
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime RatedAt { get; set; }
    }
} 