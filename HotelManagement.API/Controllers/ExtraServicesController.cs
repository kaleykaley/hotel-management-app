using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace HotelManagement.API.Controllers
{
    public class ExtraServicesController : ApiController
    {
        private readonly HotelDataDataContext dc =
            new HotelDataDataContext(ConfigurationManager.ConnectionStrings["HotelManagementConnectionString"].ConnectionString);

        /// <summary>
        /// Retrieves all extra services
        /// </summary>
        /// <returns>HTTPS response</returns>
        // GET api/ExtraServices
        public IHttpActionResult Get()
        {
            return Ok(dc.ExtraServices.ToList());
        }

        /// <summary>
        /// Retrieves a specific extra service by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HTTP response</returns>
        // GET api/ExtraServices/5
        public IHttpActionResult Get(int id)
        {
            ExtraService service = dc.ExtraServices.FirstOrDefault(es => es.ExtraServiceId == id);

            if (service == null)
                return NotFound();

            return Ok(service);
        }
    }
}