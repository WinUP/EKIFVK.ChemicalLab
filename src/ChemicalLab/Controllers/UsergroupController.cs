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

namespace EKIFVK.ChemicalLab.Controllers
{
    /// <summary>
    /// 用户组相关API
    /// <list type="bullet">
    /// <item><description>GET /{name} => GetInfo</description></item>
    /// <item><description>POST /{name} => Add</description></item>
    /// <item><description>DELETE /{name} => Delete</description></item>
    /// <item><description>PATCH /{name} => RouteHttpPatch</description></item>
    /// <item><description>GET /.count => GetGroupCount</description></item>
    /// <item><description>GET /.list => GetGroupList</description></item>
    /// </list>
    /// </summary>
    [Route("usergroup")]
    public class UsergroupController : BasicVerifiableController
    {
        private readonly IOptions<UserModuleConfiguration> _configuration;

        public UsergroupController(ChemicalLabContext database, IAuthentication verifier, IOptions<UserModuleConfiguration> configuration)
            : base(database, verifier)
        {
            _configuration = configuration;
        }

        

        /// <summary>
        /// 获取用户组信息<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> name, description, enabled[bool], users[int]<br />
        /// <list type="bullet">
        /// <item><description>用户组名不存在：401 INVALID_NAME -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">用户组名</param>
        /// <returns></returns>
        [HttpGet("{name}")]
        public JsonResult GetInfo(string name)
        {
            var group = Database.SystemUsergroup.FirstOrDefault(e => e.Name == name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            return JsonResponse(data: new Hashtable
            {
                {"name", group.Name},
                {"description", group.Description},
                {"enabled", group.Enabled},
                {"users", group.SystemUser.Count}
            });
        }

        /// <summary>
        /// 添加新的用户组<br />
        /// <br />
        /// 权限：GROUP:MANAGE<br />
        /// 返回：200 SUCCESS -> id[int]<br />
        /// <list type="bullet">
        /// <item><description>用户名不合法：400 INVALID_NAME -> null</description></item>
        /// <item><description>Token不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>同名用户组已存在：409 ALREADY_EXIST -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">用户组名（不能包含/\.）</param>
        /// <returns></returns>
        [HttpPost("{name}")]
        public JsonResult Add(string name)
        {
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return JsonResponse(StatusCodes.Status400BadRequest, Consts.Value.INVALID_NAME);
            var user = FindUser();
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_MANAGE);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group != null)
                return JsonResponse(StatusCodes.Status409Conflict, Consts.Value.ALREADY_EXIST);
            group = new SystemUsergroup
            {
                Name = name
            };
            Database.SystemUsergroup.Add(group);
            Database.SaveChanges();
            return JsonResponse(data: group.Id);
        }

        /// <summary>
        /// 删除用户<br />
        /// <br />
        /// 权限：GROUP:MANAGE &amp; 要删除的用户组没有任何从属用户<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户组不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>用户组存在从属用户：403 PERMISSION_DENIED -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
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
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_MANAGE);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (Database.SystemUser.Count(e => e.Usergroup == group.Id) > 0)
                return JsonResponse(StatusCodes.Status403Forbidden, Consts.Value.PERMISSION_DENIED);
            Database.SystemUsergroup.Remove(group);
            Database.SaveChanges();
            return JsonResponse();
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
                case "permission":
                    answer = ChangePermission(user, name, (string[]) data);
                    break;
                case "tag":
                    answer = ChangeTagOrDescription(user, name, (string) data, true);
                    break;
                case "description":
                    answer = ChangeTagOrDescription(user, name, (string) data, false);
                    break;
                case "name":
                    answer = ChangeName(user, name, (string)data);
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
        /// 修改用户组权限<br />
        /// <br />
        /// 权限：GROUP:PERMPATCH<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户组或某一权限不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户组名</param>
        /// <param name="permission">新的权限列表</param>
        /// <returns></returns>
        public JsonResult ChangePermission(SystemUser user, string name, string[] permission)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_PERMPATCH);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (!permission.All(e => Database.SystemPermission.Count(i => i.Name == e) > 0))
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            group.Permission = string.Join(" ", permission);
            Database.SaveChanges();
            return JsonResponse();
        }

        /// <summary>
        /// 修改用户组的附加数据或描述<br />
        /// <br />
        /// 权限：GROUP:PATCH<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户组不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户组名</param>
        /// <param name="data">新的数据</param>
        /// <param name="isChangeTag">修改的是否是附加数据（为False时修改描述）</param>
        /// <returns></returns>
        public JsonResult ChangeTagOrDescription(SystemUser user, string name, string data, bool isChangeTag)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_PATCH);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            if (isChangeTag)
                group.Tag = data;
            else
                group.Description = data;
            Database.SaveChanges();
            return JsonResponse();
        }

        /// <summary>
        /// 修改用户组的名称<br />
        /// <br />
        /// 权限：GROUP:PATCH<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户组不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户组名</param>
        /// <param name="data">新的数据</param>
        /// <returns></returns>
        public JsonResult ChangeName(SystemUser user, string name, string data)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_PATCH);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            group.Name = data;
            Database.SaveChanges();
            return JsonResponse();
        }

        /// <summary>
        /// 启用或禁用用户组<br />
        /// <br />
        /// 权限：GROUP:ENABLE<br />
        /// 返回：200 SUCCESS -> null<br />
        /// <list type="bullet">
        /// <item><description>Token或用户组不存在：401 INVALID_NAME -> null</description></item>
        /// <item><description>权限不足：403 权限验证失败组 -> null</description></item>
        /// </list>
        /// </summary>
        /// <param name="user">当前会话的用户</param>
        /// <param name="name">用户组名</param>
        /// <param name="enabled">是否启用用户组</param>
        /// <returns></returns>
        public JsonResult ChangeEnabled(SystemUser user, string name, bool enabled)
        {
            if (user == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            var verifyResult = Checker.Verify(user, HttpContext.Connection.RemoteIpAddress.ToString(), Consts.Value.PERM_GROUP_ENABLE);
            if (verifyResult != PermissionService.VerifyResult.Authorized)
                return JsonResponse(StatusCodes.Status403Forbidden, Checker.ToString(verifyResult));
            var group = FindGroup(name);
            if (group == null)
                return JsonResponse(StatusCodes.Status401Unauthorized, Consts.Value.INVALID_NAME);
            group.Enabled = enabled;
            Database.SaveChanges();
            return JsonResponse();
        }

        /// <summary>
        /// 获取用户组数量<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> count[int]<br />
        /// </summary>
        /// <returns></returns>
        [HttpGet(".count")]
        public JsonResult GetGroupCount()
        {
            return JsonResponse(data: Database.SystemUsergroup.Count().ToString());
        }

        /// <summary>
        /// 获取用户组名列表<br />
        /// <br />
        /// 权限：无<br />
        /// 返回：200 SUCCESS -> name[]<br />
        /// </summary>
        /// <param name="skip">跳过的数据条数</param>
        /// <param name="count">获取的数据条数</param>
        /// <returns></returns>
        [HttpGet(".list")]
        public JsonResult GetGroupList(int skip, int count)
        {

            return JsonResponse(data: Database.SystemUsergroup.Skip(skip).Take(count).Select(e => e.Name));
        }
    }
}
