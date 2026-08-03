namespace HotelManagement.WPF.Data.Entities
{
    public class Room
    {
        public int RoomId { get; set; }

        public int RoomNumber { get; set; }

        public string RoomType { get; set; }

        public int Capacity { get; set; }

        public decimal PricePerNight { get; set; }

        public string RoomStatus { get; set; }


        public override string ToString()
        {
            return $"{RoomNumber} - {RoomType} - {RoomStatus}";
        }
    }
}
