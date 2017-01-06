using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Authentication;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// Controller with support for user authentication service
    /// </summary>
    public class BasicVerifiableController : BasicController {
        /// <summary>
        /// Database context for EntityFrameworkCore
        /// </summary>
        protected readonly ChemicalLabContext Database;

        /// <summary>
        /// Authentication service
        /// </summary>
        protected readonly IAuthentication Verifier;

        /// <summary>
        /// Logging service
        /// </summary>
        protected readonly ITrackService Tracker;

        /// <summary>
        /// User of current session
        /// </summary>
        protected readonly User Session;

        public BasicVerifiableController(ChemicalLabContext database, IAuthentication verifier, ITrackService tracker) {
            Verifier = verifier;
            Tracker = tracker;
            Database = database;
            Session = FindUser();
        }

        /// <summary>
        /// Get user's instance by search its name<br />
        /// User's name is not case sensitive
        /// </summary>
        /// <param name="name">User's name</param>
        /// <returns>User's instance</returns>
        protected User FindUser(string name) {
            var lowerName = name.ToLower();
            return Database.Users.FirstOrDefault(e => e.Name == lowerName);
        }

        /// <summary>
        /// Get user's instance in current request
        /// </summary>
        /// <returns>User's instance</returns>
        protected User FindUser() {
            var token = Verifier.FindToken(Request.Headers);
            return Database.Users.FirstOrDefault(e => e.AccessToken == token);
        }

        /// <summary>
        /// Get group's instance by search its name<br />
        /// Usergroup's name is case sensitive
        /// </summary>
        /// <param name="name">Group's name</param>
        /// <returns></returns>
        protected UserGroup FindGroup(string name) {
            return Database.UserGroups.FirstOrDefault(e => e.Name == name);
        }

        /// <summary>
        /// Get group's instance of given user
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <returns></returns>
        protected UserGroup FindGroup(User user) {
            return user.UserGroupNavigation ?? Database.UserGroups.FirstOrDefault(e => e.Id == user.UserGroup);
        }

        /// <summary>
        /// Compare user's name
        /// </summary>
        /// <param name="nameA">UserA's name</param>
        /// <param name="nameB">UserB's name</param>
        /// <returns>Is the name equal</returns>
        protected static bool IsUserNameEqual(string nameA, string nameB) {
            return string.Equals(nameA, nameB, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <returns>Verification result</returns>
        protected VerifyResult Verify(User user, string permissionGroup) {
            var result = Verifier.Verify(user, permissionGroup, HttpContext.Connection.RemoteIpAddress);
            if (result == VerifyResult.Denied)
                Tracker.Write(new TrackRecord(TrackType.ErrorL2, user).Add("p", permissionGroup));
            else if (result == VerifyResult.Passed)
                Tracker.Write(new TrackRecord(TrackType.InfoL2, user).Add("p", permissionGroup));
            return result;
        }

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <param name="verifyResult">Variable to handle verification result</param>
        /// <returns>Is the verification passed</returns>
        protected bool Verify(User user, string permissionGroup, out VerifyResult verifyResult) {
            verifyResult = Verifier.Verify(user, permissionGroup, HttpContext.Connection.RemoteIpAddress);
            if (verifyResult == VerifyResult.Denied)
                Tracker.Write(new TrackRecord(TrackType.ErrorL2, user).Add("p", permissionGroup));
            else if (verifyResult == VerifyResult.Passed)
                Tracker.Write(new TrackRecord(TrackType.InfoL2, user).Add("p", permissionGroup));
            return verifyResult == VerifyResult.Passed;
        }

        /// <summary>
        /// Get a regular EKIFVK json response with Http status code 403 and message is verification result
        /// </summary>
        /// <param name="result">Verification result</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        protected JsonResult Denied(VerifyResult result, object data = null) {
            return BasicResponse(StatusCodes.Status403Forbidden, Verifier.ToString(result), data);
        }

        /// <summary>
        /// Get a regular EKIFVK json response with Http status code 403 and message is VerifyResult.NonexistentToken
        /// </summary>
        /// <returns>Json response</returns>
        protected JsonResult NonexistentToken() {
            return BasicResponse(StatusCodes.Status403Forbidden, Verifier.ToString(VerifyResult.NonexistentToken));
        }
    }
}