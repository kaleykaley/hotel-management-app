using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    // DOESN'T NEED COMPLETE CRUD
    public class ExtraServicesController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        // GET api/ExtraServices
        public IHttpActionResult Get()
        {
            return Ok(dc.ExtraServices.ToList());
        }

        // GET api/ExtraServices/5
        public IHttpActionResult Get(int id)
        {
            ExtraService service = dc.ExtraServices
                                     .FirstOrDefault(es => es.ExtraServiceId == id);

            if (service == null)
                return NotFound();

            return Ok(service);
        }
    }
}