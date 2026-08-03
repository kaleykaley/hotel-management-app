using System;

namespace HotelManagement.WPF.Data.ViewModels
{
    public class StayHistory
    {
        public int ReservationId { get; set; }

        public int RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Nights { get; set; }
    }
}
