namespace HotelManagement.WPF.Data.Entities
{
    public class Room
    {
        // Primary key from SQL Server
        public int RoomId { get; set; }

        // Hotel room number (101, 102, etc.)
        public int RoomNumber { get; set; }

        // Room category
        public string RoomType { get; set; }

        // Maximum number of guests
        public int Capacity { get; set; }

        // Cost per night
        public decimal PricePerNight { get; set; }

        // Current room state
        public string RoomStatus { get; set; }


        public override string ToString()
        {
            return $"{RoomNumber} - {RoomType} - {RoomStatus}";
        }
    }
}
