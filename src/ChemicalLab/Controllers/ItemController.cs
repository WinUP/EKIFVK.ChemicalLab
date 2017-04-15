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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/item")]
    public class ItemController : VerifiableController {
        private readonly IOptions<LabModule> Configuration;

        public ItemController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<LabModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpGet("{id}")]
        [Verify("")]
        public JsonResult GetInfo(int id) {
            return Json(data: Database.Items
                .Where(e => e.Id == id)
                .Include(e => e.OwnerNavigation)
                .Include(e => e.ExperimentNavigation)
                .Include(e => e.VendorNavigation)
                .Select(e => new {
                    e.Detail,
                    RegisterTime = e.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReceivedTime = e.ReceivedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    OpenedTime = e.OpenedTime.HasValue ? e.OpenedTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    e.Location,
                    Ownder = e.OwnerNavigation.Name,
                    Experiment = e.ExperimentNavigation.Name,
                    Vendor = e.VendorNavigation.Number,
                    e.Used,
                    e.Disabled,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray());
        }

        [HttpPost]
        [Verify("IT:ADD")]
        public JsonResult Add([FromBody] Hashtable param) {
            var id = (int) param["detail"];
            var detail = Database.ItemDetails.FirstOrDefault(e => e.Id == id);
            if (detail == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetail);
            if (detail.Disabled)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            id = (int)param["location"];
            var location = Database.Locations.FirstOrDefault(e => e.Id == id);
            if (location == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidLocation);
            id = (int)param["experiment"];
            var experiment = Database.Experiments.FirstOrDefault(e => e.Id == id);
            if (experiment == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidExperimant);
            id = (int)param["vendor"];
            var vendor = Database.Vendors.FirstOrDefault(e => e.Id == id);
            if (vendor == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidVendor);
            if (vendor.Disabled)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            var amount = (int) param["amount"];
            var ids = new List<int>();
            for (var i = 0; i < amount; i++) {
                var target = new Item {
                    DetailNavigation = detail,
                    RegisterTime = DateTime.Now,
                    ReceivedTime = DateTime.Parse(param["receiverTime"].ToString()),
                    LocationNavigation = location,
                    OwnerNavigation = CurrentUser,
                    ExperimentNavigation = experiment,
                    VendorNavigation = vendor,
                    Used = (double) param["used"],
                    Disabled = false,
                    LastUpdate = DateTime.Now
                };
                if (param.ContainsKey("openedTime"))
                    target.OpenedTime = DateTime.Parse(param["openedTime"].ToString());
                Database.Items.Add(target);
                Database.SaveChanges();
                ids.Add(target.Id);
                Tracker.Get(Operation.AddNewItem).By(CurrentUser).At(target.Id).From("").To("").Save();
            }
            return Json(data: ids.ToArray());
        }

        [HttpDelete("{id}")]
        [Verify("IT:DELETE")]
        public JsonResult Delete(int id) {
            var target = Database.Items.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidDetail);
            try {
                Tracker.Get(Operation.DeleteItem).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Items.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        //! Changing Detail, RegisterTime, ReceivedTime, or Owner is not allowed
        [HttpPatch("{id}")]
        [Verify("ID:MANAGE")]
        public JsonResult ChangeInformation(int id, [FromBody] Hashtable param)  {
            var target = Database.Items.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidItem);
            if (target.Owner != CurrentUser.Id)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            var data = new Hashtable();
            if (param.ContainsKey("openedTime")) {
                data["openedTime"] = true;
                Tracker.Get(Operation.ChangeItemOpenedTime).By(CurrentUser).At(target.Id).From(target.OpenedTime?.ToString("yyyy-MM-dd HH:mm:ss")).Do(() => {
                    target.OpenedTime = DateTime.Parse(param["openedTime"].ToString());
                }).To(target.OpenedTime?.ToString("yyyy-MM-dd HH:mm:ss")).Save(false);
            }
            if (param.ContainsKey("location")) {
                data["location"] = true;
                var locationId = (int)param["location"];
                var location = Database.Locations.FirstOrDefault(e => e.Id == locationId);
                if (location == null)
                    data["location"] = Configuration.Value.InvalidLocation;
                else
                    Tracker.Get(Operation.ChangeItemLocation).By(CurrentUser).At(target.Id).From(target.Location.ToString()).Do(() => {
                        target.LocationNavigation = location;
                    }).To(locationId.ToString()).Save(false);
            }
            if (param.ContainsKey("experiment")) {
                data["experiment"] = true;
                var experimentId = (int)param["experiment"];
                var experiment = Database.Experiments.FirstOrDefault(e => e.Id == experimentId);
                if (experiment == null)
                    data["location"] = Configuration.Value.InvalidExperimant;
                else
                    Tracker.Get(Operation.ChangeItemExperiment).By(CurrentUser).At(target.Id).From(target.Experiment.ToString()).Do(() => {
                        target.ExperimentNavigation = experiment;
                    }).To(experimentId.ToString()).Save(false);
            }
            if (param.ContainsKey("vendor")) {
                data["vendor"] = true;
                var vendorId = (int)param["vendor"];
                var vendor = Database.Vendors.FirstOrDefault(e => e.Id == vendorId);
                if (vendor == null)
                    data["vendor"] = Configuration.Value.InvalidVendor;
                else
                    Tracker.Get(Operation.ChangeItemVendor).By(CurrentUser).At(target.Id).From(target.Vendor.ToString()).Do(() => {
                        target.VendorNavigation = vendor;
                    }).To(vendorId.ToString()).Save(false);
            }
            if (param.ContainsKey("used")) {
                data["used"] = true;
                Tracker.Get(Operation.ChangeItemUsed).By(CurrentUser).At(target.Id).From(target.Used.ToString()).Do(() => {
                    target.Used = (double)param["size"];
                }).To(target.Used.ToString()).Save(false);
            }
            if (param.ContainsKey("disabled"))  {
                data["disabled"] = true;
                Tracker.Get(Operation.ChangeItemDisabled).By(CurrentUser).At(target.Id).From(target.Disabled.ToString()).Do(() => {
                    target.Disabled = (bool)param["disabled"];
                }).To(target.Disabled.ToString()).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet(".count")]
        [Verify("IT:MANAGE")]
        public JsonResult GetCount(ItemSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.Items.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [Verify("IT:MANAGE")]
        public JsonResult GetList(ItemSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.Items.FromSql(query, param.ToArray())
                .Include(e => e.OwnerNavigation)
                .Include(e => e.ExperimentNavigation)
                .Include(e => e.VendorNavigation)
                .Select(e => new {
                    e.Detail,
                    RegisterTime = e.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReceivedTime = e.ReceivedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    OpenedTime = e.OpenedTime.HasValue ? e.OpenedTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    e.Location,
                    Ownder = e.OwnerNavigation.Name,
                    Experiment = e.ExperimentNavigation.Name,
                    Vendor = e.VendorNavigation.Number,
                    e.Used,
                    e.Disabled,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray());
        }

        private string QueryGenerator(ItemSearchFilter filter, ICollection<object> param) {
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.ReceivedTimeMin)) {
                condition.Add("ReceivedTime >= @p" + ++paramCount);
                param.Add(filter.ReceivedTimeMin);
            }
            if (!string.IsNullOrEmpty(filter.ReceivedTimeMax)) {
                condition.Add("ReceivedTime <= @p" + ++paramCount);
                param.Add(filter.ReceivedTimeMax);
            }
            if (!string.IsNullOrEmpty(filter.OpenedTimeMin)) {
                condition.Add("OpenedTime >= @p" + ++paramCount);
                param.Add(filter.OpenedTimeMin);
            }
            if (!string.IsNullOrEmpty(filter.OpenedTimeMax)) {
                condition.Add("OpenedTime <= @p" + ++paramCount);
                param.Add(filter.OpenedTimeMax);
            }
            if (filter.Location.Length > 0) {
                condition.Add("Location IN (@p" + ++paramCount + ")");
                param.Add(filter.Location.Select(e => e.ToString()).Join(","));
            }
            if (!string.IsNullOrEmpty(filter.Owner)) {
                var user = Verifier.FindUser(filter.Owner);
                if (user != null) {
                    condition.Add("Owner = @p" + ++paramCount);
                    param.Add(user.Id);
                }
            }
            if (filter.Experiment.Length > 0) {
                condition.Add("Experiment IN (@p" + ++paramCount + ")");
                param.Add(filter.Experiment.Select(e => e.ToString()).Join(","));
            }
            if (filter.Vendor.Length > 0) {
                condition.Add("Vendor IN (@p" + ++paramCount + ")");
                param.Add(filter.Vendor.Select(e => e.ToString()).Join(","));
            }
            if (filter.UsedMin.HasValue) {
                condition.Add("Used >= @p" + ++paramCount);
                param.Add(filter.UsedMin.Value);
            }
            if (filter.UsedMax.HasValue) {
                condition.Add("Used <= @p" + ++paramCount);
                param.Add(filter.UsedMax.Value);
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
            } else
                query = "SELECT * FROM ItemDetail WHERE " + query;
            if (filter.Take.HasValue)  {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}
