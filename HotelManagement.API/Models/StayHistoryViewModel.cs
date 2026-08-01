using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelManagement.API
{
    public class StayHistoryViewModel
    {
        public int ReservationId { get; set; }

        public int RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Nights { get; set; }

        // not needed - all checked-out
        //public string Status { get; set; }
    }
}