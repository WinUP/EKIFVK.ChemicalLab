using System;
using System.Collections;
using System.Linq;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

//! Username is not case sensitive

namespace EKIFVK.ChemicalLab.Controllers
{
    /// <summary>
    /// API for User Management
    /// <list type="bullet">
    /// <item><description>GET /{name} => GetInfo</description></item>
    /// <item><description>POST /{name} => Register</description></item>
    /// <item><description>PUT /{name} => Login</description></item>
    /// <item><description>DELETE /{name} => Delete</description></item>
    /// <item><description>PATCH /{name} => RouteHttpPatch</description></item>
    /// <item><description>GET /.count => GetUserCount</description></item>
    /// <item><description>GET /.list => GetUserList</description></item>
    /// </list>
    /// </summary>
    [Route("user")]
    public class UserController : BasicVerifiableController
    {
        public UserController(ChemicalLabContext database, IAuthentication verifier, IOptions<UserModuleConfiguration> configuration)
            : base(database, verifier, configuration) { }

        /// <summary>
        /// Get user information<br />
        /// <br />
        /// Return：
        /// <list type="bullet">
        /// <item><description>{name, group, accessTime, accessAddress, allowMulti, disabled:bool, update}</description></item>
        /// </list>
        /// Error:
        /// <list type="bullet">
        /// <item><description>Not sign in：401 NotSignIn</description></item>
        /// <item><description>No target user：401 NoTargetUser</description></item>
        /// <item><description>Permission denied：403 [VerifyResult]</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">Target user's name</param>
        /// <returns></returns>
        [HttpGet("{name}")]
        public JsonResult GetInfo(string name)
        {
            var user = FindUser();
            if (user == null) return BasicResponse(StatusCodes.Status401Unauthorized, Configuration.Value.NotSignIn);
            if (!Verify(user, Configuration.Value.UserManagePermission, out var verifyResult)) return Basic403(verifyResult);
            user = FindUser(name);
            if (user == null) return BasicResponse(StatusCodes.Status401Unauthorized, Configuration.Value.NoTargetUser);
            return BasicResponse(data: new Hashtable
            {
                {"name", user.Name},
                {"group", user.UserGroupNavigation.Name},
                {"accessTime", user.LastAccessTime},
                {"accessAddress", user.LastAccessAddress},
                {"allowMulti", user.AllowMultiAddressLogin},
                {"disabled", user.Disabled},
                {"update", user.LastUpdate}
            });
        }

        /// <summary>
        /// Register<br />
        /// <br />
        /// Return：
        /// <list type="bullet">
        /// <item><description>{id}</description></item>
        /// </list>
        /// Error:
        /// <list type="bullet">
        /// <item><description>Not sign in：401 NOT_SIGN_IN</description></item>
        /// <item><description>Permission denied：403 [VerifyResult]</description></item>
        /// <item><description>Invalid username format：400 INVALID_USERNAME_FORMAT</description></item>
        /// <item><description>Invalid password format：400 INVALID_PASSWORD_FORMAT</description></item>
        /// <item><description>User already exist：409 USER_ALREADY_EXIST</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">Username (cannot have /\.)</param>
        /// <param name="parameter">
        /// Paramaters<br />
        /// <list type="bullet">
        /// <item><description>password: Uppercase SHA256 of password</description></item>
        /// </list>
        /// </param>
        /// <returns></returns>
        [HttpPost("{name}")]
        public JsonResult Register(string name, [FromBody] Hashtable parameter)
        {
            var user = FindUser();
            if (user == null) return BasicResponse(StatusCodes.Status401Unauthorized, Configuration.Value.NotSignIn);
            var verifyResult = Verify(user, Configuration.Value.UserAddingPermission);
            if (verifyResult != VerifyResult.Passed) return Basic403(verifyResult);
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return BasicResponse(StatusCodes.Status400BadRequest, Configuration.Value.InvalidUsernameFormat);
            var password = parameter["password"].ToString();
            if (password.ToUpper() != password || password.Length != 64)
                return BasicResponse(StatusCodes.Status400BadRequest, Configuration.Value.InvalidPasswordFormat);
            user = FindUser(name);
            if (user != null) return BasicResponse(StatusCodes.Status409Conflict, Configuration.Value.UserAlreadyExist);
            var normalUsergroupId = Configuration.Value.NormalUserGroup;
            user = new User
            {
                Name = name,
                Password = password,
                UserGroupNavigation = Database.UserGroups.FirstOrDefault(e => e.Id == normalUsergroupId),
                LastUpdate = DateTime.Now
            };
            Database.Users.Add(user);
            Database.SaveChanges();
            return BasicResponse(data: user.Id);
        }

        /// <summary>
        /// 用户登陆<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> token<br />
        /// <list type="bullet">
        /// <item><description>用户不存在：401 INVALID_USERNAME -> null</description></item>
        /// <item><description>密码不正确：403 INVALID_PASSWORD -> null</description></item>
        /// <item><description>登陆太频繁：403 LOGIN_TOOCLOSE -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="config">系统设置服务（由依赖注入提供）</param>
        /// <param name="name">用户名</param>
        /// <param name="password">密码的SHA256结果（大写表示）</param>
        [HttpPut("{name}")]
        public JsonResult Login([FromServices] IOptions<SystemConfig> config, string name, string password)
        {
            var user = FindUser(name);
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (user.Password != password)
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.INVALID_PASSWORD);
            if (user.LastActiveTime?.AddSeconds(config.Value.ClosestLoginTimespan) > DateTime.Now)
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.LOGIN_TOOCLOSE);
            var group = Database.SystemUsergroup.FirstOrDefault(e => e.Id == user.Usergroup);
            if(!user.Enabled || !group.Enabled)
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.LOGIN_DENIED);
            var token = Guid.NewGuid().ToString().ToUpper();
            user.AccessToken = token;
            Checker.UpdateAccessTime(user);
            user.LastActiveTime = DateTime.Now;
            user.LastAccessIp = HttpContext.Connection.RemoteIpAddress.ToString();
            Database.SaveChanges();
            return BasicResponse(data: token);
        }

        /// <summary>
        /// 删除用户<br />
        /// <br />
        /// 权限：USER:MANAGE &amp; 删除的不是自己<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// <item><description>数据库操作失败：500 SERVER_ERROR -> exception</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">用户名</param>
        /// <returns></returns>
        [HttpDelete("{name}")]
        public JsonResult Delete(string name)
        {
            var user = FindUser();
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (Equals(user.Name, name))
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.PERMISSION_DENIED);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_USER_MANAGE);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            user = FindUser(name);
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            try
            {
                Database.SystemUser.Remove(user);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                return JsonResponse(StatusCodes.Status500InternalServerError, ex.Message, Consts.Value.SERVER_ERROR);
            }
            return BasicResponse();
        }

        [HttpPatch("{name}")]
        public JsonResult RouteHttpPatch(string name, [FromBody] Hashtable parameter)
        {
            var user = FindUser();
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            JsonResult answer;
            var operation = parameter["operation"].ToString();
            var data = parameter["data"];
            switch (operation)
            {
                case "token":
                    answer = Logout(user, name);
                    break;
                case "password":
                    answer = ChangePassword(user, name, (string) data);
                    break;
                case "group":
                    answer = ChangeUsergroup(user, name, (string) data);
                    break;
                case "tag":
                    answer = ChangeTagOrDescription(user, name, (string) data, true);
                    break;
                case "description":
                    answer = ChangeTagOrDescription(user, name, (string) data, false);
                    break;
                case "enabled":
                    answer = ChangeEnabled(user, name, (bool) data);
                    break;
                default:
                    answer = JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
                    break;
            }
            return answer;
        }

        /// <summary>
        /// 用户注销<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>尝试注销其他用户：403 PERMISSION_DENIED -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户名</param>
        /// <returns></returns>
        public JsonResult Logout(SystemUser user, string name)
        {
            if (!Equals(user.Name, name))
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.PERMISSION_DENIED);
            user.AccessToken = null;
            Database.SaveChanges();
            return BasicResponse();
        }

        /// <summary>
        /// 修改或重置用户密码<br />
        /// <br />
        /// 权限：USER:PATCH（重置密码时）<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户名</param>
        /// <param name="newPassword">新的密码（重置密码时不需要）</param>
        /// <returns></returns>
        public JsonResult ChangePassword(SystemUser user, string name, string newPassword)
        {
            if (!Equals(user.Name, name))
            {
                var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_USER_PATCH);
                if (verifyResult != PermissionService.VerifyResult.Authorized)
                    return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
                user = FindUser(name);
                if (user == null)
                    return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
                user.Password = Consts.Value.NORMAL_PASSWORD;
            } else
                user.Password = newPassword;
            Database.SaveChanges();
            return BasicResponse();
        }

        /// <summary>
        /// 修改用户的用户组<br />
        /// <br />
        /// 权限：USER:GROUPPATCH &amp; 修改的不是自己的用户组<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token、用户或用户组不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户名</param>
        /// <param name="newGroup">新的用户组</param>
        /// <returns></returns>
        public JsonResult ChangeUsergroup(SystemUser user, string name, string newGroup)
        {
            if (Equals(user.Name, name))
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.PERMISSION_DENIED);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_USER_GROUPPATCH);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            user = FindUser(name);
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var group = Database.SystemUsergroup.FirstOrDefault(e => e.Name == newGroup);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            user.UsergroupNavigation = group;
            Database.SaveChanges();
            return BasicResponse();
        }

        /// <summary>
        /// 修改用户的附加数据或描述<br />
        /// <br />
        /// 权限：USER:PATCH（修改其他用户的附加数据或描述时）<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户名</param>
        /// <param name="data">新的数据</param>
        /// <param name="isChangeTag">修改的是否是附加数据（为False时修改描述）</param>
        /// <returns></returns>
        public JsonResult ChangeTagOrDescription(SystemUser user, string name, string data, bool isChangeTag)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (!Equals(user.Name, name))
            {
                var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_USER_PATCH);
                if (verifyResult != PermissionService.VerifyResult.Authorized)
                    return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
                user = FindUser(name);
                if (user == null)
                    return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            }
            if (isChangeTag)
                user.Tag = data;
            else
                user.Description = data;
            Database.SaveChanges();
            return BasicResponse();
        }

        /// <summary>
        /// 启用或禁用用户<br />
        /// <br />
        /// 权限：USER:ENABLE &amp; 不是修改自己<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户名</param>
        /// <param name="enabled">是否启用用户</param>
        /// <returns></returns>
        public JsonResult ChangeEnabled(SystemUser user, string name, bool enabled)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (Equals(user.Name, name))
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.PERMISSION_DENIED);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_USER_ENABLE);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            user = FindUser(name);
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            user.Enabled = enabled;
            Database.SaveChanges();
            return BasicResponse();
        }

        /// <summary>
        /// 获取用户数量<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> count[int]<br />
        /// </summary>
        /// <returns></returns>
        [HttpGet(".count")]
        public JsonResult GetUserCount()
        { 
            return JsonResponse(data: Database.SystemUser.Count().ToString());
        }

        /// <summary>
        /// 获取用户名列表<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> name[]<br />
        /// </summary>
        /// <param name="skip">跳过的数据条数</param>
        /// <param name="count">获取的数据条数</param>
        /// <returns></returns>
        [HttpGet(".list")]
        public JsonResult GetUserList(int skip, int count)
        {
            return JsonResponse(data: Database.SystemUser.Skip(skip).Take(count).Select(e=> e.Name));
        }
    }
}