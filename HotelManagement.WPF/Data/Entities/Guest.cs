namespace HotelManagement.WPF.Data.Entities
{
    public class Guest
    {
        public int GuestId { get; set; }

        public string Name { get; set; }

        public string PhoneContact { get; set; }

        public string Email { get; set; }

        public string DocumentType { get; set; }   // Passport, Driver's License, National ID

        public string DocumentNumber { get; set; }

        /// <summary>
        /// Returns guest ID and name as a string.
        /// </summary>
        public override string ToString()
        {
            return $"{GuestId} - {Name}";
        }
    }
}