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
            var listReservations = dc.Reservations
                .Select(r => new ReservationViewModel
                {
                    ReservationId = r.ReservationId,

                    GuestId = r.GuestId,
                    RoomId = r.RoomId,

                    GuestName = r.Guest.Name,

                    RoomNumber = r.Room.RoomNumber,

                    PricePerNight = r.Room.PricePerNight,

                    CheckInDate = r.CheckInDate,

                    CheckOutDate = r.CheckOutDate,

                    NumberOfGuests = r.NumberOfGuests,

                    ReservationStatus = r.ReservationStatus,

                    // add extra services to reservation view model
                    ExtraServices = r.ReservationExtraServices
                        .Select(s => new ReservationExtraServiceViewModel
                        {
                            ExtraServiceId = s.ExtraServiceId,
                            ServiceName = s.ExtraService.Name,
                            Price = s.ExtraService.Price,
                            Quantity = s.Quantity
                        })
                        .ToList()
                })
                .ToList();
                

            return Ok(listReservations);
        }

        // GET api/Reservations/5
        public IHttpActionResult Get(int id)
        {
            ReservationViewModel reservation = dc.Reservations
                .Where(r => r.ReservationId == id)
                .Select(r => new ReservationViewModel
                {
                    ReservationId = r.ReservationId,

                    GuestName = r.Guest.Name,

                    RoomNumber = r.Room.RoomNumber,

                    PricePerNight = r.Room.PricePerNight,

                    CheckInDate = r.CheckInDate,

                    CheckOutDate = r.CheckOutDate,

                    NumberOfGuests = r.NumberOfGuests,

                    ReservationStatus = r.ReservationStatus,

                    // add extra services to reservation view model
                    ExtraServices = r.ReservationExtraServices
                        .Select(s => new ReservationExtraServiceViewModel
                        {
                            ExtraServiceId = s.ExtraServiceId,
                            ServiceName = s.ExtraService.Name,
                            Price = s.ExtraService.Price,
                            Quantity = s.Quantity
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (reservation != null)
            {
                return ResponseMessage(
                    Request.CreateResponse(HttpStatusCode.OK, reservation));
            }

            return ResponseMessage(
                Request.CreateResponse(HttpStatusCode.NotFound, "Reservation not found."));
        }


        // POST api/Reservations
        // Insert a Reservation, returns ihttp result
        public IHttpActionResult Post([FromBody] ReservationViewModel model)
        {
            string error = ValidateReservation(model);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // New reservation defaults to "Reserved" status
            Reservation reservation = new Reservation
            {
                GuestId = model.GuestId,
                RoomId = model.RoomId,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfGuests = model.NumberOfGuests,
                ReservationStatus = "Reserved"
            };

            dc.Reservations.InsertOnSubmit(reservation);
            dc.SubmitChanges();

            if (model.ExtraServices != null)
            {
                foreach (var service in model.ExtraServices)
                {
                    dc.ReservationExtraServices.InsertOnSubmit(
                        new ReservationExtraService
                        {
                            ReservationId = reservation.ReservationId,
                            ExtraServiceId = service.ExtraServiceId,
                            Quantity = service.Quantity
                        });
                }
            }

            dc.SubmitChanges();

            // if it arrives here, correu tudo bem
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // PUT: like update from CRUD

        // PUT api/Reservations/5
        public IHttpActionResult Put(int id, [FromBody] ReservationViewModel model)
        {
            string error = ValidateReservation(model);

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
            existingReservation.GuestId = model.GuestId;
            existingReservation.RoomId = model.RoomId;
            existingReservation.CheckInDate = model.CheckInDate;
            existingReservation.CheckOutDate = model.CheckOutDate;
            existingReservation.NumberOfGuests = model.NumberOfGuests;
            existingReservation.ReservationStatus = model.ReservationStatus;

            // Replace all existing services with new list
            var oldServices = dc.ReservationExtraServices.Where(x => x.ReservationId == id);

            dc.ReservationExtraServices.DeleteAllOnSubmit(oldServices);

            // add the new services
            if (model.ExtraServices != null)
            {
                foreach (var service in model.ExtraServices)
                {
                    dc.ReservationExtraServices.InsertOnSubmit(
                        new ReservationExtraService
                        {
                            ReservationId = id,
                            ExtraServiceId = service.ExtraServiceId,
                            Quantity = service.Quantity
                        });
                }
            }

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


        // PUT CheckIn api/Reservations/5/CheckIn
        [HttpPut]
        [Route("api/Reservations/{id}/CheckIn")]
        public IHttpActionResult CheckIn(int id)
        {
            Reservation reservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no Reservation with this ID."));
            }

            if (reservation.ReservationStatus != "Reserved")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "This reservation is not in a state that allows check-in."));
            }

            reservation.ReservationStatus = "Checked_In";

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

        // PUT CheckOut api/Reservations/5/CheckOut
        [HttpPut]
        [Route("api/Reservations/{id}/CheckOut")]
        public IHttpActionResult CheckOut(int id)
        {
            Reservation reservation = dc.Reservations.FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no Reservation with this ID."));
            }

            if (reservation.ReservationStatus != "Checked_In")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "This reservation is not in a state that allows check-out."));
            }

            reservation.ReservationStatus = "Checked_Out";

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

        private string ValidateReservation(ReservationViewModel reservation)
        {
            if (reservation == null)
            {
                return "No reservation data supplied.";
            }

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

            // check that reservation dates don't overlap
            bool conflict = dc.Reservations.Any(r =>
                r.ReservationId != reservation.ReservationId &&
                r.RoomId == reservation.RoomId &&
                r.ReservationStatus != "Cancelled" &&
                reservation.CheckInDate < r.CheckOutDate &&
                reservation.CheckOutDate > r.CheckInDate
            );

            if (conflict)
            {
                return "Room already has a reservation during these dates.";
            }

            if (reservation.ExtraServices != null)
            {
                foreach (var service in reservation.ExtraServices)
                {
                    if (!dc.ExtraServices.Any(s => s.ExtraServiceId == service.ExtraServiceId))
                    {
                        return $"Extra service with ID {service.ExtraServiceId} does not exist.";
                    }

                    if (service.Quantity <= 0)
                    {
                        return "Service quantity must be greater than zero.";
                    }
                }
            }

            return null; // everything is valid
        }
    }
}