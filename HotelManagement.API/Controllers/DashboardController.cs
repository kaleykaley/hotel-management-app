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
        private readonly HotelDataDataContext dc = new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        /// <summary>
        /// Retrieves dashboard statistics and recent activity 
        /// </summary>
        /// <returns>Dashboard data</returns>
        // GET api/Dashboard
        public IHttpActionResult Get()
        {
            List<string> activities = new List<string>();

            // Recent check-ins
            var checkIns = dc.Reservations
                .Where(r => r.ReservationStatus == "Checked_In")
                .OrderByDescending(r => r.CheckInDate)
                .Take(5)
                .Select(r => $"✓ Guest {r.Guest.Name} checked into Room {r.Room.RoomNumber}")
                .ToList();

            activities.AddRange(checkIns);

            // Recent check-outs
            var checkOuts = dc.Reservations
                .Where(r => r.ReservationStatus == "Checked_Out")
                .OrderByDescending(r => r.CheckOutDate)
                .Take(5)
                .Select(r => $"✓ Room {r.Room.RoomNumber} checked out")
                .ToList();

            activities.AddRange(checkOuts);

            // Recent payments
            var payments = dc.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => $"✓ Payment received for Invoice #{p.InvoiceId}")
                .ToList();

            activities.AddRange(payments);

            DashboardViewModel dashboard = new DashboardViewModel
            {
                TotalGuests = dc.Guests.Count(),

                AvailableRooms = dc.Rooms.Count(r => r.RoomStatus == "Available"),

                ActiveReservations = dc.Reservations.Count(r => r.ReservationStatus == "Reserved" ||
                    r.ReservationStatus == "Checked_In"),

                UnpaidInvoices = dc.Invoices.Count(i => i.InvoiceStatus == "Unpaid"),

                TodayCheckIns = dc.Reservations.Count(r => r.CheckInDate.Date == DateTime.Today),

                Activities = activities.Take(10).ToList()
            };

            return Ok(dashboard);
        }
    }
}