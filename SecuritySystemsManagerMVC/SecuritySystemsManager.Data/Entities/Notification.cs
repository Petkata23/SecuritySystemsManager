using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class Notification : BaseEntity
    {
        public int RecipientId { get; set; }
        public virtual User Recipient { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters")]
        public string Message { get; set; }
        
        public DateTime DateSent { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
