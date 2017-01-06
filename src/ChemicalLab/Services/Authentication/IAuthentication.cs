using System.Net;
using Microsoft.AspNetCore.Http; 
using EKIFVK.ChemicalLab.Models;

namespace EKIFVK.ChemicalLab.Services.Authentication {
    /// <summary>
    /// User authentication service
    /// </summary>
    public interface IAuthentication {
        /// <summary>
        /// Find token in http header
        /// </summary>
        /// <param name="header">Http header</param>
        /// <returns>Token in this http header</returns>
        string FindToken(IHeaderDictionary header);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="token">User's access token</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <param name="address">User's address</param>
        /// <returns>Verification result</returns>
        VerifyResult Verify(string token, string permissionGroup, IPAddress address = null);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permissionGroup">Permission group which should be checked</param>
        /// <param name="address">User's address</param>
        /// <returns>Verification result</returns>
        VerifyResult Verify(User user, string permissionGroup, IPAddress address = null);

        /// <summary>
        /// Update user's last acsess time to now
        /// </summary>
        /// <param name="token">User's token</param>
        void UpdateAccessTime(string token);

        /// <summary>
        /// Update user's last acsess time to now
        /// </summary>
        /// <param name="user">User's instance</param>
        void UpdateAccessTime(User user);

        /// <summary>
        /// Update user's last acsess addrress
        /// </summary>
        /// <param name="token">User's token</param>
        /// <param name="address">User's address</param>
        void UpdateAccessAddress(string token, IPAddress address);

        /// <summary>
        /// Update user's last acsess addrress
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="address">User's address</param>
        void UpdateAccessAddress(User user, IPAddress address);

        /// <summary>
        /// Convert verify result to string
        /// </summary>
        /// <param name="result"></param>
        /// <returns>String value of verify result</returns>
        string ToString(VerifyResult result);
    }
}