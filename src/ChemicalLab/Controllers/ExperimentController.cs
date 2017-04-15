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
    [Route("api/1.1/experiment")]
    public class ExperimentController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public ExperimentController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost]
        [Verify("EX:ADD")]
        public JsonResult Add([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidExperimant);
            var type = Database.Experiments.FirstOrDefault(e => e.Name == name);
            if (type != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            type = new Experiment {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.Experiments.Add(type);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewExperiment).By(CurrentUser).At(type.Id).From("").To("").Save();
            return Json(data: type.Id);
        }

        [HttpDelete("{id}")]
        [Verify("EX:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.Experiments.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidExperimant);
            if (Database.Items.Count(e => e.Experiment == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteExperiment).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Experiments.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{id}")]
        [Verify("EX:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Experiments.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidExperimant);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeExperimentName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
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
            return Json(data: Database.Experiments.Include(e => e.Items).Select(e => new {
                e.Id,
                e.Name,
                ItemDetails = e.Items.Count,
                Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToArray());
        }
    }
}