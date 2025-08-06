using System;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class UserProfileDto : BaseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        
        // Account Activity Properties
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int TotalOrders { get; set; }
        public int TotalLocations { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }
} 