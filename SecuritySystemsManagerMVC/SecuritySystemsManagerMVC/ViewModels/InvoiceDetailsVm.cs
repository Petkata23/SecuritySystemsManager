using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class InvoiceDetailsVm : BaseVm
    {
        [DisplayName("Security System Order")]
        public string OrderTitle { get; set; }
        
        [DisplayName("Order ID")]
        public int SecuritySystemOrderId { get; set; }

        [DisplayName("Issued On")]
        public DateTime IssuedOn { get; set; }

        [DisplayName("Total Amount")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Is Paid")]
        public bool IsPaid { get; set; }
        
        // Client information
        [DisplayName("Client Name")]
        public string ClientFullName { get; set; }
        
        [DisplayName("Client Email")]
        public string ClientEmail { get; set; }
        
        [DisplayName("Client Phone")]
        public string ClientPhone { get; set; }
        
        // Location information
        [DisplayName("Location Name")]
        public string LocationName { get; set; }
        
        [DisplayName("Location Address")]
        public string LocationAddress { get; set; }
        
        // Order information
        [DisplayName("Order Description")]
        public string OrderDescription { get; set; }
        
        [DisplayName("Order Status")]
        public string OrderStatus { get; set; }
        
        [DisplayName("Requested Date")]
        public DateTime OrderRequestedDate { get; set; }
        
        // Due date (30 days from issue date)
        [DisplayName("Due Date")]
        public DateTime DueDate => IssuedOn.AddDays(30);
    }
}
