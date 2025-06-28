using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class InvoiceDetailsVm : BaseVm
    {
        [DisplayName("Security System Order")]
        public string OrderTitle { get; set; }

        [DisplayName("Issued On")]
        public DateTime IssuedOn { get; set; }

        [DisplayName("Total Amount")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Is Paid")]
        public bool IsPaid { get; set; }
    }
}
