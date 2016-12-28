using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EKIFVK.ChemicalLab.Controllers
{
    /// <summary>
    /// Controller with support of EKIFVK regular json response
    /// </summary>
    public class BasicController : Controller
    {
        /// <summary>
        /// Get a regular EKIFVK json response
        /// </summary>
        /// <param name="statusCode">Http status code (default 200)</param>
        /// <param name="message">Message which should be returned (default null)</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        protected JsonResult BasicResponse(int statusCode = StatusCodes.Status200OK, string message = null, object data = null)
        {
            Response.StatusCode = statusCode;
            return Json(new Hashtable {{"d", data ?? ""}, {"m", message ?? "SUCCESS"}});
        }
    }
}
