using System;
using System.Collections;
using System.Linq;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/vendor")]
    public class VendorController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public VendorController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost]
        [Verify("VD:ADD")]
        public JsonResult Add([FromBody] Hashtable parameter) {
            var number = parameter["number"].ToString();
            var name = parameter["name"].ToString();
            if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidVendor);
            var type = Database.Vendors.FirstOrDefault(e => e.Number == number);
            if (type != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            type = new Vendor {
                Name =  name,
                Number = number,
                LastUpdate = DateTime.Now
            };
            Database.Vendors.Add(type);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewVendor).By(CurrentUser).At(type.Id).From("").To("").Save();
            return Json(data: type.Id);
        }

        [HttpDelete("{id}")]
        [Verify("VD:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.Vendors.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidVendor);
            if (Database.Items.Count(e => e.Vendor == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteVendor).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Vendors.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        //! Change Number is not allowed
        [HttpPatch("{id}")]
        [Verify("VD:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Vendors.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidVendor);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeVendorName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                    target.Name = param["name"].ToString();
                }).To(target.Name).Save(false);
            }
            if (param.ContainsKey("disabled")) {
                data["disabled"] = true;
                Tracker.Get(Operation.ChangeVendorDisabled).By(CurrentUser).At(target.Id).From(target.Disabled.ToString()).Do(() => {
                    target.Disabled = (bool) param["disabled"];
                }).To(target.Disabled.ToString()).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet]
        [Verify("")]
        public JsonResult GetList() {
            return Json(data: Database.Vendors.Include(e => e.Items).Select(e => new {
                e.Id,
                e.Name,
                e.Number,
                ItemDetails = e.Items.Count,
                Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToArray());
        }
    }
}