using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelManagement.API
{
    public class ReservationExtraServiceViewModel
    {
        public int ExtraServiceId { get; set; }

        public string ServiceName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}