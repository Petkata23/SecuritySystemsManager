using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class ChatMessage : BaseEntity
    {
        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
        public string Message { get; set; }
        public int SenderId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Sender name must be between 2 and 100 characters")]
        public string SenderName { get; set; }
        public int? RecipientId { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Recipient name must be between 2 and 100 characters")]
        public string? RecipientName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromSupport { get; set; }
        [StringLength(500, ErrorMessage = "Attachment URL cannot be longer than 500 characters")]
        public string? AttachmentUrl { get; set; }
        [StringLength(200, ErrorMessage = "Attachment name cannot be longer than 200 characters")]
        public string? AttachmentName { get; set; }

        // Navigation properties
        public virtual User Sender { get; set; }
        public virtual User? Recipient { get; set; }
    }
} 