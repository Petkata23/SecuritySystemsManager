using System;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class DropboxToken : BaseEntity
    {
        [Required]
        [StringLength(1000, ErrorMessage = "Access token cannot be longer than 1000 characters")]
        public string AccessToken { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Refresh token cannot be longer than 1000 characters")]
        public string RefreshToken { get; set; }

        [Required]
        public DateTime ExpiryTime { get; set; }
    }
} 