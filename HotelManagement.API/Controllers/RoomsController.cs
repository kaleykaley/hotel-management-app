using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class RoomsController : ApiController
    {
        private readonly HotelDataDataContext dc =
               new HotelDataDataContext(
                   ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/Rooms
        // retrieve entire list of rooms
        public List<Room> Get()
        {
            // don't return "isDeleted" rooms, only active ones
            var listRooms = from room in dc.Rooms
                            where room.IsDeleted == false
                            select room;
            return listRooms.ToList(); // convert back to list from object var
        }

        // GET api/Rooms/5
        public IHttpActionResult Get(int id)
        {
            // don't return "isDeleted" rooms, only active ones
            Room room = dc.Rooms.FirstOrDefault(r =>r.RoomId == id && !r.IsDeleted);

            if (room != null)
            {
                // response says the Room was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, room));
            }
            // Room not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Room not found."));
        }


        // POST api/Rooms
        // Insert a Room, returns ihttp result
        public IHttpActionResult Post([FromBody] Room newRoom)
        {
            string error = ValidateRoom(newRoom);

            if (error != null)
            {
                return ResponseMessage(
                    Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            newRoom.RoomStatus = "Available";

            // room doesn't exist yet, so add it
            dc.Rooms.InsertOnSubmit(newRoom);

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
        // PUT api/Rooms/5
        public IHttpActionResult Put(int id, [FromBody] Room editedRoom)
        {
            string error = ValidateRoom(editedRoom);

            if (error != null)
            {
                return ResponseMessage(
                    Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            // don't allow editing of a deleted room, only active ones
            Room existingRoom = dc.Rooms.FirstOrDefault(r =>r.RoomId == id && !r.IsDeleted);

            if (existingRoom == null) // Room requested to change doesnt exist
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no Room with this ID to edit."));
            }
            // Room found, so update it
            existingRoom.RoomNumber = editedRoom.RoomNumber;
            existingRoom.RoomType = editedRoom.RoomType;
            existingRoom.Capacity = editedRoom.Capacity;
            existingRoom.PricePerNight = editedRoom.PricePerNight;
            existingRoom.RoomStatus = editedRoom.RoomStatus;

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

        // DELETE api/Rooms/5
        // Added a route so this method is called when URL contains a RoomId
        //[Route("api/Rooms/{RoomId}")]
        public IHttpActionResult Delete(int id)
        {
            Room room = dc.Rooms.FirstOrDefault(g => g.RoomId == id);

            if (room != null) // Room requested to delete exists, so delete it
            {
                // Check if room has active/future reservations
                if (room.Reservations.Any(r =>
                    r.CheckOutDate >= DateTime.Today &&
                    (r.ReservationStatus == "Reserved" ||
                     r.ReservationStatus == "Checked_In")))
                {
                    return ResponseMessage(
                        Request.CreateResponse(
                            HttpStatusCode.Conflict,
                            "Cannot delete a room with active or future reservations."));
                }

                // soft delete: mark the room as deleted instead of removing it from the database
                room.IsDeleted = true;

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

            // Room requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no Room with this ID to delete."));
        }


        private string ValidateRoom(Room room)
        {
            if (room == null)
            {
                return "Room data is required.";
            }

            if (room.RoomNumber <= 0)
            {
                return "Room number must be greater than 0.";
            }

            if (room.RoomNumber > 9999)
            {
                return "Room number is too large.";
            }

            if (room.Capacity <= 0)
            {
                return "Capacity must be greater than 0.";
            }

            if (room.Capacity > 10)
            {
                return "Room capacity cannot be greater than 10 guests.";
            }

            if (room.PricePerNight <= 0)
            {
                return "Price per night must be greater than 0.";
            }

            if (room.PricePerNight > 1000)
            {
                return "Price per night must be less than 1000.";
            }

            // check for already existing room number
            bool duplicateRoomNumber = dc.Rooms.Any(r =>
                 r.RoomNumber == room.RoomNumber &&
                 r.RoomId != room.RoomId &&
                 !r.IsDeleted); // ignores itself when editing

            if (duplicateRoomNumber)
            {
                return "A room with this number already exists.";
            }

            return null;
        }
    }
}