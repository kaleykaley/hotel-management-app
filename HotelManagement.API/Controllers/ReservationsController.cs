using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class ReservationsController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);


        // GET api/Reservations
        // retrieve entire list of reservations
        /*public List<Reservation> Get()
        {
            var listReservations = from Reservation in dc.Reservations select Reservation;
            return listReservations.ToList(); // convert back to list from object var
        }*/

        // TEMP CODE FOR TEST
        public IHttpActionResult Get()
        {
            var listReservations = dc.Reservations.Select(r => new
            {
                r.ReservationId,
                r.GuestId,
                GuestName = r.Guest.Name,

                r.RoomId,
                RoomNumber = r.Room.RoomNumber,

                r.CheckInDate,
                r.CheckOutDate,
                r.NumberOfGuests,
                r.ReservationStatus
            });

            return Ok(listReservations.ToList());
        }

        // GET api/Reservations/5
        public IHttpActionResult Get(int id)
        {
            Reservation reservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == id);

            if (reservation != null)
            {
                // response says the Reservation was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, reservation));
            }
            // Reservation not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Reservation not found."));
        }


        // POST api/Reservations
        // Insert a Reservation, returns ihttp result
        public IHttpActionResult Post([FromBody] Reservation newReservation)
        {
            string error = ValidateReservation(newReservation);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // New reservation defaults to "Reserved" status
            newReservation.ReservationStatus = "Reserved";

            // reservation doesn't exist yet, so add it
            dc.Reservations.InsertOnSubmit(newReservation);

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

        // PUT api/Reservations/5
        public IHttpActionResult Put(int id, [FromBody] Reservation editedReservation)
        {
            string error = ValidateReservation(editedReservation);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }
            // delete same ID check?
            Reservation existingReservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == id);

            if (existingReservation == null) // Reservation requested to change doesnt exist
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no Reservation with this ID to edit."));
            }
            // Reservation found, so update it
            existingReservation.GuestId = editedReservation.GuestId;
            existingReservation.RoomId = editedReservation.RoomId;
            existingReservation.CheckInDate = editedReservation.CheckInDate;
            existingReservation.CheckOutDate = editedReservation.CheckOutDate;
            existingReservation.NumberOfGuests = editedReservation.NumberOfGuests;
            existingReservation.ReservationStatus = editedReservation.ReservationStatus;

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

        // DELETE api/Reservations/5
        // Added a route so this method is called when URL contains a ReservationId
        //[Route("api/Reservations/{ReservationId}")]
        public IHttpActionResult Delete(int id)
        {
            Reservation reservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == id);

            if (reservation != null) // Reservation requested to delete exists, so delete it
            {
                dc.Reservations.DeleteOnSubmit(reservation);

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
            // Reservation requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no Reservation with this ID to delete."));
        }

        private string ValidateReservation(Reservation reservation)
        {
            var guest = dc.Guests.FirstOrDefault(g => g.GuestId == reservation.GuestId);

            if (guest == null)
            {
                return "Guest does not exist.";
            }

            var room = dc.Rooms.FirstOrDefault(r => r.RoomId == reservation.RoomId);

            if (room == null)
            {
                return "Room does not exist.";
            }

            if (room.RoomStatus == "Occupied" ||
                room.RoomStatus == "Maintenance")
            {
                return "Room is unavailable.";
            }

            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                return "Checkout date must be after check-in date.";
            }

            if (reservation.NumberOfGuests > room.Capacity)
            {
                return $"This room only allows a maximum of {room.Capacity} guests.";
            }

            /*bool conflict = dc.Reservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                r.ReservationStatus != "Cancelled" &&
                reservation.CheckInDate < r.CheckOutDate &&
                reservation.CheckOutDate > r.CheckInDate
            );

            if (conflict)
            {
                return "Room already has a reservation during these dates.";
            }*/

            return null; // everything is valid
        }
    }
}