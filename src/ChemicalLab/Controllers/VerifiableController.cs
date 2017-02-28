using System.Collections;
using System.Linq;
using System.Net;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Verification;
using EKIFVK.ChemicalLab.Services.Tracking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// Controller with support of EKIFVK regular json response
    /// </summary>
    public abstract class VerifiableController : Controller {
        /// <summary>
        /// Database
        /// </summary>
        public ChemicalLabContext Database { get; }

        /// <summary>
        /// Authentication service
        /// </summary>
        public IVerificationService Verifier { get; }

        /// <summary>
        /// Logging service
        /// </summary>
        public readonly ITrackService Tracker;

        /// <summary>
        /// User in this session (might be null)
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// IP address in this session
        /// </summary>
        public IPAddress CurrentAddress { get; private set; }

        protected VerifiableController(ChemicalLabContext database, IVerificationService verifier, ITrackService tracker) {
            Verifier = verifier;
            Tracker = tracker;
            Database = database;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            var token = Verifier.ExtractToken(Request.Headers);
            if (token != null) {
                CurrentUser = Database.Users.FirstOrDefault(e => e.AccessToken == token);
            }
            CurrentAddress = context.HttpContext.Connection.RemoteIpAddress;
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Get a formatted json response
        /// </summary>
        /// <param name="statusCode">Http status code (default 200)</param>
        /// <param name="message">Message which should be returned (default null)</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        public JsonResult FormattedResponse(int statusCode = StatusCodes.Status200OK, string message = null,
            object data = null) {
            Response.StatusCode = statusCode;
            return Json(new Hashtable {{"data", data ?? ""}, {"message", message ?? "SUCCESS"}});
        }

        /// <summary>
        /// Get a formatted json response with permission rejected message and write this message to Tracker
        /// </summary>
        /// <param name="verificationResult">Verification result</param>
        /// <param name="permission">Requested permission</param>
        /// <returns>Json response</returns>
        public JsonResult RejectedResponse(VerificationResult verificationResult, string permission) {
            WritePermissionRejected(verificationResult, permission);
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Json(new Hashtable { { "data", "" }, { "message", Verifier.ToString(verificationResult) } });
        }

        /// <summary>
        /// Write permission rejected message to Tracker
        /// </summary>
        /// <param name="permission">Requested permission</param>
        /// <param name="verificationResult">Verification result</param>
        public void WritePermissionRejected(VerificationResult verificationResult, string permission) {
            Tracker.Write(new TrackRecord(TrackType.E2P, CurrentUser, "", 0, "").Note($"Permission rejected in [{permission}] because {Verifier.ToString(verificationResult)}"));
        }
    }
}