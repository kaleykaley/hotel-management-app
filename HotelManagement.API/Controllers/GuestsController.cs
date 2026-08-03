using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class GuestsController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Guests
        // retrieve entire list of categories
        public List<Guest> Get()
        {
            var lista = from Guest in dc.Guests select Guest;
            return lista.ToList(); // convert back to list from object var
        }

        // GET api/guests/5
        public IHttpActionResult Get(int id)
        {
            Guest guest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (guest != null)
            {
                // response says the guest was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, guest));
            }
            // guest not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Guest not found."));
        }


        // GET api/Guests/5/StayHistory
        [Route("api/Guests/{id}/StayHistory")]
        public IHttpActionResult GetStayHistory(int id)
        {
            Guest guest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (guest == null)
            {
                return ResponseMessage(
                    Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        "Guest not found."));
            }

            // Get guest's completed reservations and combine them with room info
            // "Go through Reservations table, and for each row, temporarily call that row reservation"
            // Get the guest's completed reservations
            var history = from reservation in dc.Reservations

                              // Only get this guest's past stays
                          where reservation.GuestId == id
                          && reservation.ReservationStatus == "Checked_Out"

                          // Create the object sent to WPF
                          select new StayHistoryViewModel
                          {
                              ReservationId = reservation.ReservationId,

                              // Access room through the Reservation relationship
                              RoomNumber = reservation.Room.RoomNumber,

                              CheckInDate = reservation.CheckInDate,
                              CheckOutDate = reservation.CheckOutDate,

                              // Calculate total nights stayed
                              Nights = (reservation.CheckOutDate -
                                        reservation.CheckInDate).Days
                          };

            return Ok(history.ToList());
        }

        // GET api/Guests/ActiveReservations
        [Route("api/Guests/ActiveReservations")]
        public IHttpActionResult GetGuestsWithActiveReservations()
        {
            var guests = dc.Guests
                .Where(g => dc.Reservations.Any(r =>
                    r.GuestId == g.GuestId &&
                    (r.ReservationStatus == "Reserved" ||
                     r.ReservationStatus == "Checked_In")));

            return Ok(guests.ToList());
        }

        // GET api/Guests/WithStayHistory
        [Route("api/Guests/WithStayHistory")]
        public IHttpActionResult GetGuestsWithStayHistory()
        {
            // Get guests where there exists at least one checked-out reservation
            var guests = dc.Guests
                .Where(g => dc.Reservations.Any(r =>
                    r.GuestId == g.GuestId &&
                    r.ReservationStatus == "Checked_Out"));

            return Ok(guests.ToList());
        }

        // POST api/guests
        // Insert a guest, returns ihttp result
        public IHttpActionResult Post([FromBody] Guest newGuest)
        {
            string error = ValidateGuest(newGuest);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // guest doesn't exist yet, so add it
            dc.Guests.InsertOnSubmit(newGuest);

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
        // PUT api/guests/5
        public IHttpActionResult Put(int id, [FromBody] Guest editedGuest)
        {
            string error = ValidateGuest(editedGuest);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            Guest existingGuest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (existingGuest == null) // guest requested to change doesnt exist
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no guest with this ID to edit."));
            }

            // Guest found, so update it
            existingGuest.Name = editedGuest.Name;
            existingGuest.PhoneContact = editedGuest.PhoneContact;
            existingGuest.Email = editedGuest.Email;
            existingGuest.DocumentType = editedGuest.DocumentType;
            existingGuest.DocumentNumber = editedGuest.DocumentNumber;

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

        // DELETE api/guests/5
        // Added a route so this method is called when URL contains a GuestId
        //[Route("api/guests/{GuestId}")]
        public IHttpActionResult Delete(int id)
        {
            Guest guest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (guest != null) // guest requested to delete exists, so delete it
            {

                // Check if guest has active reservations
                var activeReservation = dc.Reservations
                    .FirstOrDefault(r => r.GuestId == id &&
                        (r.ReservationStatus == "Reserved" ||
                         r.ReservationStatus == "Checked_In"));

                if (activeReservation != null)
                {
                    return BadRequest("Guest cannot be deleted because they have an active reservation.");
                }

                // No active reservations, so delete guest
                dc.Guests.DeleteOnSubmit(guest);

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
            // guest requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no guest with this ID to delete."));
        }


        private string ValidateGuest(Guest guest)
        {
            if (guest == null)
            {
                return "Guest data is required.";
            }

            if (string.IsNullOrEmpty(guest.Name))
            {
                return "Guest name is required.";
            }

            if (string.IsNullOrEmpty(guest.PhoneContact))
            {
                return "Phone contact is required.";
            }

            if (string.IsNullOrEmpty(guest.Email))
            {
                return "Email is required.";
            }

            if (!guest.Email.Contains("@"))
            {
                return "Invalid email format.";
            }

            if (string.IsNullOrEmpty(guest.DocumentType))
            {
                return "Document type is required.";
            }

            if (string.IsNullOrEmpty(guest.DocumentNumber))
            {
                return "Document number is required.";
            }

            return null;
        }
    }
}