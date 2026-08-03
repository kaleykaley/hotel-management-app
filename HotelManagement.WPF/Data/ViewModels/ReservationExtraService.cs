namespace HotelManagement.WPF.Data.Entities
{
    /// Represents an extra service selection in the reservation form
    public class ReservationExtraService
    {
        public int ExtraServiceId { get; set; }

        public string ServiceName { get; set; }

        public decimal Price { get; set; }

        public bool IsSelected { get; set; }

        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;

        public string DisplayText
        {
            get
            {
                return $"{ServiceName} ({Price:0.00} €)";
            }
        }
    }
}