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
    [Route("api/1.1/user")]
    public class UserController : VerifiableController {
        private readonly IOptions<UserModule> Configuration;

        public UserController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<UserModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpGet("{name}")]
        [Verify("")]
        public JsonResult GetInfo(string name) {
            var targetUser = Verifier.FindUser(name);
            if (targetUser == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUserName);
            var data = new Hashtable {
                {"name", targetUser.Name},
                {"displayName", targetUser.DisplayName},
                {"userGroup", Verifier.FindGroup(targetUser).Name}
            };
            if (CurrentUser != null && CurrentUser.Name == name || Verifier.Check(CurrentUser, "US:MANAGE", CurrentAddress)) {
                data["activeTime"] = targetUser.LastAccessTime?.ToString("yyyy-MM-dd HH:mm:ss");
                data["accessAddress"] = targetUser.LastAccessAddress;
                data["disabled"] = targetUser.Disabled;
                data["update"] = targetUser.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return Json(data: data);
        }

        [HttpPost("{name}")]
        [Verify("US:ADD")]
        public JsonResult Add(string name, [FromBody] Hashtable parameter) {
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidUserName);
            var user = Verifier.FindUser(name);
            if (user != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            var group = Verifier.FindGroup(parameter["userGroup"].ToString());
            if (group == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidGroupName);
            user = new User {
                Name = name,
                Password = Configuration.Value.DefaulPasswordHash,
                DisplayName = parameter["displayName"].ToString(),
                UserGroupNavigation = group,
                Disabled = false,
                LastUpdate = DateTime.Now
            };
            Database.Users.Add(user);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewUser).By(CurrentUser).At(user.Id).From("").To("").Save();
            return Json(data: user.Id);
        }

        [HttpDelete("{name}")]
        [Verify("US:DELETE")]
        public JsonResult Delete(string name) {
            if (CurrentUser.Name == name)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.CannotRemoveSelf);
            var target = Verifier.FindUser(name);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUserName);
            try {
                var id = target.Id;
                Tracker.Get(Operation.DeleteUser).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.TrackHistories.RemoveRange(Database.TrackHistories.Where(e => e.Modifier == id));
                    Database.Items.RemoveRange(Database.Items.Where(e => e.Owner == id));
                    Database.Users.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("{name}")]
        public JsonResult ChangeInformation(string name, [FromBody] Hashtable param) {
            var target = Verifier.FindUser(name);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidUserName);
            var data = new Hashtable();
            if (param.ContainsKey("displayName")) {
                if (!Verifier.Check(CurrentUser, "", out var result, CurrentAddress)) {
                    data["displayName"] = Verifier.ToString(result);
                } else {
                    data["displayName"] = true;
                    if (CurrentUser != target && !Verifier.Check(CurrentUser, "US:MODIFY", CurrentAddress))
                        data["displayName"] = Configuration.Value.OperationDenied;
                    else {
                        Tracker.Get(Operation.ChangeUserDisplayName).By(CurrentUser).At(target.Id).From(target.DisplayName).Do(() => {
                            target.DisplayName = param["displayName"].ToString();
                        }).To(target.DisplayName).Save(false);
                    }
                }
            }
            if (param.ContainsKey("password")) {
                if (!Verifier.Check(CurrentUser, "", out var result, CurrentAddress)) {
                    data["password"] = Verifier.ToString(result);
                } else {
                    data["password"] = true;
                    if (CurrentUser != target) {
                        if (Verifier.Check(CurrentUser, "US:MODIFY", CurrentAddress)) {
                            Tracker.Get(Operation.ResetUserPassword).By(CurrentUser).At(target.Id).From(target.Password).Do(() => {
                                target.Password = Configuration.Value.DefaulPasswordHash;
                            }).To(target.Password).Save(false);
                        }
                        else
                            data["password"] = Configuration.Value.OperationDenied;
                    } else {
                        Tracker.Get(Operation.ChangeUserPassowrd).By(CurrentUser).At(target.Id).From(target.Password).Do(() => {
                            target.Password = param["password"].ToString();
                        }).To(target.Password).Save(false);
                    }
                }
            }
            if (param.ContainsKey("userGroup")) {
                data["userGroup"] = true;
                if (CurrentUser == target)
                    data["userGroup"] = Configuration.Value.CannotChangeSelf;
                else if (!Verifier.Check(CurrentUser, "US:GROUP", CurrentAddress)) {
                    data["userGroup"] = Configuration.Value.OperationDenied;
                } else {
                    var group = Verifier.FindGroup(param["userGroup"].ToString());
                    if (group == null)
                        data["userGroup"] = Configuration.Value.InvalidGroupName;
                    else {
                        Tracker.Get(Operation.ChangeUserGroup).By(CurrentUser).At(target.Id).From(target.UserGroup.ToString()).Do(() => {
                            target.UserGroupNavigation = group;
                        }).To(group.Id.ToString()).Save(false);
                    }
                }
            }
            if (param.ContainsKey("accessToken")) {
                if (param["accessToken"] is bool) {
                    if (CurrentUser.Name != name)
                        data["accessToken"] = Configuration.Value.OperationDenied;
                    else {
                        CurrentUser.AccessToken = null;
                        Verifier.UpdateAccessTime(CurrentUser, false);
                        Verifier.UpdateAccessAddress(CurrentUser, CurrentAddress);
                        data["accessToken"] = true;
                    }
                } else {
                    if (target.Password != param["accessToken"].ToString())
                        data["accessToken"] = Configuration.Value.IncorrectPassword;
                    else {
                        target.AccessToken = Guid.NewGuid().ToString().ToUpper();
                        Verifier.UpdateAccessTime(target, false);
                        Verifier.UpdateAccessAddress(target, CurrentAddress);
                        data["accessToken"] = target.AccessToken;
                    }
                }
            }
            if (param.ContainsKey("disabled")) {
                data["disabled"] = true;
                if (CurrentUser == target)
                    data["disabled"] = Configuration.Value.CannotChangeSelf;
                else if (!Verifier.Check(CurrentUser, "US:DISABLE", CurrentAddress))
                    data["disabled"] = Configuration.Value.OperationDenied;
                else  {
                    Tracker.Get(Operation.ChangeUserDisabled).By(CurrentUser).At(target.Id).From(target.Disabled.ToString()).Do(() => {
                        target.Disabled = (bool)param["disabled"];
                    }).To(target.Disabled.ToString()).Save(false);
                }
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet(".count")]
        public JsonResult GetUserCount(UserSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.Users.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [Verify("US:MANAGE")]
        public JsonResult GetUserList(UserSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.Users.FromSql(query, param.ToArray())
                .Select(e => new {
                    e.Name,
                    e.DisplayName,
                    UserGroup = Verifier.FindGroup(e).Name,
                    e.LastAccessTime,
                    e.LastAccessAddress,
                    e.Disabled,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray());
        }

        private string QueryGenerator(UserSearchFilter filter, ICollection<object> param) {
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.Name)) {
                condition.Add("Name LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Name);
            }
            if (!string.IsNullOrEmpty(filter.Group)) {
                var group = Database.UserGroups.FirstOrDefault(e => e.Name == filter.Group);
                if (group != null) {
                    condition.Add("UserGroup = @p" + ++paramCount);
                    param.Add(group.Id);
                }
            }
            if (filter.Disabled.HasValue) {
                condition.Add("Disabled = @p" + ++paramCount);
                param.Add(filter.Disabled.Value ? 1 : 0);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0) {
                query = "SELECT * FROM User WHERE ID >= (SELECT ID FROM User WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM User WHERE " + query;
            if (filter.Take.HasValue) {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}