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

        /// <summary>
        /// Retrieves the entire list of guests
        /// </summary>
        /// <returns></returns>
        // GET api/Guests
        public List<Guest> Get()
        {
            var lista = from Guest in dc.Guests select Guest;
            return lista.ToList(); // convert back to list from object var
        }

        /// <summary>
        /// Retrieves a specific guest by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
        // GET api/guests/5
        public IHttpActionResult Get(int id)
        {
            Guest guest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (guest != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, guest));
            }
            // guest not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Guest not found."));
        }

        /// <summary>
        /// Retrieves the stay history of a specific guest by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
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

        /// <summary>
        /// Retrieves all guests with active reservations
        /// </summary>
        /// <returns>HTTP response</returns>
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

        /// <summary>
        /// Retrieves all guests with stay history 
        /// </summary>
        /// <returns>HTTP response</returns>
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

        /// <summary>
        /// Inserts a new guest into the database
        /// </summary>
        /// <param name="newGuest"></param>
        /// <returns>HTTP response</returns>
        // POST api/guests
        public IHttpActionResult Post([FromBody] Guest newGuest)
        {
            string error = ValidateGuest(newGuest);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // add new guest
            dc.Guests.InsertOnSubmit(newGuest);

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
        /// Updates an existing guest
        /// </summary>
        /// <param name="id"></param>
        /// <param name="editedGuest"></param>
        /// <returns>HTTP response</returns>
        // PUT api/guests/5
        public IHttpActionResult Put(int id, [FromBody] Guest editedGuest)
        {
            string error = ValidateGuest(editedGuest);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            Guest existingGuest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (existingGuest == null) // guest requested to update doesnt exist
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        /// <summary>
        /// Deletes a guest if they have no active reservations
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/guests/5
        public IHttpActionResult Delete(int id)
        {
            Guest guest = dc.Guests.FirstOrDefault(g => g.GuestId == id);

            if (guest != null) // guest requested to delete exists, so delete it
            {
                // Check if guest has active reservations
                var activeReservation = dc.Reservations.FirstOrDefault(r => r.GuestId == id &&
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

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
            }
            // guest requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no guest with this ID to delete."));
        }

        /// <summary>
        /// Validates the guest data
        /// </summary>
        /// <param name="guest"></param>
        /// <returns>Error message or null if valid</returns>
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

            if (!guest.Email.Contains("@") || !guest.Email.Contains("."))
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