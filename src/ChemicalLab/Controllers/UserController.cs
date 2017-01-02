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
using EKIFVK.ChemicalLab.SearchFilter;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Authentication;
using EKIFVK.ChemicalLab.Services.Logging;

namespace EKIFVK.ChemicalLab.Controllers
{
    /// <summary>
    /// API for User Management
    /// <list type="bullet">
    /// <item><description>GET /{name} => GetInfo</description></item>
    /// <item><description>POST /{name} => Register</description></item>
    /// <item><description>PUT /{name}/token => SignIn</description></item>
    /// <item><description>DELETE /{name}/token => SignOut</description></item>
    /// <item><description>DELETE /{name} => Disable</description></item>
    /// <item><description>PATCH /{name} => ChangeUserInformation</description></item>
    /// <item><description>GET /.count => GetUserCount</description></item>
    /// <item><description>GET /.list => GetUserList</description></item>
    /// </list>
    /// </summary>
    [Route("api/v1/user")]
    public class UserController : BasicVerifiableController
    {
        private readonly IOptions<UserModuleConfiguration> _configuration;

        public UserController(ChemicalLabContext database, IAuthentication verifier, ILoggingService logger, IOptions<UserModuleConfiguration> configuration)
            : base(database, verifier, logger)
        {
            _configuration = configuration;
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
        public JsonResult GetInfo(string name)
        {
            var currentUser = FindUser();
            if (!Verify(currentUser, _configuration.Value.UserManagePermission, out var verifyResult)) return PermissionDenied(verifyResult);
            var targetUser = FindUser(name);
            if (targetUser == null) return BasicResponse(StatusCodes.Status404NotFound, _configuration.Value.NoTargetUser);
            var response = BasicResponse(data: new Hashtable
            {
                {"n", targetUser.Name},
                {"d", targetUser.DisplayName},
                {"g", FindGroup(targetUser).Name},
                {"t", targetUser.LastAccessTime},
                {"a", targetUser.LastAccessAddress},
                {"m", targetUser.AllowMultiAddressLogin},
                {"r", targetUser.Disabled},
                {"u", targetUser.LastUpdate}
            });
            Logger.Write(new LoggingRecord(LoggingType.InfoLevel3, currentUser)
                .AddContent(_configuration.Value.GetUserInfoLog)
                .AddData("name", name));
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
        /// <item><description>Invalid password format: 400 InvalidPasswordFormat</description></item>
        /// <item><description>User already exist: 409 UserAlreadyExist</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">User's name (cannot have /\?, first letter cannot be .)</param>
        /// <param name="parameter">
        /// Parameters<br />
        /// <list type="bullet">
        /// <item><description>password: Uppercase SHA256 of password</description></item>
        /// </list>
        /// </param>
        [HttpPost("{name}")]
        public JsonResult Register(string name, [FromBody] Hashtable parameter)
        {
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf("?", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return BasicResponse(StatusCodes.Status400BadRequest, _configuration.Value.InvalidUsernameFormat);
            var currentUser = FindUser();
            if (!Verify(currentUser, _configuration.Value.UserAddingPermission, out var verifyResult)) return PermissionDenied(verifyResult);
            var password = parameter["password"].ToString();
            if (password.ToUpper() != password || password.Length != 64)
                return BasicResponse(StatusCodes.Status400BadRequest, _configuration.Value.InvalidPasswordFormat);
            var targetUser = FindUser(name);
            if (targetUser != null)
            {
                Logger.Write(new LoggingRecord(LoggingType.ErrorLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.RegisterExistentUserLog)
                    .AddData("name", name));
                return BasicResponse(StatusCodes.Status409Conflict, _configuration.Value.UserAlreadyExist);
            }
            var normalUsergroupId = _configuration.Value.DefaultUserGroup;
            targetUser = new User
            {
                Name = name.ToLower(),
                Password = password,
                UserGroupNavigation = Database.UserGroups.FirstOrDefault(e => e.Id == normalUsergroupId),
                LastUpdate = DateTime.Now
            };
            Database.Users.Add(targetUser);
            Database.SaveChanges();
            Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.RegisterUserLog)
                    .AddData("name", name));
            return BasicResponse(data: targetUser.Id);
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
        public JsonResult SignIn(string name, string password)
        {
            var user = FindUser(name);
            if (user == null) return BasicResponse(StatusCodes.Status404NotFound, _configuration.Value.NoTargetUser);
            if (user.Password != password) return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.WrongPassword);
            if (user.Disabled) return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.DisabledUser);
            if (FindGroup(user).Disabled) return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.DisabledUser);
            if (user.AllowMultiAddressLogin && Verify(user, "") == VerifyResult.Passed)
            {
                Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, user, "User", user.Id)
                    .AddContent(_configuration.Value.SingInLog)
                    .AddData("name", name));
                return BasicResponse(data: user.AccessToken);
            }
            var token = Guid.NewGuid().ToString().ToUpper();
            user.AccessToken = token;
            Verifier.UpdateAccessTime(user);
            Verifier.UpdateAccessAddress(user, HttpContext.Connection.RemoteIpAddress);
            Database.SaveChanges();
            Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, user, "User", user.Id)
                    .AddContent(_configuration.Value.SingInLog)
                    .AddData("name", name));
            return BasicResponse(data: token);
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
        public JsonResult SignOut(string name)
        {
            var user = FindUser();
            if (user == null) return NonexistentToken();
            if (!IsUserNameEqual(user.Name, name))
            {
                Logger.Write(new LoggingRecord(LoggingType.ErrorLevel1, user, "User", user.Id)
                    .AddContent(_configuration.Value.TrySignOutOtherUserLog)
                    .AddData("name", name));
                return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.CannotSingOutOthers);
            }
            user.AccessToken = null;
            Verifier.UpdateAccessTime(user);
            Verifier.UpdateAccessAddress(user, HttpContext.Connection.RemoteIpAddress);
            Database.SaveChanges();
            Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, user, "User", user.Id)
                    .AddContent(_configuration.Value.SingOutLog)
                    .AddData("name", name));
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
        public JsonResult Disable(string name)
        {
            var currentUser = FindUser();
            if (!Verify(currentUser, _configuration.Value.UserModifyDisabledPermission, out var verifyResult)) return PermissionDenied(verifyResult);
            if (IsUserNameEqual(currentUser.Name, name))
            {
                Logger.Write(new LoggingRecord(LoggingType.ErrorLevel1, currentUser, "User", currentUser.Id)
                    .AddContent(_configuration.Value.TryDisableSelfLog)
                    .AddData("name", name));
                return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.CannotRemoveSelf);
            }
            var targetUser = FindUser(name);
            if (targetUser == null) return BasicResponse(StatusCodes.Status404NotFound, _configuration.Value.NoTargetUser);
            targetUser.Disabled = true;
            Database.SaveChanges();
            Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.DisableUserLog)
                    .AddData("name", name));
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
        public JsonResult ChangeUserInformation(string name, [FromBody] Hashtable parameter)
        {
            var currentUser = FindUser();
            if (currentUser == null) return NonexistentToken();
            var targetUser = FindUser(name);
            if (targetUser == null) return BasicResponse(StatusCodes.Status404NotFound, _configuration.Value.NoTargetUser);
            var finalData = new JObject();
            if (parameter.ContainsKey("password"))
            {
                if (currentUser != targetUser)
                {
                    if (!Verify(currentUser, _configuration.Value.UserResetPasswordPermission, out var verifyResult))
                        return PermissionDenied(verifyResult, finalData);
                    targetUser.Password = _configuration.Value.DefaulPasswordHash;
                    Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                        .AddContent(_configuration.Value.ResetPasswordLog)
                        .AddData("name", name));
                }
                else
                {
                    targetUser.Password = parameter["password"].ToString();
                    Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                        .AddContent(_configuration.Value.ChangePasswordLog)
                        .AddData("name", name));
                }
                finalData.Add("p", true);
            }
            if (parameter.ContainsKey("group"))
            {
                if (currentUser == targetUser)
                {
                    Logger.Write(new LoggingRecord(LoggingType.ErrorLevel1, currentUser, "User", targetUser.Id)
                        .AddContent(_configuration.Value.TryChangeSelfGroupLog)
                        .AddData("name", name));
                    return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.CannotChangeSelfGroup, finalData);
                }
                if (!Verify(currentUser, _configuration.Value.UserChangeGroupPermission, out var verifyResult)) return PermissionDenied(verifyResult, finalData);
                var group = FindGroup(parameter["group"].ToString());
                if (group == null) return BasicResponse(StatusCodes.Status404NotFound, _configuration.Value.NoTargetGroup, finalData);
                var previousGroupName = FindGroup(targetUser).Name;
                targetUser.UserGroupNavigation = group;
                finalData.Add("g", true);
                Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.ChangeUserGroupLog)
                    .AddData("name", name)
                    .Add("old", previousGroupName)
                    .Add("new", group.Name));
            }
            if (parameter.ContainsKey("allowMulti"))
            {
                if (currentUser != targetUser && !Verify(currentUser, _configuration.Value.UserModifyPermission, out var verifyResult))
                    return PermissionDenied(verifyResult, finalData);
                var previousValue = targetUser.AllowMultiAddressLogin;
                targetUser.AllowMultiAddressLogin = (bool) parameter["allowMulti"];
                finalData.Add("m", true);
                Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.ChangeUserAllowMultipleLoginLog)
                    .AddData("name", name)
                    .Add("old", previousValue)
                    .Add("new", targetUser.AllowMultiAddressLogin));
            }
            if (!parameter.ContainsKey("disabled")) return BasicResponse(data: finalData);
            {
                if (currentUser == targetUser)
                    return BasicResponse(StatusCodes.Status403Forbidden, _configuration.Value.CannotDisableSelf, finalData);
                if (!Verify(currentUser, _configuration.Value.UserDisablePermission, out var verifyResult))
                    return PermissionDenied(verifyResult, finalData);
                var previousValue = targetUser.Disabled;
                targetUser.Disabled = (bool)parameter["disabled"];
                finalData.Add("r", true);
                Logger.Write(new LoggingRecord(LoggingType.InfoLevel1, currentUser, "User", targetUser.Id)
                    .AddContent(_configuration.Value.ChangeUserDisabledLog)
                    .AddData("name", name)
                    .Add("old", previousValue)
                    .Add("new", targetUser.Disabled));
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
        public JsonResult GetUserCount(UserSearchFilter filter)
        {
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
        public JsonResult GetUserList(UserSearchFilter filter)
        {
            var user = FindUser();
            if (!Verify(user, _configuration.Value.UserManagePermission, out var verifyResult)) return PermissionDenied(verifyResult);
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return BasicResponse(data: Database.Users.FromSql(query, param.ToArray()).Select(e => new Hashtable
            {
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

        private string QueryGenerator(UserSearchFilter filter, ICollection<object> param)
        {
            //? MySql connector for .net core still does not support Take() and Skip() in this version
            //? which means we can only form SQL query manually
            //? Also, LIMIT in mysql has significant performnce issue so we will not use LIMIT
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.Name))
            {
                condition.Add("Name LIKE concat('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Name);
            }
            if (!string.IsNullOrEmpty(filter.Group))
            {
                var group = Database.UserGroups.FirstOrDefault(e => e.Name == filter.Group);
                if (group != null)
                {
                    condition.Add("UserGroup = @p" + ++paramCount);
                    param.Add(group.Id);
                }
            }
            if (filter.Disabled.HasValue)
            {
                condition.Add("Disabled = @p" + ++paramCount);
                param.Add(filter.Disabled.Value ? 1 : 0);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0)
            {
                query = "SELECT * FROM User WHERE ID >= (SELECT ID FROM User WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM User WHERE " + query;
            if (filter.Take.HasValue)
            {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}