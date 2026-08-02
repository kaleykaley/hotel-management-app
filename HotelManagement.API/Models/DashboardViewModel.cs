using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelManagement.API
{
    public class DashboardViewModel
    {
        public int TotalGuests { get; set; }

        public int AvailableRooms { get; set; }

        public int ActiveReservations { get; set; }

        public int UnpaidInvoices { get; set; }

        public int TodayCheckIns { get; set; }  

        public List<string> Activities { get; set; }

    }
}