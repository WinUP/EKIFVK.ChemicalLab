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
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/unit")]
    public class UnitController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public UnitController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost]
        [Verify("UN:ADD")]
        public JsonResult Add([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidUnit);
            var unit = Database.Units.FirstOrDefault(e => e.Name == name);
            if (unit != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            unit = new Unit {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.Units.Add(unit);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewUnit).By(CurrentUser).At(unit.Id).From("").To("").Save();
            return Json(data: unit.Id);
        }

        [HttpDelete("{id}")]
        [Verify("UN:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.Units.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUnit);
            if (Database.ItemDetails.Count(e => e.Unit == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteUnit).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Units.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{id}")]
        [Verify("UN:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Units.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUnit);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeUnitName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                    target.Name = param["name"].ToString();
                }).To(target.Name).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet]
        [Verify("")]
        public JsonResult GetList() {
            return Json(data: Database.Units.Select(e => new {
                e.Id,
                e.Name,
                Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToArray());
        }
    }
}