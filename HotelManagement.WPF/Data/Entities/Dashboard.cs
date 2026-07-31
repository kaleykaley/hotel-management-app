using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    public class Dashboard
    {
        public int TotalGuests { get; set; }

        public int AvailableRooms { get; set; }

        public int ActiveReservations { get; set; }

        public int UnpaidInvoices { get; set; }
    }
}