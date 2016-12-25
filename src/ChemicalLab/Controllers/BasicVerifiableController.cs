using System;
using System.Linq;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EKIFVK.ChemicalLab.Controllers
{
    public class BasicVerifiableController : BasicController
    {
        protected readonly ChemicalLabContext Database;
        protected readonly IAuthentication Verifier;
        protected readonly IOptions<UserModuleConfiguration> Configuration;

        public BasicVerifiableController(ChemicalLabContext database, IAuthentication verifier, IOptions<UserModuleConfiguration> configuration)
        {
            Verifier = verifier;
            Database = database;
            Configuration = configuration;
        }

        protected User FindUser(string name)
        {
            var lowerName = name.ToLower();
            return Database.Users.FirstOrDefault(e => e.Name == lowerName);
        }

        protected User FindUser()
        {
            var token = Verifier.FindToken(Request.Headers);
            return Database.Users.FirstOrDefault(e => e.AccessToken == token);
        }

        protected VerifyResult Verify(User user, string permission)
        {
            return Verifier.Verify(user, permission, HttpContext.Connection.RemoteIpAddress.ToString());
        }

        protected VerifyResult Verify(User user, string[] permissions)
        {
            return Verifier.Verify(user, permissions, HttpContext.Connection.RemoteIpAddress.ToString());
        }

        protected bool Verify(User user, string[] permissions, out VerifyResult verifyResult)
        {
            verifyResult = Verifier.Verify(user, permissions, HttpContext.Connection.RemoteIpAddress.ToString());
            return verifyResult == VerifyResult.Passed;
        }

        protected bool Verify(User user, string permissions, out VerifyResult verifyResult)
        {
            verifyResult = Verifier.Verify(user, permissions, HttpContext.Connection.RemoteIpAddress.ToString());
            return verifyResult == VerifyResult.Passed;
        }

        protected JsonResult Basic403(VerifyResult result)
        {
            return BasicResponse(StatusCodes.Status403Forbidden, Verifier.ToString(result));
        }
    }
}
