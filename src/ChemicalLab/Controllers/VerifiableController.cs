using System;
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
    /// Controller with support of verification
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
        /// Tracking service
        /// </summary>
        public readonly ITrackerService Tracker;

        /// <summary>
        /// User in this session (might be null)
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// IP address in this session
        /// </summary>
        public IPAddress CurrentAddress { get; private set; }

        protected VerifiableController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker) {
            Verifier = verifier;
            Tracker = tracker;
            Database = database;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            var token = Verifier.FindToken(Request.Headers);
            if (token != null) {
                CurrentUser = Database.Users.FirstOrDefault(e => e.AccessToken == token);
            }
            CurrentAddress = context.HttpContext.Connection.RemoteIpAddress;
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Check if a name is valid for database
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public bool IsNameValid(string name) {
            return !string.IsNullOrEmpty(name) &&
                   name.IndexOf("?", StringComparison.Ordinal) < 0 &&
                   name.IndexOf("/", StringComparison.Ordinal) < 0 &&
                   name.IndexOf("\\", StringComparison.Ordinal) < 0 &&
                   name.IndexOf(".", StringComparison.Ordinal) != 0;
        }

        /// <summary>
        /// Get a formatted json response
        /// </summary>
        /// <param name="statusCode">Http status code (default 200)</param>
        /// <param name="message">Message which should be returned (default null)</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        public JsonResult Json(int statusCode = StatusCodes.Status200OK, string message = null, object data = null) {
            Response.StatusCode = statusCode;
            return Json(new Hashtable {{"data", data ?? ""}, {"message", message ?? "SUCCESS"}});
        }

        /// <summary>
        /// Get a formatted json response
        /// </summary>
        /// <param name="ex">Server error</param>
        /// <returns></returns>
        public JsonResult Json(Exception ex) {
            return Json(StatusCodes.Status500InternalServerError, ex.Message, "SERVER_ERROR");
        }

        /// <summary>
        /// Get a formatted json response
        /// </summary>
        /// <param name="verificationResult">Permission verification result</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        public JsonResult Json(VerificationResult verificationResult, object data = null) {
            return Json(StatusCodes.Status403Forbidden, Verifier.ToString(verificationResult), data);
        }
    }
}