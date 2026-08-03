using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class InvoicesController : ApiController
    {
        private readonly HotelDataDataContext dc = new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Invoices
        public IHttpActionResult Get()
        {
            var invoices = dc.Invoices
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    ReservationId = i.ReservationId,
                    IssueDate = i.IssueDate,
                    InvoiceStatus = i.InvoiceStatus,

                    GuestName = i.Reservation.Guest.Name,

                    RoomNumber = i.Reservation.Room.RoomNumber,

                    RoomCharge =
                        (i.Reservation.CheckOutDate - i.Reservation.CheckInDate).Days
                        * i.Reservation.Room.PricePerNight,

                    TotalDue = CalculateInvoiceTotal(i)
                })
                .ToList();

            return Ok(invoices);
        }
         public IHttpActionResult Get(int id)
         {
            InvoiceViewModel invoice = dc.Invoices
                .Where(i => i.InvoiceId == id)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    ReservationId = i.ReservationId,
                    IssueDate = i.IssueDate,
                    InvoiceStatus = i.InvoiceStatus,

                    GuestName = i.Reservation.Guest.Name,

                    RoomNumber = i.Reservation.Room.RoomNumber,

                    RoomCharge =
                        (i.Reservation.CheckOutDate - i.Reservation.CheckInDate).Days
                        * i.Reservation.Room.PricePerNight,

                    TotalDue = CalculateInvoiceTotal(i)
                })
                .FirstOrDefault();
            if (invoice != null)
             {
                 // response says the Invoice was found and here it is
                 return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, invoice));
             }
             // Invoice not found, sends 404 default
             return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Invoice not found."));
         }


        // POST api/Invoices
        // Insert a Invoice, returns ihttp result
        public IHttpActionResult Post([FromBody] Invoice newInvoice)
        {
            string error = ValidateInvoice(newInvoice);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // Invoice doesn't exist yet, so add it
            dc.Invoices.InsertOnSubmit(newInvoice);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }
            // if it arrives here, correu tudo bem
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // PUT: like update from CRUD
        // only allow editing the Invoice Status
        // PUT api/Invoices/5
        public IHttpActionResult Put(int id, [FromBody] Invoice editedInvoice)
        {
            string error = ValidateInvoice(editedInvoice);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            Invoice existingInvoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            if (existingInvoice == null) // Invoice requested to change doesnt exist
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no Invoice with this ID to edit."));
            }
            // Invoice found, so update it
            existingInvoice.InvoiceStatus = editedInvoice.InvoiceStatus;

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }
            // if it arrives here, correu tudo bem
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }


        /// <summary>
        /// Calculates the total amount due for an invoice: room charges + extra services
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private decimal CalculateInvoiceTotal(Invoice invoice)
        {
            decimal total = 0;

            var reservation = dc.Reservations
                .FirstOrDefault(r => r.ReservationId == invoice.ReservationId);

            if (reservation != null)
            {
                int nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;

                total += nights * dc.Rooms.First(r => r.RoomId == reservation.RoomId).PricePerNight;
            }

            var extras = dc.ReservationExtraServices
                .Where(x => x.ReservationId == invoice.ReservationId)
                .ToList();

            foreach (var extra in extras)
            {
                total += extra.ExtraService.Price * extra.Quantity;
            }

            return total;
        }

        /// <summary>
        /// Validates the invoice data before inserting or updating it in the database
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private string ValidateInvoice(Invoice invoice)
        {
            if (invoice == null)
            {
                return "Invoice data is required.";
            }

            // Check reservation exists
            var reservation = dc.Reservations
                .FirstOrDefault(r => r.ReservationId == invoice.ReservationId);

            if (reservation == null)
            {
                return "There is no reservation with this ID.";
            }

            // Check reservation does not already have an invoice
            var existingInvoice = dc.Invoices
                .FirstOrDefault(i => i.ReservationId == invoice.ReservationId);

            if (existingInvoice != null)
            {
                return "This reservation already has an invoice.";
            }

            // Check invoice status
            if (invoice.InvoiceStatus != "Paid" &&
                invoice.InvoiceStatus != "Unpaid")
            {
                return "Invalid invoice status.";
            }

            return null;
        }
    }
}