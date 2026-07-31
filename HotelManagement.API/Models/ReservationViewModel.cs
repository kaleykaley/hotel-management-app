using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelManagement.API
{
    public class ReservationViewModel
    {
        public int ReservationId { get; set; }

        public int GuestId { get; set; }

        public int RoomId { get; set; }

        public string GuestName { get; set; }

        public int RoomNumber { get; set; }

        public decimal PricePerNight { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        public string ReservationStatus { get; set; }

        public List<ReservationExtraServiceViewModel> ExtraServices { get; set; }

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