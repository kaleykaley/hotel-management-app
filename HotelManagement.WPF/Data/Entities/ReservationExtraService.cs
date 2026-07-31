using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.WPF.Data.Entities
{
    // represents what's shown on the screen while the user is filling in the reservation form
    public class ReservationExtraService
    {
        public int ExtraServiceId { get; set; }

        public string ServiceName { get; set; }

        public decimal Price { get; set; }

        public bool IsSelected { get; set; } // not needed?

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
        /*public class ReservationExtraService

                }


        {
            public ExtraService Service { get; set; }

            public bool IsSelected { get; set; }

            public int Quantity { get; set; } = 1;

            public string DisplayText
            {
                get
                {
                    return $"{Service.Name} ({Service.Price:0.00} €)";
                }
            } */
