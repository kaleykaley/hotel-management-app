using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        public int GuestId { get; set; }
        public string GuestName { get; set; }


        public int RoomId { get; set; }
        public int RoomNumber { get; set; }


        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        // Reserved, Checked_In, Checked_Out, Cancelled
        public string ReservationStatus { get; set; }

        public int NumberOfNights =>
            (CheckOutDate - CheckInDate).Days;
    }
}
