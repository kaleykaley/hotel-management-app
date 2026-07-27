using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    public class Invoice
    {

        // API is responsible for calculating: room charge, extra services total, final total
        public int InvoiceId { get; set; }

        public int ReservationId { get; set; }

        public DateTime IssueDate { get; set; }

        public string InvoiceStatus { get; set; }

        public string GuestName { get; set; }

        public int RoomNumber { get; set; }

        public decimal RoomCharge { get; set; }

        public decimal TotalDue { get; set; }
    }
}
