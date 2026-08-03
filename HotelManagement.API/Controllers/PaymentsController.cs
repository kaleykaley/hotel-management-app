using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class PaymentsController : ApiController
    {
        private readonly HotelDataDataContext dc = new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Payments
        // retrieve entire list of payments
        public List<Payment> Get()
        {
            var listPayments = from Payment in dc.Payments select Payment;
            return listPayments.ToList(); // convert back to list from object var
        }

        // GET api/Payments/5
        public IHttpActionResult Get(int id)
        {
            Payment payment = dc.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                // response says the Payment was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, payment));
            }
            // Payment not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Payment not found."));
        }

        // POST api/Payments
        // Insert a Payment, returns ihttp result
        public IHttpActionResult Post([FromBody] Payment newPayment)
        {
            string error = ValidatePayment(newPayment);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // Find invoice this payment belongs to
            Invoice invoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == newPayment.InvoiceId);
            if (invoice == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Invoice not found."));
            }

            // Mark invoice as paid
            invoice.InvoiceStatus = "Paid";

            // Add the payment
            dc.Payments.InsertOnSubmit(newPayment);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }
            // if it arrives here, everything ran well
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

                total += nights * reservation.Room.PricePerNight;
            }

            var extras = dc.ReservationExtraServices
                .Where(x => x.ReservationId == invoice.ReservationId).ToList();

            foreach (var extra in extras)
            {
                total += extra.ExtraService.Price * extra.Quantity;
            }

            return total;
        }

        /// <summary>
        /// Validates the payment data before inserting it into the database
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        private string ValidatePayment(Payment payment)
        {
            if (payment == null)
            {
                return "Payment data is required.";
            }

            var invoice = dc.Invoices
                .FirstOrDefault(i => i.InvoiceId == payment.InvoiceId);

            if (invoice == null)
            {
                return "There is no invoice with this ID.";
            }

            if (payment.PaymentType != "Cash" &&
                payment.PaymentType != "Credit_Card" &&
                payment.PaymentType != "Debit_Card")
            {
                return "Invalid payment method.";
            }

            if (payment.AmountPaid <= 0)
            {
                return "Payment amount must be greater than zero.";
            }

            decimal invoiceTotal = CalculateInvoiceTotal(invoice);

            // Check if the payment amount matches the invoice total (allowing for a small rounding error)
            if (Math.Abs(payment.AmountPaid - invoiceTotal) > 0.01m)
            {
                return "Payment must cover the full invoice amount.";
            }

            var existingPayment = dc.Payments
                .FirstOrDefault(p => p.InvoiceId == payment.InvoiceId);

            if (existingPayment != null)
            {
                return "This invoice already has a payment registered.";
            }

            return null;
        }
    }
}