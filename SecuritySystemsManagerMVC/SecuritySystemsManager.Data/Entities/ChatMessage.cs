using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class ChatMessage : BaseEntity
    {
        public string Message { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromSupport { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }

        // Navigation properties
        public virtual User Sender { get; set; }
        public virtual User? Recipient { get; set; }
    }
} 