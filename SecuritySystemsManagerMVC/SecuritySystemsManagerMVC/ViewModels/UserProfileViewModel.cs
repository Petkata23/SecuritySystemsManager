using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class UserProfileViewModel
    {
        public string Username { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "Profile Image")]
        public string ProfileImage { get; set; }
        
        [Display(Name = "Two-Factor Authentication")]
        public bool TwoFactorEnabled { get; set; }
        
        public bool HasAuthenticator { get; set; }
        
        public bool Is2faEnabled { get; set; }
        
        public int RecoveryCodesLeft { get; set; }
        
        // Account Activity Properties
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int TotalOrders { get; set; }
        public int TotalLocations { get; set; }
        public string UserRole { get; set; }
    }
} 