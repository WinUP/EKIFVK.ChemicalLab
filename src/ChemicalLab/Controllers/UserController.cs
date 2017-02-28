using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EKIFVK.ChemicalLab.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Filters;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Verification;
using EKIFVK.ChemicalLab.Services.Tracking;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// API for User Management
    /// </summary>
    [Route("api/1.1/user")]
    public class UserController : VerifiableController {
        private readonly IOptions<UserModuleConfiguration> _conf;

        public UserController(ChemicalLabContext database, IVerificationService verifier, ITrackService tracker,
            IOptions<UserModuleConfiguration> configuration)
            : base(database, verifier, tracker) {
            _conf = configuration;
        }

        private TrackRecord T(TrackType trackType, int record, string column) {
            return new TrackRecord(trackType, CurrentUser, _conf.Value.UserTable, record, column);
        }

        [HttpGet("{name}")]
        [PermissionCheck("USER:GET")]
        public JsonResult GetInfo(string name) {
            var target = Verifier.FindUser(name);
            if (target == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyUser);
            var response = FormattedResponse(data: new Hashtable {
                {"name", target.Name},
                {"display", target.DisplayName},
                {"usergroup", Verifier.FindGroup(target).Name},
                {"time", target.LastAccessTime},
                {"address", target.LastAccessAddress},
                {"disabled", target.Disabled},
                {"update", target.LastUpdate}
            });
            Tracker.Write(T(TrackType.I3I, target.Id, "ALL").Note(_conf.Value.GetUserInfo));
            return response;
        }

        [HttpPost("{name}")]
        [PermissionCheck("USER:ADD")]
        public JsonResult Add(string name, [FromBody] Hashtable parameter) {
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf("?", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return FormattedResponse(StatusCodes.Status400BadRequest, _conf.Value.InvalidFormat);
            name = name.ToLower();
            var newUser = Verifier.FindUser(name);
            if (newUser != null) {
                Tracker.Write(T(TrackType.E1D, newUser.Id, "").Note(_conf.Value.UserAlreadyExist));
                return FormattedResponse(StatusCodes.Status409Conflict, _conf.Value.UserAlreadyExist);
            }
            var groupId = int.Parse(parameter["group"].ToString());
            var group = Database.UserGroups.FirstOrDefault(e => e.Id == groupId);
            if (group == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyGroup);
            newUser = new User {
                Name = name.ToLower(),
                Password = _conf.Value.DefaulPasswordHash,
                DisplayName = parameter["display"].ToString(),
                UserGroupNavigation = group,
                LastUpdate = DateTime.Now
            };
            Database.Users.Add(newUser);
            Tracker.Write(T(TrackType.I1D, newUser.Id, "").Note(_conf.Value.AddUser));
            return FormattedResponse(data: newUser.Id);
        }

        [HttpPut("{name}/token")]
        public JsonResult SignIn(string name, string password) {
            var user = Verifier.FindUser(name);
            if (user == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyUser);
            if (user.Password != password)
                return FormattedResponse(StatusCodes.Status403Forbidden, _conf.Value.WrongPassword);
            if (user.Disabled || Verifier.FindGroup(user).Disabled)
                return FormattedResponse(StatusCodes.Status403Forbidden, _conf.Value.DisabledUser);
            var previousToken = user.AccessToken;
            user.AccessToken = Guid.NewGuid().ToString().ToUpper();
            Verifier.UpdateAccessTime(user);
            Verifier.UpdateAccessAddress(user, HttpContext.Connection.RemoteIpAddress);
            Tracker.Write(T(TrackType.I1D, user.Id, _conf.Value.UserTableAccessToken)
                .Note(_conf.Value.SingIn)
                .PreviousData(previousToken)
                .NewData(user.AccessToken));
            return FormattedResponse(data: user.AccessToken);
        }

        [HttpDelete("{name}/token")]
        public JsonResult SignOut(string name) {
            if (CurrentUser == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyUser);
            if (CurrentUser.Name != name) {
                Tracker.Write(T(TrackType.E1D, CurrentUser.Id, "").Note(_conf.Value.SignOutOthers));
                return FormattedResponse(StatusCodes.Status403Forbidden, _conf.Value.SignOutOthers);
            }
            var previousToken = CurrentUser.AccessToken;
            CurrentUser.AccessToken = null;
            Verifier.UpdateAccessTime(CurrentUser);
            Verifier.UpdateAccessAddress(CurrentUser, HttpContext.Connection.RemoteIpAddress);
            Tracker.Write(T(TrackType.I1D, CurrentUser.Id, _conf.Value.UserTableAccessToken)
                .Note(_conf.Value.SingOut)
                .PreviousData(previousToken)
                .NewData(""));
            return FormattedResponse();
        }

        [HttpPatch("{name}")]
        [PermissionCheck("")]
        public JsonResult ChangeUserInformation(string name, [FromBody] Hashtable parameter) {
            var target = Verifier.FindUser(name);
            if (target == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyUser);
            var finalData = new JObject();
            if (parameter.ContainsKey("password")) {
                var previous = target.Password;
                string note = null;
                if (CurrentUser != target) {
                    if (Verifier.Check(CurrentUser, "USER:MODIFY", out var verifyResult)) {
                        note = _conf.Value.ResetPassword;
                        target.Password = _conf.Value.DefaulPasswordHash;
                    }
                    else {
                        finalData.Add("password", false);
                        WritePermissionRejected(verifyResult, "USER:MODIFT");
                    }
                }
                else {
                    note = _conf.Value.ChangePassword;
                    target.Password = parameter["password"].ToString();
                }
                target.LastUpdate = DateTime.Now;
                if (note != null) {
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserTablePassword)
                        .Note(note)
                        .PreviousData(previous)
                        .NewData(target.Password));
                    finalData.Add("password", true);
                }
            }
            if (parameter.ContainsKey("group")) {
                if (CurrentUser == target) {
                    Tracker.Write(T(TrackType.E1D, target.Id, _conf.Value.UserTableGroup)
                        .Note(_conf.Value.ChangeSelfGroup));
                    return FormattedResponse(StatusCodes.Status403Forbidden, _conf.Value.ChangeSelfGroup,
                        finalData);
                }
                if (!Verifier.Check(CurrentUser, "USER:GROUP", out var verifyResult)) {
                    finalData.Add("usergroup", false);
                    WritePermissionRejected(verifyResult, "USER:GROUP");
                }
                else {
                    var groupName = parameter["group"].ToString();
                    var group = Database.UserGroups.FirstOrDefault(e => e.Name == groupName);
                    if (group == null)
                        return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyGroup, finalData);
                    var previous = Verifier.FindGroup(target).Name;
                    target.UserGroupNavigation = group;
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserTableGroup)
                        .Note(_conf.Value.ChangeUserGroup)
                        .PreviousData(previous)
                        .NewData(group.Name));
                    finalData.Add("usergroup", true);
                }
            }
            if (parameter.ContainsKey("display")) {
                var previous = target.DisplayName;
                if (CurrentUser != target && !Verifier.Check(CurrentUser, "USER:MODIFY", out var verifyResult)) {
                    finalData.Add("display", false);
                    WritePermissionRejected(verifyResult, "USER:MODIFT");
                }
                else {
                    target.DisplayName = parameter["display"].ToString();
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserTableDisplayName)
                        .Note(_conf.Value.ChangeUserDisplayName)
                        .PreviousData(previous)
                        .NewData(target.DisplayName));
                    finalData.Add("display", true);
                }
            }
            if (parameter.ContainsKey("disable")) {
                if (CurrentUser.Name == name) {
                    Tracker.Write(T(TrackType.E1D, CurrentUser.Id, "").Note(_conf.Value.DisableSelf));
                    finalData.Add("disable", false);
                }
                else if (!Verifier.Check(CurrentUser, "USER:STATUS", out var verifyResult)) {
                    finalData.Add("disable", false);
                    WritePermissionRejected(verifyResult, "USER:STATUS");
                }
                else {
                    var previous = target.Disabled;
                    target.Disabled = (bool) parameter["disable"];
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserTableDisabled)
                        .Note(_conf.Value.DisableUser)
                        .PreviousData(previous)
                        .NewData(target.Disabled));
                    finalData.Add("disable", true);
                }
            }
            return FormattedResponse(data: finalData);
        }

        [HttpGet(".count")]
        public JsonResult GetUserCount(UserSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return FormattedResponse(data: Database.Users.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [PermissionCheck("USER:MANAGE")]
        public JsonResult GetUserList(UserSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return FormattedResponse(data: Database.Users.FromSql(query, param.ToArray()).Select(e => new Hashtable {
                {"name", e.Name},
                {"display", e.DisplayName},
                {"usergroup", Verifier.FindGroup(e).Name},
                {"time", e.LastAccessTime},
                {"address", e.LastAccessAddress},
                {"disabled", e.Disabled},
                {"update", e.LastUpdate}
            }).ToArray());
        }

        private string QueryGenerator(UserSearchFilter filter, ICollection<object> param) {
            //? MySql connector for .net core still does not support Take() and Skip() in this version
            //? which means we can only form SQL query manually
            //? Also, LIMIT in mysql has significant performnce issue so we will not use LIMIT
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