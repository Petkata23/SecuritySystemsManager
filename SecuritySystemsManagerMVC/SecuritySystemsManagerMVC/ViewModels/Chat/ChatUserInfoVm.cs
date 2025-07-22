using System;
using System.Collections.Generic;

namespace SecuritySystemsManagerMVC.ViewModels.Chat
{
    public class ChatUserInfoVm
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? FirstMessageDate { get; set; }
        public int TotalMessages { get; set; }
        public double AverageRating { get; set; }
        public List<ChatRatingVm> Ratings { get; set; } = new();
    }

    public class ChatRatingVm
    {
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime RatedAt { get; set; }
    }
} 