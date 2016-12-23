using System;
using System.Linq;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Authentication;

namespace EKIFVK.ChemicalLab.Controllers
{
    public class UserBasedController : BaseController
    {
        protected readonly ChemicalLabContext Database;
        protected readonly IAuthentication Verifier;
        protected readonly IOptions<UserModuleConfiguration> Configuration;

        public UserBasedController(ChemicalLabContext database, IAuthentication checker, IOptions<UserModuleConfiguration> configuration)
        {
            Verifier = checker;
            Database = database;
            Configuration = configuration;
        }

        /// <summary>
        /// 根据用户名查找用户实体（用户名无视大小写）
        /// </summary>
        /// <param name="name">用户名</param>
        /// <returns></returns>
        protected SystemUser FindUser(string name)
        {
            return Database.SystemUser.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// 获取当前会话的用户的实体
        /// </summary>
        /// <returns></returns>
        protected SystemUser FindUser()
        {
            var token = Verifier.FindToken(Request.Headers);
            return Database.SystemUser.FirstOrDefault(e => e.AccessToken == token);
        }

        /// <summary>
        /// 无视大小写比较字符串
        /// </summary>
        /// <param name="name1">字符串1</param>
        /// <param name="name2">字符串2</param>
        /// <returns></returns>
        protected static bool Equals(string name1, string name2)
        {
            return string.Equals(name1, name2, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
