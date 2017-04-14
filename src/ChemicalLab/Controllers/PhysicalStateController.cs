using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/physicaltype")]
    public class PhysicalStateController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public PhysicalStateController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost]
        [Verify("PS:ADD")]
        public JsonResult Add([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidPhysicalState);
            var state = Database.PhysicalStates.FirstOrDefault(e => e.Name == name);
            if (state != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            state = new PhysicalState {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.PhysicalStates.Add(state);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewPhysicalState).By(CurrentUser).At(state.Id).From("").To("").Save();
            return Json(data: state.Id);
        }

        [HttpDelete("{id}")]
        [Verify("PS:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.PhysicalStates.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPhysicalState);
            if (Database.ItemDetails.Count(e => e.PhysicalState == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeletePhysicalState).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.PhysicalStates.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{id}")]
        [Verify("PS:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.PhysicalStates.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPhysicalState);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangePhysicalStateName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
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