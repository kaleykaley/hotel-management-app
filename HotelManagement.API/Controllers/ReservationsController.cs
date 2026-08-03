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
        private readonly HotelDataDataContext dc = new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);


        /// <summary>
        /// Retrieves all reservations, including extra services
        /// </summary>
        /// <returns>List of ReservationViewModel</returns>
        // GET api/Reservations
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

        /// <summary>
        /// Retrieve a specific reservation by ID, including extra services
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ReservationViewModel</returns>
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

        /// <summary>
        /// Retrieve all reservations that do not have an associated invoice
        /// </summary>
        /// <returns>List of ReservationViewModel</returns>
        // GET api/Reservations/WithoutInvoice
        [HttpGet]
        [Route("api/Reservations/WithoutInvoice")]
        public IHttpActionResult GetWithoutInvoice()
        {
            var reservations = dc.Reservations
                .Where(r => !dc.Invoices.Any(i => i.ReservationId == r.ReservationId))
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

            return Ok(reservations);
        }

        /// <summary>
        /// Inserts a new reservation into the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns>HTTP response</returns>
        // POST api/Reservations
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        /// <summary>
        /// /// Updates an existing reservation and replaces its extra services
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>HTTP response</returns>
        // PUT api/Reservations/5
        public IHttpActionResult Put(int id, [FromBody] ReservationViewModel model)
        {
            string error = ValidateReservation(model);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }
            
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        /// <summary>
        /// Check in a reservation by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
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

            // Can only check in if the reservation is currently reserved
            if (reservation.ReservationStatus != "Reserved")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "This reservation is not in a state that allows check-in."));
            }

            // Check-in cannot happen before the scheduled check-in date
            if (reservation.CheckInDate.Date > DateTime.Today)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Check-in is only possible on or after the reservation check-in date."));
            }

            reservation.ReservationStatus = "Checked_In";
            reservation.Room.RoomStatus = "Occupied"; // room is now occupied

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
        /// Check out a reservation by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
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

            // Can only check out if the reservation is currently checked in
            if (reservation.ReservationStatus != "Checked_In")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "This reservation is not in a state that allows check-out."));
            }

            reservation.ReservationStatus = "Checked_Out";
            reservation.Room.RoomStatus = "Available"; // room becomes available again after check-out

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
        /// Cancel a reservation by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
        // PUT api/Reservations/5/Cancel
        [HttpPut]
        [Route("api/Reservations/{id}/Cancel")]
        public IHttpActionResult Cancel(int id)
        {
            Reservation reservation = dc.Reservations
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Reservation not found."));
            }

            // Cannot cancel after check-in
            if (reservation.ReservationStatus == "Checked_In")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A reservation that has already checked in cannot be cancelled."));
            }

            // Cannot cancel already finished reservations
            if (reservation.ReservationStatus == "Checked_Out")
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A completed reservation cannot be cancelled."));
            }

            // Cannot cancel after check-in date
            if (reservation.CheckInDate <= DateTime.Today)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A reservation can only be cancelled before the check-in date."));
            }

            reservation.ReservationStatus = "Cancelled";

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
        /// Validates a reservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns>error message if invalid, or null if valid.</returns>
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

            if (room.IsDeleted)
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

            if (reservation.CheckInDate.Date < DateTime.Today)
            {
                return "Check-in date cannot be in the past.";
            }

            if (reservation.CheckOutDate.Date < DateTime.Today)
            {
                return "Check-out date cannot be in the past.";
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