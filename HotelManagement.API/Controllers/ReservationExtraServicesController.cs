using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class ReservationExtraServicesController : ApiController
    {
        private readonly HotelDataDataContext dc = new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        /// <summary>
        /// Retrieves a list of all ReservationExtraService records
        /// </summary>
        /// <returns>List of ReservationExtraService records</returns>
        // GET api/ReservationExtraServices
        public List<ReservationExtraService> Get()
        {
            // Returns the rows from table linking reservations to extra services + quantity
            var listReservationExtraServiceExtraServices = from ReservationExtraService in dc.ReservationExtraServices select ReservationExtraService;
            return listReservationExtraServiceExtraServices.ToList(); // convert back to list from object var
        }

        /// <summary>
        /// Retrieves a specific ReservationExtraService record based on the composite key of reservationId and extraServiceId
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="extraServiceId"></param>
        /// <returns>Specific ReservationExtraService record</returns>
        // GET api/ReservationExtraServices/5/2
        public IHttpActionResult Get(int reservationId, int extraServiceId)
        {
            // Need both IDs because the primary key is a composite key of ReservationId and ExtraServiceId
            ReservationExtraService item = dc.ReservationExtraServices
                .FirstOrDefault(r => r.ReservationId == reservationId && r.ExtraServiceId == extraServiceId);

            if (item != null)
            {
                return Ok(item);
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Reservation extra service not found."));
        }

        /// <summary>
        /// Inserts a new ReservationExtraService record into the database
        /// </summary>
        /// <param name="newReservationExtraService"></param>
        /// <returns>HTTP response</returns>
        // POST api/ReservationExtraServices
        public IHttpActionResult Post([FromBody] ReservationExtraService newReservationExtraService)
        {
            string error = ValidateReservationExtraService(newReservationExtraService);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // association doesn't exist yet, so add it
            dc.ReservationExtraServices.InsertOnSubmit(newReservationExtraService);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        /// <summary>
        /// Deletes a specific ReservationExtraService record based on the composite key of reservationId and extraServiceId
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="extraServiceId"></param>
        /// <returns>HTTP response</returns>
        // DELETE api/ReservationExtraServices/5/2
        [Route("api/ReservationExtraServices/{reservationId}/{extraServiceId}")]
        public IHttpActionResult Delete(int reservationId, int extraServiceId)
        {
            ReservationExtraService reservationExtraService =
                dc.ReservationExtraServices.FirstOrDefault(r =>
                    r.ReservationId == reservationId &&
                    r.ExtraServiceId == extraServiceId);

            if (reservationExtraService != null)
            {
                dc.ReservationExtraServices.DeleteOnSubmit(reservationExtraService);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception e)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable,
                            e.Message));
                }

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK,
                        "Extra service removed from reservation."));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "This extra service is not associated with this reservation."));
        }

        /// <summary>
        /// Validates the ReservationExtraService object
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Error message or null if valid</returns>
        private string ValidateReservationExtraService(ReservationExtraService item)
        {
            if (item == null)
            {
                return "Reservation extra service data is required.";
            }

            var reservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == item.ReservationId);

            if (reservation == null)
            {
                return "Reservation does not exist.";
            }

            var extraService = dc.ExtraServices
                .FirstOrDefault(es => es.ExtraServiceId == item.ExtraServiceId);

            if (extraService == null)
            {
                return "Extra service does not exist.";
            }

            if (item.Quantity <= 0)
            {
                return "Quantity must be greater than zero.";
            }

            var existing = dc.ReservationExtraServices.FirstOrDefault(r => r.ReservationId == item.ReservationId && r.ExtraServiceId == item.ExtraServiceId);

            if (existing != null)
            {
                return "This extra service is already added to this reservation.";
            }

            return null;
        }
    }
}