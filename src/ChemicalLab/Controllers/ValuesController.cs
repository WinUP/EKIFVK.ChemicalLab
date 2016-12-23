using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Authentication;

namespace EKIFVK.ChemicalLab.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ChemicalLabContext _database;
        private readonly IAuthentication _verifier;

        public ValuesController(ChemicalLabContext database, IAuthentication verifier)
        {
            _database = database;
            _verifier = verifier;
        }

        protected User FindUser(string name)
        {
            return _database.Users.FirstOrDefault(e => e.Name == name);
        }

        protected User FindUser()
        {
            var token = _verifier.FindToken(Request.Headers);
            return _database.Users.FirstOrDefault(e => e.AccessToken == token);
        }

        // GET api/values
        [HttpGet]
        public string Get()
        {
            var verifyResult = _verifier.ToString(_verifier.Verify(FindUser("WinUP"), "DATA:VIEW", HttpContext.Connection.RemoteIpAddress.ToString()));
            return verifyResult;
            //! PASSED
            // Where, Select, ToArray, FK, ADD, REMOVE, UPDATE
            //! NOT PASS
            // Take, Skip
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
