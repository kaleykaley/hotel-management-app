using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; } 

        public string PaymentType { get; set; }   // Cash, Credit_Card, Debit_Card

        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; }

        public int InvoiceId { get; set; }

    }
}
