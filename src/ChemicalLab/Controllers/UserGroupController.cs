using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EKIFVK.ChemicalLab.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.SearchFilters;
using EKIFVK.ChemicalLab.Services.Verification;
using EKIFVK.ChemicalLab.Services.Tracking;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/usergroup")]
    public class UserGroupController : VerifiableController {
        private readonly IOptions<UserModule> Configuration;

        public UserGroupController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<UserModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpGet("{name}")]
        [Verify("")]
        public JsonResult GetInfo(string name) {
            var group = Verifier.FindGroup(name);
            if (group == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidGroupName);
            return Json(data: new Hashtable {
                {"name", group.Name},
                {"note", group.Note},
                {"permission", group.Permission},
                {"disabled", group.Disabled},
                {"user", Database.Users.Count(e => e.UserGroup == group.Id)}
            });
        }

        [HttpPost("{name}")]
        [Verify("UG:ADD")]
        public JsonResult Add(string name, [FromBody] Hashtable param) {
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidGroupName);
            var group = Verifier.FindGroup(name);
            if (group != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            group = new UserGroup {
                Name = name,
                Note = param["note"].ToString(),
                Permission = param["permission"].ToString(),
                LastUpdate = DateTime.Now,
                Disabled = false
            };
            Database.UserGroups.Add(group);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewUserGroup).By(CurrentUser).At(group.Id).From("").To("").Save();
            return Json(data: group.Id);
        }

        [HttpDelete("{name}")]
        [Verify("UG:DELETE")]
        public JsonResult Delete(string name) {
            if (Verifier.FindGroup(CurrentUser).Name == name)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.CannotRemoveSelf);
            var target = Verifier.FindGroup(name);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidGroupName);
            var targetId = target.Id;
            if (Database.Users.Count(e => e.UserGroup == targetId) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteUserGroup).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.UserGroups.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{name}")]
        [Verify("UG:MANAGE")]
        public JsonResult ChangeInformation(string name, [FromBody] Hashtable param) {
            var target = Verifier.FindGroup(name);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidGroupName);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                var newName = param["name"].ToString();
                if (Database.UserGroups.Count(e => e.Name == newName) > 0)
                    data["name"] = Configuration.Value.AlreadyExisted;
                else {
                    Tracker.Get(Operation.ChangeUserGroupName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                        target.Name = newName;
                    }).To(target.Name).Save(false);
                }
            }
            if (param.ContainsKey("note")) {
                data["note"] = true;
                Tracker.Get(Operation.ChangeUserGroupNote).By(CurrentUser).At(target.Id).From(target.Note).Do(() => {
                    target.Note = param["note"].ToString();
                }).To(target.Note).Save(false);
            }
            if (param.ContainsKey("permission")) {
                data["permission"] = true;
                if (!Verifier.Check(CurrentUser, "UG:PERM", CurrentAddress))
                    data["permission"] = Configuration.Value.OperationDenied;
                else {
                    Tracker.Get(Operation.ChangeUserGroupPermission).By(CurrentUser).At(target.Id).From(target.Permission).Do(() => {
                        target.Permission = param["permission"].ToString();
                    }).To(target.Permission).Save(false);
                }
            }
            if (param.ContainsKey("disabled")) {
                data["disabled"] = true;
                if (Verifier.FindGroup(CurrentUser) == target)
                    data["disabled"] = Configuration.Value.CannotChangeSelf;
                else if (!Verifier.Check(CurrentUser, "UG:DISABLE", CurrentAddress))
                    data["disabled"] = Configuration.Value.OperationDenied;
                else {
                    Tracker.Get(Operation.ChangeUserGroupDisabled).By(CurrentUser).At(target.Id).From(target.Disabled.ToString()).Do(() => {
                        target.Disabled = (bool)param["disabled"];
                    }).To(target.Disabled.ToString()).Save(false);
                }
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet(".count")]
        [Verify("")]
        public JsonResult GetGroupCount(GroupSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.UserGroups.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [Verify("UG:MANAGE")]
        public JsonResult GetGroupList(GroupSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.UserGroups.FromSql(query, param.ToArray())
                .Select(e => new {
                    e.Name,
                    e.Note,
                    e.Permission,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"),
                    User = Database.Users.Count(u => u.UserGroup == e.Id)
                }).ToArray());
        }

        private static string QueryGenerator(GroupSearchFilter filter, ICollection<object> param) {
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.Name)) {
                condition.Add("Name LIKE CONCAT('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Name);
            }
            if (filter.Disabled.HasValue) {
                condition.Add("Disabled = @p" + ++paramCount);
                param.Add(filter.Disabled.Value ? 1 : 0);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0) {
                query = "SELECT * FROM UserGroup WHERE ID >= (SELECT ID FROM UserGroup WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM UserGroup WHERE " + query;
            if (filter.Take.HasValue) {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}