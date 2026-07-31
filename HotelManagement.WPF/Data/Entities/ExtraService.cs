namespace HotelManagement.WPF.Data.Entities
{
    // Additional Services Management
    // Possibility to add extra services to reservations (breakfast, spa access, room service, parking)
    public class ExtraService
    {
        public int ExtraServiceId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        /// Returns formatted string.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} - {Price:0.00}€";
        }
    }
}
