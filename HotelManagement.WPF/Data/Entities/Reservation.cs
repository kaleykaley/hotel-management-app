using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    public class Reservation
    {

        // breakfast 20, spa 50, room service 25, parking 10

        public int ReservationId { get; set; }

        public int GuestId { get; set; }
        public string GuestName { get; set; }


        public int RoomId { get; set; }
        public int RoomNumber { get; set; }

        public decimal PricePerNight { get; set; } // display property, not stored in the database


        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        // Reserved, Checked_In, Checked_Out, Cancelled
        public string ReservationStatus { get; set; }

        public List<ReservationExtraService> ExtraServices { get; set; } = new List<ReservationExtraService>();

        public int NumberOfNights =>
            (CheckOutDate - CheckInDate).Days;

        public string DisplayName
        {
            get
            {
                return $"Reservation #{ReservationId} - {GuestName} (Room {RoomNumber})";
            }
        }
    }
}
