using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.SearchFilters;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/detail")]
    public class ItemDetailController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public ItemDetailController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpGet("{id}")]
        [Verify("")]
        public JsonResult GetInfo(int id) {
            var target = Database.ItemDetails.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetail);
            return Json(data: Database.ItemDetails
                .Include(e => e.ContainterTypeNavigation)
                .Include(e => e.DetailTypeNavigation)
                .Include(e => e.UnitNavigation)
                .Include(e => e.PhysicalState)
                .Include(e => e.Items)
                .Select(e => new {
                    e.Prefix,
                    e.Name,
                    e.Cas,
                    Unit = e.UnitNavigation.Name,
                    e.Size,
                    Container = e.ContainterTypeNavigation.Name,
                    State = e.PhysicalStateNavigation.Name,
                    Type = e.DetailTypeNavigation.Name,
                    e.Msds,
                    e.MsdsDate,
                    e.Required,
                    e.Note,
                    e.Disabled,
                    Items = e.Items.Count,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray());
        }

        [HttpPost]
        [Verify("ID:ADD")]
        public JsonResult Add([FromBody] Hashtable param) {
            var typeId = (int) param["type"];
            var type = Database.ItemDetailTypes.FirstOrDefault(e => e.Id == typeId);
            if (type == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetailType);
            var prefix = param["prefix"].ToString();
            var name = param["name"].ToString();
            var cas = param.ContainsKey("cas") ? param["cas"].ToString() : null;
            if (type.RequireCas) {
                if (cas == null)
                    return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidCas);
                if (Database.ItemDetails.Count(e => e.Cas == cas) > 0)
                    return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            }
            var id = (int) param["unit"];
            var unit = Database.Units.FirstOrDefault(e => e.Id == id);
            if (unit == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUnit);
            id = (int) param["container"];
            var container = Database.ContainterTypes.FirstOrDefault(e => e.Id == id);
            if (container == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidContainerType);
            id = (int) param["state"];
            var state = Database.PhysicalStates.FirstOrDefault(e => e.Id == id);
            if (state == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPhysicalState);
            var target = new ItemDetail {
                Prefix = prefix,
                Name = name,
                UnitNavigation = unit,
                Size = (double) param["size"],
                ContainterTypeNavigation = container,
                PhysicalStateNavigation = state,
                DetailTypeNavigation = type,
                MsdsDate = DateTime.Parse(param["msdsDate"].ToString()),
                Required = (int) param["required"],
                Note = param["note"].ToString(),
                Disabled = false,
                LastUpdate = DateTime.Now
            };
            if (type.RequireCas) target.Cas = cas;
            Database.ItemDetails.Add(target);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewItemDetail).By(CurrentUser).At(target.Id).From("").To("").Save();
            return Json(data: target.Id);
        }

        [HttpDelete("{id}")]
        [Verify("ID:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.ItemDetails.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetail);
            if (Database.Items.Count(e => e.Detail == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteItemDetail).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.ItemDetails.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        //! Changing CAS or DetailType is not allowed
        [HttpPatch("{id}")]
        [Verify("ID:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param) {
            var target = Database.ItemDetails.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetail);
            var data = new Hashtable();
            if (param.ContainsKey("prefix")) {
                data["prefix"] = true;
                Tracker.Get(Operation.ChangeItemDetailPrefix).By(CurrentUser).At(target.Id).From(target.Prefix).Do(() => {
                    target.Prefix = param["prefix"].ToString();
                }).To(target.Prefix).Save(false);
            }
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeItemDetailName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                    target.Name = param["name"].ToString();
                }).To(target.Name).Save(false);
            }
            if (param.ContainsKey("unit")) {
                data["unit"] = true;
                var unitId = (int) param["unit"];
                var unit = Database.Units.FirstOrDefault(e => e.Id == unitId);
                if (unit == null)
                    data["unit"] = Configuration.Value.InvalidUnit;
                else
                    Tracker.Get(Operation.ChangeItemDetailUnit).By(CurrentUser).At(target.Id).From(target.Unit.ToString()).Do(() => {
                        target.UnitNavigation = unit;
                    }).To(unitId.ToString()).Save(false);
            }
            if (param.ContainsKey("size")) {
                data["size"] = true;
                Tracker.Get(Operation.ChangeItemDetailName).By(CurrentUser).At(target.Id).From(target.Size.ToString()).Do(() => {
                    target.Size = (double) param["size"];
                }).To(target.Size.ToString()).Save(false);
            }
            if (param.ContainsKey("container")) {
                data["container"] = true;
                var containerId = (int)param["container"];
                var container = Database.ContainterTypes.FirstOrDefault(e => e.Id == containerId);
                if (container == null)
                    data["container"] = Configuration.Value.InvalidContainerType;
                else
                    Tracker.Get(Operation.ChangeItemDetailContainerType).By(CurrentUser).At(target.Id).From(target.ContainerType.ToString()).Do(() => {
                        target.ContainterTypeNavigation = container;
                    }).To(containerId.ToString()).Save(false);
            }
            if (param.ContainsKey("state")) {
                data["state"] = true;
                var stateId = (int)param["state"];
                var state = Database.PhysicalStates.FirstOrDefault(e => e.Id == stateId);
                if (state == null)
                    data["state"] = Configuration.Value.InvalidPhysicalState;
                else
                    Tracker.Get(Operation.ChangeItemDetailPhysicalState).By(CurrentUser).At(target.Id).From(target.PhysicalState.ToString()).Do(() => {
                        target.PhysicalStateNavigation = state;
                    }).To(stateId.ToString()).Save(false);
            }
            if (param.ContainsKey("msds")) {
                data["msds"] = true;
                Tracker.Get(Operation.ChangeItemDetailMsds).By(CurrentUser).At(target.Id).From(target.Msds).Do(() => {
                    target.Msds = param["msds"].ToString();
                }).To(target.Msds).Save(false);
            }
            if (param.ContainsKey("msdsDate")) {
                data["msdsDate"] = true;
                Tracker.Get(Operation.ChangeItemDetailMsdsDate).By(CurrentUser).At(target.Id).From(target.MsdsDate.ToString("yyyy-MM-dd HH:mm:ss")).Do(() => {
                    target.MsdsDate = DateTime.Parse(param["msdsDate"].ToString());
                }).To(target.MsdsDate.ToString("yyyy-MM-dd HH:mm:ss")).Save(false);
            }
            if (param.ContainsKey("required")) {
                data["required"] = true;
                Tracker.Get(Operation.ChangeItemDetailRequired).By(CurrentUser).At(target.Id).From(target.Required.ToString()).Do(() => {
                    target.Required = (int) param["required"];
                }).To(target.Required.ToString()).Save(false);
            }
            if (param.ContainsKey("note")) {
                data["note"] = true;
                Tracker.Get(Operation.ChangeItemDetailNote).By(CurrentUser).At(target.Id).From(target.Note).Do(() => {
                    target.Note = param["note"].ToString();
                }).To(target.Note).Save(false);
            }
            if (param.ContainsKey("disabled")) {
                data["disabled"] = true;
                Tracker.Get(Operation.ChangeItemDetailDisabled).By(CurrentUser).At(target.Id).From(target.Disabled.ToString()).Do(() => {
                    target.Disabled = (bool) param["disabled"];
                }).To(target.Disabled.ToString()).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet(".count")]
        [Verify("ID:MANAGE")]
        public JsonResult GetCount(ItemDetailSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.ItemDetails.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [Verify("ID:MANAGE")]
        public JsonResult GetList(ItemDetailSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.ItemDetails.FromSql(query, param.ToArray())
                .Include(e => e.ContainterTypeNavigation)
                .Include(e => e.DetailTypeNavigation)
                .Include(e => e.UnitNavigation)
                .Include(e => e.PhysicalState)
                .Include(e => e.Items)
                .Select(e => new {
                    e.Prefix,
                    e.Name,
                    e.Cas,
                    Unit = e.UnitNavigation.Name,
                    e.Size,
                    Container = e.ContainterTypeNavigation.Name,
                    State = e.PhysicalStateNavigation.Name,
                    Type = e.DetailTypeNavigation.Name,
                    e.Msds,
                    e.MsdsDate,
                    e.Required,
                    e.Note,
                    e.Disabled,
                    Items = e.Items.Count,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray());
        }

        private static string QueryGenerator(ItemDetailSearchFilter filter, ICollection<object> param) {
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.Prefix)) {
                condition.Add("Prefix LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Prefix);
            }
            if (!string.IsNullOrEmpty(filter.Name)) {
                condition.Add("Name LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Name);
            }
            if (!string.IsNullOrEmpty(filter.Cas)) {
                condition.Add("CAS LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Cas);
            }
            if (filter.Unit.HasValue) {
                condition.Add("Unit = @p" + ++paramCount);
                param.Add(filter.Unit.Value);
            }
            if (filter.SizeMin.HasValue) {
                condition.Add("Size >= @p" + ++paramCount);
                param.Add(filter.SizeMin.Value);
            }
            if (filter.SizeMax.HasValue) {
                condition.Add("Size <= @p" + ++paramCount);
                param.Add(filter.SizeMax.Value);
            }
            if (filter.Container.HasValue) {
                condition.Add("ContainerType = @p" + ++paramCount);
                param.Add(filter.Container.Value);
            }
            if (filter.State.HasValue) {
                condition.Add("PhysicalState = @p" + ++paramCount);
                param.Add(filter.State.Value);
            }
            if (filter.Detail.HasValue) {
                condition.Add("DetailType = @p" + ++paramCount);
                param.Add(filter.Detail.Value);
            }
            if (!string.IsNullOrEmpty(filter.Msds)) {
                condition.Add("Msds LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Msds);
            }
            if (!string.IsNullOrEmpty(filter.MsdsDateMin)) {
                condition.Add("MsdsDate >= @p" + ++paramCount);
                param.Add(filter.MsdsDateMin);
            }
            if (!string.IsNullOrEmpty(filter.MsdsDateMax)) {
                condition.Add("MsdsDate <= @p" + ++paramCount);
                param.Add(filter.MsdsDateMax);
            }
            if (filter.RequiredMin.HasValue) {
                condition.Add("Required >= @p" + ++paramCount);
                param.Add(filter.RequiredMin.Value);
            }
            if (filter.RequiredMax.HasValue) {
                condition.Add("Required <= @p" + ++paramCount);
                param.Add(filter.RequiredMax.Value);
            }
            if (filter.Disabled.HasValue) {
                condition.Add("Disabled = @p" + ++paramCount);
                param.Add(filter.Disabled.Value ? 1 : 0);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0) {
                query = "SELECT * FROM ItemDetail WHERE ID >= (SELECT ID FROM ItemDetail WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM ItemDetail WHERE " + query;
            if (filter.Take.HasValue) {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}
