using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Entities
{
    public class Notification : BaseEntity
    {
        public int RecipientId { get; set; }
        public virtual User Recipient { get; set; }

        public string Message { get; set; }
        public DateTime DateSent { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
