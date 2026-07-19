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
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Invoices
        public IHttpActionResult Get()
        {
            return Ok(dc.Invoices.ToList());
        }

        // GET api/Invoices/5
        public IHttpActionResult Get(int id)
        {
            Invoice Invoice = dc.Invoices.FirstOrDefault(p => p.InvoiceId == id);

            if (Invoice != null)
            {
                // response says the Invoice was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, Invoice));
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

        // allow edits to Invoices??
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

        // DELETE api/Invoices/5
        // Added a route so this method is called when URL contains a InvoiceId
        //[Route("api/Invoices/{InvoiceId}")]
        public IHttpActionResult Delete(int id)
        {
            Invoice invoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            if (invoice != null) // Invoice requested to delete exists, so delete it
            {
                dc.Invoices.DeleteOnSubmit(invoice);

                try
                {
                    dc.SubmitChanges(); // commit updates to db
                }
                catch (Exception e)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
                }
                // if it arrives here, correu tudo bem
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
            }
            // Invoice requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no Invoice with this ID to delete."));
        }

        // move to helper class? need only in paymentscontroler?
        private decimal CalculateInvoiceTotal(Invoice invoice)
        {
            decimal total = 0;

            var reservation = dc.Reservations
                .FirstOrDefault(r => r.ReservationId == invoice.ReservationId);

            if (reservation != null)
            {
                int nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;

                total += nights * reservation.Room.PricePerNight;
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

        private string ValidateInvoice(Invoice invoice)
        {
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