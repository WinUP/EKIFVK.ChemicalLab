using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Filters;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Authentication;
using EKIFVK.ChemicalLab.Services.Tracking;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// API for User Management
    /// <list type="bullet">
    /// <item><description>GET /{name} => GetInfo</description></item>
    /// <item><description>POST /{name} => Add</description></item>
    /// <item><description>PUT /{name}/token => SignIn</description></item>
    /// <item><description>DELETE /{name}/token => SignOut</description></item>
    /// <item><description>DELETE /{name} => Disable</description></item>
    /// <item><description>PATCH /{name} => ChangeUserInformation</description></item>
    /// <item><description>GET /.count => GetUserCount</description></item>
    /// <item><description>GET /.list => GetUserList</description></item>
    /// </list>
    /// </summary>
    [Route("api/v1/user")]
    public class UserController : BasicVerifiableController {
        private readonly IOptions<UserModuleConfiguration> _setting;

        public UserController(ChemicalLabContext database, IAuthentication verifier, ITrackService tracker,
            IOptions<UserModuleConfiguration> setting)
            : base(database, verifier, tracker) {
            _setting = setting;
        }

        /// <summary>
        /// Get user information<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>UserManagePermission</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>{n, d, g, t, a, m, r:bool, u}</description></item>
        /// <item><description>n: name</description></item>
        /// <item><description>d: display name</description></item>
        /// <item><description>g: user's group</description></item>
        /// <item><description>t: last access time</description></item>
        /// <item><description>a: last access address</description></item>
        /// <item><description>m: allow multiple login</description></item>
        /// <item><description>r: disabled</description></item>
        /// <item><description>u: last update</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>No target user: 404 NoTargetUser</description></item>
        /// <item><description>Permission denied: 403 [VerifyResult]</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">Target user's name</param>
        [HttpGet("{name}")]
        public JsonResult GetInfo(string name) {
            if (!Verify(Session, _setting.Value.UserManagePermission, out var verifyResult))
                return Denied(verifyResult);
            var target = FindUser(name);
            if (target == null)
                return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetUser);
            var response = BasicResponse(data: new Hashtable {
                {"n", target.Name},
                {"d", target.DisplayName},
                {"g", FindGroup(target).Name},
                {"t", target.LastAccessTime},
                {"a", target.LastAccessAddress},
                {"m", target.AllowMultiAddressLogin},
                {"r", target.Disabled},
                {"u", target.LastUpdate}
            });
            Tracker.Write(new TrackRecord(TrackType.InfoL3, Session, _setting.Value.UserTable, target.Id, "ALL")
                .AddNote(_setting.Value.GetUserInfo));
            return response;
        }

        /// <summary>
        /// Register<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>UserAddingPermission</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>id:int</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>Permission denied: 403 [VerifyResult]</description></item>
        /// <item><description>Invalid username format: 400 InvalidUsernameFormat</description></item>
        /// <item><description>No target group: 404 NoTargetGroup</description></item>
        /// <item><description>User already exist: 409 UserAlreadyExist</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">User's name (cannot have /\?, first letter cannot be .)</param>
        /// <param name="parameter">
        /// Parameters<br />
        /// <list type="bullet">
        /// <item><description>group: User's group's name</description></item>
        /// </list>
        /// </param>
        [HttpPost("{name}")]
        public JsonResult Add(string name, [FromBody] Hashtable parameter) {
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf("?", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return BasicResponse(StatusCodes.Status400BadRequest, _setting.Value.InvalidUsernameFormat);
            if (!Verify(Session, _setting.Value.UserAddingPermission, out var verifyResult))
                return Denied(verifyResult);
            var newUser = FindUser(name);
            if (newUser != null) {
                Tracker.Write(new TrackRecord(TrackType.ErrorL1, Session, _setting.Value.UserTable, newUser.Id, "")
                    .AddNote(_setting.Value.UserAlreadyExist));
                return BasicResponse(StatusCodes.Status409Conflict, _setting.Value.UserAlreadyExist);
            }
            var groupId = int.Parse(parameter["group"].ToString());
            var group = Database.UserGroups.FirstOrDefault(e => e.Id == groupId);
            if (group == null)
                return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetGroup);
            newUser = new User {
                Name = name.ToLower(),
                Password = _setting.Value.DefaulPasswordHash,
                UserGroupNavigation = group,
                LastUpdate = DateTime.Now
            };
            Database.Users.Add(newUser);
            Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, newUser.Id, "")
                .AddNote(_setting.Value.AddUser));
            return BasicResponse(data: newUser.Id);
        }

        /// <summary>
        /// User sign in<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>NULL</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>token</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>No target user: 404 NoTargetUser</description></item>
        /// <item><description>User is disabled: 403 DisabledUser</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">User's name</param>
        /// <param name="password">Uppercase SHA256 of password</param>
        [HttpPut("{name}/token")]
        public JsonResult SignIn(string name, string password) {
            //! What is allow multiple login:
            //! By checking this, user can use its account with different browser or different computer at the same time
            var user = FindUser(name);
            if (user == null)
                return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetUser);
            if (user.Password != password)
                return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.WrongPassword);
            if (user.Disabled || FindGroup(user).Disabled)
                return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.DisabledUser);
            if (user.AllowMultiAddressLogin && Verify(user, "") == VerifyResult.Passed) {
                Tracker.Write(new TrackRecord(TrackType.InfoL1, user, _setting.Value.UserTable, user.Id, _setting.Value.UserTableAccessToken)
                    .AddNote(_setting.Value.SingIn)
                    .AddPreviousData(user.AccessToken)
                    .AddNewData(user.AccessToken));
                return BasicResponse(data: user.AccessToken);
            }
            var previousToken = user.AccessToken;
            user.AccessToken = Guid.NewGuid().ToString().ToUpper();
            Verifier.UpdateAccessTime(user);
            Verifier.UpdateAccessAddress(user, HttpContext.Connection.RemoteIpAddress);
            Tracker.Write(new TrackRecord(TrackType.InfoL1, user, _setting.Value.UserTable, user.Id, _setting.Value.UserTableAccessToken)
                .AddNote(_setting.Value.SingIn)
                .AddPreviousData(previousToken)
                .AddNewData(user.AccessToken));
            return BasicResponse(data: user.AccessToken);
        }

        /// <summary>
        /// User sign out<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>NULL</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>NULL</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>Cannot find current user: 403 [VerifyResult.NonexistentToken]</description></item>
        /// <item><description>Cannot sign out other user: 403 CannotSignOutOthers</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">User's name</param>
        /// <returns></returns>
        [HttpDelete("{name}/token")]
        public JsonResult SignOut(string name) {
            if (Session == null) return NonexistentToken();
            if (!IsUserNameEqual(Session.Name, name)) {
                Tracker.Write(new TrackRecord(TrackType.ErrorL1, Session, _setting.Value.UserTable, Session.Id, "")
                    .AddNote(_setting.Value.CannotSingOutOthers));
                return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.CannotSingOutOthers);
            }
            var previousToken = Session.AccessToken;
            Session.AccessToken = null;
            Verifier.UpdateAccessTime(Session);
            Verifier.UpdateAccessAddress(Session, HttpContext.Connection.RemoteIpAddress);
            Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, Session.Id, _setting.Value.UserTableAccessToken)
                .AddNote(_setting.Value.SingOut)
                .AddPreviousData(previousToken)
                .AddNewData(""));
            return BasicResponse();
        }

        /// <summary>
        /// Detele user<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>UserModifyDisabledPermission</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>NULL</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>Permission denied: 403 [VerifyResult]</description></item>
        /// <item><description>Cannot remove self: 403 CannotRemoveSelf</description></item>
        /// <item><description>No target user: 404 NoTargetUser</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">User's name</param>
        [HttpDelete("{name}")]
        public JsonResult Disable(string name) {
            if (!Verify(Session, _setting.Value.UserModifyDisabledPermission, out var verifyResult))
                return Denied(verifyResult);
            if (IsUserNameEqual(Session.Name, name)) {
                Tracker.Write(new TrackRecord(TrackType.ErrorL1, Session, _setting.Value.UserTable, Session.Id, "")
                    .AddNote(_setting.Value.CannotDisableSelf));
                return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.CannotDisableSelf);
            }
            var target = FindUser(name);
            if (target == null)
                return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetUser);
            target.Disabled = true;
            target.LastUpdate = DateTime.Now;
            Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTableDisabled)
                .AddNote(_setting.Value.DisableUser)
                .AddPreviousData(true)
                .AddNewData(false));
            return BasicResponse();
        }

        /// <summary>
        /// Modify user's information<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>UserResetPasswordPermission (only for change password)</description></item>
        /// <item><description>UserChangeGroupPermission (only for change usergroup)</description></item>
        /// <item><description>UserModifyPermission (only for change other's multiple address sign in)</description></item>
        /// <item><description>UserDisablePermission (only for change disabled)</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>{p?:bool, g?:bool, m?:bool, r?:bool}</description></item>
        /// <item><description>p: is password change success</description></item>
        /// <item><description>g: is user's group change success</description></item>
        /// <item><description>m: is allow multiple login change success</description></item>
        /// <item><description>r: is disabled change success</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>Permission denied: 403 [VerifyResult]</description></item>
        /// <item><description>Cannot change self's usergroup: 403 CannotChangeSelfGroup</description></item>
        /// <item><description>Cannot disable or enable self: 403 CannotDisableSelf</description></item>
        /// <item><description>No target user: 404 NoTargetUser</description></item>
        /// <item><description>No target group: 404 NoTargetGroup</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">Target user's name</param>
        /// <param name="parameter">
        /// Parameters<br />
        /// <list type="bullet">
        /// <item><description>password?: new password (or let it empty to reset password)</description></item>
        /// <item><description>group?: new usergroup</description></item>
        /// <item><description>allowMulti?: new value of allow multiple address</description></item>
        /// <item><description>disabled?: new value of disabled</description></item>
        /// </list>
        /// </param>
        [HttpPatch("{name}")]
        public JsonResult ChangeUserInformation(string name, [FromBody] Hashtable parameter) {
            if (Session == null)
                return NonexistentToken();
            var target = FindUser(name);
            if (target == null)
                return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetUser);
            var finalData = new JObject();
            if (parameter.ContainsKey("password")) {
                var previous = target.Password;
                string note;
                if (Session != target) {
                    if (!Verify(Session, _setting.Value.UserResetPasswordPermission, out var verifyResult))
                        return Denied(verifyResult, finalData);
                    note = _setting.Value.ResetPassword;
                    target.Password = _setting.Value.DefaulPasswordHash;
                }
                else {
                    note = _setting.Value.ChangePassword;
                    target.Password = parameter["password"].ToString();
                }
                target.LastUpdate = DateTime.Now;
                Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTablePassword)
                    .AddNote(note)
                    .AddPreviousData(previous)
                    .AddNewData(target.Password));
                finalData.Add("p", true);
            }
            if (parameter.ContainsKey("group")) {
                if (Session == target) {
                    Tracker.Write(new TrackRecord(TrackType.ErrorL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTableGroup)
                        .AddNote(_setting.Value.CannotChangeSelfGroup));
                    return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.CannotChangeSelfGroup, finalData);
                }
                if (!Verify(Session, _setting.Value.UserChangeGroupPermission, out var verifyResult))
                    return Denied(verifyResult, finalData);
                var group = FindGroup(parameter["group"].ToString());
                if (group == null)
                    return BasicResponse(StatusCodes.Status404NotFound, _setting.Value.NoTargetGroup, finalData);
                var previous = FindGroup(target).Name;
                target.UserGroupNavigation = group;
                target.LastUpdate = DateTime.Now;
                Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTableGroup)
                    .AddNote(_setting.Value.ChangeUserGroup)
                    .AddPreviousData(previous)
                    .AddNewData(group.Name));
                finalData.Add("g", true);
            }
            if (parameter.ContainsKey("allowMulti")) {
                if (Session != target && !Verify(Session, _setting.Value.UserModifyPermission, out var verifyResult))
                    return Denied(verifyResult, finalData);
                var previous = target.AllowMultiAddressLogin;
                target.AllowMultiAddressLogin = (bool) parameter["allowMulti"];
                target.LastUpdate = DateTime.Now;
                Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTableAllowMultipleLogin)
                    .AddNote(_setting.Value.ChangeUserAllowMultipleLogin)
                    .AddPreviousData(previous)
                    .AddNewData(target.AllowMultiAddressLogin));
                finalData.Add("m", true);
            }
            if (!parameter.ContainsKey("disabled")) return BasicResponse(data: finalData);
            {
                if (Session == target)
                    return BasicResponse(StatusCodes.Status403Forbidden, _setting.Value.CannotDisableSelf, finalData);
                if (!Verify(Session, _setting.Value.UserDisablePermission, out var verifyResult))
                    return Denied(verifyResult, finalData);
                var previous = target.Disabled;
                target.Disabled = (bool) parameter["disabled"];
                target.LastUpdate = DateTime.Now;
                Tracker.Write(new TrackRecord(TrackType.InfoL1, Session, _setting.Value.UserTable, target.Id, _setting.Value.UserTableDisabled)
                    .AddNote(_setting.Value.ChangeUserDisabled)
                    .AddPreviousData(previous)
                    .AddNewData(target.AllowMultiAddressLogin));
                finalData.Add("r", true);
            }
            return BasicResponse(data: finalData);
        }

        /// <summary>
        /// Get users' total count<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>NULL</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>count:int</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>NULL (all illegal parameters will be ignored)</description></item>
        /// </list>
        /// </summary>
        /// <param name="filter">Search filter</param>
        [HttpGet(".count")]
        public JsonResult GetUserCount(UserSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return BasicResponse(data: Database.Users.FromSql(query, param.ToArray()).Count());
        }

        /// <summary>
        /// Get list of users<br />
        /// <br />
        /// Permission Group
        /// <list type="bullet">
        /// <item><description>UserManagePermission</description></item>
        /// </list>
        /// Returned Value
        /// <list type="bullet">
        /// <item><description>[{n, d, g, t, a, m, r:bool, u}]</description></item>
        /// <item><description>n: name</description></item>
        /// <item><description>d: display name</description></item>
        /// <item><description>g: user's group</description></item>
        /// <item><description>t: last access time</description></item>
        /// <item><description>a: last access address</description></item>
        /// <item><description>m: allow multiple login</description></item>
        /// <item><description>r: disabled</description></item>
        /// <item><description>u: last update</description></item>
        /// </list>
        /// Probable Errors
        /// <list type="bullet">
        /// <item><description>NULL (all illegal parameters will be ignored)</description></item>
        /// </list>
        /// </summary>
        /// <param name="filter">Search filter</param>
        /// <returns></returns>
        [HttpGet(".list")]
        public JsonResult GetUserList(UserSearchFilter filter) {
            var user = FindUser();
            if (!Verify(user, _setting.Value.UserManagePermission, out var verifyResult))
                return Denied(verifyResult);
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return BasicResponse(data: Database.Users.FromSql(query, param.ToArray()).Select(e => new Hashtable {
                {"n", e.Name},
                {"d", e.DisplayName},
                {"g", FindGroup(e).Name},
                {"t", e.LastAccessTime},
                {"a", e.LastAccessAddress},
                {"m", e.AllowMultiAddressLogin},
                {"r", e.Disabled},
                {"u", e.LastUpdate}
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