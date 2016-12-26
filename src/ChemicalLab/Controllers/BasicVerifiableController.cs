using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Authentication;

namespace EKIFVK.ChemicalLab.Controllers
{
    /// <summary>
    /// Controller with support for user authentication service
    /// </summary>
    public class BasicVerifiableController : BasicController
    {
        /// <summary>
        /// Database context for EntityFrameworkCore
        /// </summary>
        protected readonly ChemicalLabContext Database;
        /// <summary>
        /// Authentication service
        /// </summary>
        protected readonly IAuthentication Verifier;

        public BasicVerifiableController(ChemicalLabContext database, IAuthentication verifier)
        {
            Verifier = verifier;
            Database = database;
        }

        /// <summary>
        /// Get user's instance by search user's name
        /// </summary>
        /// <param name="name">User's name</param>
        /// <returns>User's instance</returns>
        protected User FindUser(string name)
        {
            var lowerName = name.ToLower();
            return Database.Users.FirstOrDefault(e => e.Name == lowerName);
        }

        /// <summary>
        /// Get user's instance in current request
        /// </summary>
        /// <returns>User's instance</returns>
        protected User FindUser()
        {
            var token = Verifier.FindToken(Request.Headers);
            return Database.Users.FirstOrDefault(e => e.AccessToken == token);
        }

        /// <summary>
        /// Compare user's name
        /// </summary>
        /// <param name="nameA">UserA's name</param>
        /// <param name="nameB">UserB's name</param>
        /// <returns>Is the name equal</returns>
        protected static bool IsNameEqual(string nameA, string nameB)
        {
            return string.Equals(nameA, nameB, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <returns>Verification result</returns>
        protected VerifyResult Verify(User user, string permissionGroup)
        {
            return Verifier.Verify(user, permissionGroup, HttpContext.Connection.RemoteIpAddress);
        }

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <param name="verifyResult">Variable to handle verification result</param>
        /// <returns>Is the verification passed</returns>
        protected bool Verify(User user, string permissionGroup, out VerifyResult verifyResult)
        {
            verifyResult = Verifier.Verify(user, permissionGroup, HttpContext.Connection.RemoteIpAddress);
            return verifyResult == VerifyResult.Passed;
        }

        /// <summary>
        /// Get a regular EKIFVK json response with Http status code 403 and message is verification result
        /// </summary>
        /// <param name="result">Verification result</param>
        /// <param name="data">Data which should be returned (default null)</param>
        /// <returns>Json response</returns>
        protected JsonResult Basic403(VerifyResult result, object data = null)
        {
            return BasicResponse(StatusCodes.Status403Forbidden, Verifier.ToString(result), data);
        }

        /// <summary>
        /// Get a regular EKIFVK json response with Http status code 403 and message is VerifyResult.NonexistentToken
        /// </summary>
        /// <returns>Json response</returns>
        protected JsonResult Basic403NonexistentToken()
        {
            return BasicResponse(StatusCodes.Status403Forbidden, Verifier.ToString(VerifyResult.NonexistentToken));
        }
    }
}
