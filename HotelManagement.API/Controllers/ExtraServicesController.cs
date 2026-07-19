using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class ExtraServicesController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(
                ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/ExtraServices
        public IHttpActionResult Get()
        {
            return Ok(dc.ExtraServices.ToList());
        }

        // GET api/ExtraServices/5
        public IHttpActionResult Get(int id)
        {
            ExtraService ExtraService = dc.ExtraServices.FirstOrDefault(es => es.ExtraServiceId == id);

            if (ExtraService != null)
            {
                // response says the ExtraService was found and here it is
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, ExtraService));
            }
            // ExtraService not found, sends 404 default
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "ExtraService not found."));
        }


        // POST api/ExtraServices
        // Insert a ExtraService, returns ihttp result
        public IHttpActionResult Post([FromBody] ExtraService newExtraService)
        {
            string error = ValidateExtraService(newExtraService);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }
            // extraService doesn't exist yet, so add it
            dc.ExtraServices.InsertOnSubmit(newExtraService);

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

        // PUT api/ExtraServices/5
        public IHttpActionResult Put(int id, [FromBody] ExtraService editedExtraService)
        {
            string error = ValidateExtraService(editedExtraService);

            if (error != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, error));
            }

            ExtraService existingExtraService = dc.ExtraServices.FirstOrDefault(es => es.ExtraServiceId == id);

            if (existingExtraService == null) // ExtraService requested to change doesnt exist
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                    "There is no ExtraService with this ID to edit."));
            }

            // ExtraService found, so update it
            existingExtraService.Name = editedExtraService.Name;
            existingExtraService.Price = editedExtraService.Price;

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

        // DELETE api/ExtraServices/5
        // Added a route so this method is called when URL contains a ExtraServiceId
        //[Route("api/ExtraServices/{ExtraServiceId}")]
        public IHttpActionResult Delete(int id)
        {
            ExtraService extraService = dc.ExtraServices.FirstOrDefault(es => es.ExtraServiceId == id);

            if (extraService != null) // ExtraService requested to delete exists, so delete it
            {
                dc.ExtraServices.DeleteOnSubmit(extraService);

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
            // ExtraService requested to delete doesn't exist, so display error
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound,
                "There is no ExtraService with this ID to delete."));
        }

        private string ValidateExtraService(ExtraService extraService)
        {
            if (string.IsNullOrEmpty(extraService.Name))
            {
                return "Service name cannot be empty.";
            }

            if (extraService.Price <= 0)
            {
                return "Service price must be greater than zero.";
            }

            var existingService = dc.ExtraServices
                .FirstOrDefault(es => es.Name == extraService.Name);

            if (existingService != null)
            {
                return "A service with this name already exists.";
            }

            return null;
        }
    }
}