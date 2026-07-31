using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class DashboardController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Dashboard
        public IHttpActionResult Get()
        {
            DashboardViewModel dashboard = new DashboardViewModel
            {
                TotalGuests = dc.Guests.Count(),
                AvailableRooms = dc.Rooms.Count(r => r.RoomStatus == "Available"),
                ActiveReservations = dc.Reservations.Count(r => r.ReservationStatus == "Reserved"
                                                             || r.ReservationStatus == "Checked_In"),
                UnpaidInvoices = dc.Invoices.Count(i => i.InvoiceStatus == "Unpaid")
            };

            return Ok(dashboard);
        }
    }
}