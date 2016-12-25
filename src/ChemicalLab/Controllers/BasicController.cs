using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EKIFVK.ChemicalLab.Controllers
{
    public class BasicController : Controller
    {
        protected JsonResult BasicResponse(int statusCode = StatusCodes.Status200OK, string message = null, object data = null)
        {
            Response.StatusCode = statusCode;
            return Json(new Hashtable {{"data", data ?? ""}, {"message", message ?? "SUCCESS"}});
        }
    }
}
