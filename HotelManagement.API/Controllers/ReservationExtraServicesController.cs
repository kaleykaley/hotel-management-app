using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    // DELETE THIS CONTROLLER?
    public class ReservationExtraServicesController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);


        // GET api/ReservationExtraServiceExtraServices
        // retrieve entire list of categories
        public List<ReservationExtraService> Get()
        {
            var listReservationExtraServiceExtraServices = from ReservationExtraService in dc.ReservationExtraServices select ReservationExtraService;
            return listReservationExtraServiceExtraServices.ToList(); // convert back to list from object var
        }

        // NEED BOTH IDS BECAUSE PRIMARY KEY IS MADE OR 2 FOREIGN KEEYS
        // GET api/ReservationExtraServiceExtraServices/5/2
        public IHttpActionResult Get(int reservationId, int extraServiceId)
        {
            ReservationExtraService item = dc.ReservationExtraServices
                .FirstOrDefault(r => r.ReservationId == reservationId && r.ExtraServiceId == extraServiceId);

            if (item != null)
            {
                return Ok(item);
            }

            return ResponseMessage(
                Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    "Reservation extra service not found."));
        }

        // POST api/ReservationExtraServiceExtraServices
        // Insert a ReservationExtraService, returns ihttp result
        public IHttpActionResult Post([FromBody] ReservationExtraService newReservationExtraService)
        {
            string error = ValidateReservationExtraService(newReservationExtraService);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // category doesn't exist yet, so add it
            dc.ReservationExtraServices.InsertOnSubmit(newReservationExtraService);

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

        // DELETE api/ReservationExtraServiceExtraServices/5
        // Added a route so this method is called when URL contains a ReservationExtraServiceId
        //[Route("api/ReservationExtraServiceExtraServices/{ReservationExtraServiceId}")]
        // DELETE api/ReservationExtraServices/{reservationId}/{extraServiceId}
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
                    return ResponseMessage(
                        Request.CreateResponse(
                            HttpStatusCode.ServiceUnavailable,
                            e.Message));
                }

                return ResponseMessage(
                    Request.CreateResponse(
                        HttpStatusCode.OK,
                        "Extra service removed from reservation."));
            }

            return ResponseMessage(
                Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    "This extra service is not associated with this reservation."));
        }

        private string ValidateReservationExtraService(ReservationExtraService item)
        {
            var reservation = dc.Reservations
                .FirstOrDefault(r => r.ReservationId == item.ReservationId);

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

            var existing = dc.ReservationExtraServices
                .FirstOrDefault(r => r.ReservationId == item.ReservationId && r.ExtraServiceId == item.ExtraServiceId);

            if (existing != null)
            {
                return "This extra service is already added to this reservation.";
            }

            return null;
        }
    }
}