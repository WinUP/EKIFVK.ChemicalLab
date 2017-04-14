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
    [Route("api/1.1/containertype")]
    public class ContainerTypeController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public ContainerTypeController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost]
        [Verify("CT:ADD")]
        public JsonResult Add([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidContainerType);
            var type = Database.ContainterTypes.FirstOrDefault(e => e.Name == name);
            if (type != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            type = new ContainterType {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.ContainterTypes.Add(type);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewContainerType).By(CurrentUser).At(type.Id).From("").To("").Save();
            return Json(data: type.Id);
        }

        [HttpDelete("{id}")]
        [Verify("CT:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.ContainterTypes.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidContainerType);
            if (Database.ItemDetails.Count(e => e.ContainerType == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteContainerType).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.ContainterTypes.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{id}")]
        [Verify("CT:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.ContainterTypes.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidContainerType);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeContainerTypeName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
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
            return Json(data: Database.ContainterTypes.Include(e => e.ItemDetails).Select(e => new {
                e.Id,
                e.Name,
                ItemDetails = e.ItemDetails.Count,
                Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToArray());
        }
    }
}
