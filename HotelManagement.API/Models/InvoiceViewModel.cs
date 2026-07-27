using System;

namespace HotelManagement.API
{
    public class InvoiceViewModel
    {
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