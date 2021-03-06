﻿using System.Net;
using Microsoft.AspNetCore.Http; 
using EKIFVK.ChemicalLab.Models;

namespace EKIFVK.ChemicalLab.Services.Verification {
    /// <summary>
    /// User authentication service
    /// </summary>
    public interface IVerificationService {

        /// <summary>
        /// Find token in http header (might be null)
        /// </summary>
        /// <param name="header">Http header</param>
        /// <returns>Token in this http header</returns>
        string FindToken(IHeaderDictionary header);

        /// <summary>
        /// Find User by User's name (might be null)
        /// </summary>
        /// <param name="name">User's name</param>
        /// <returns></returns>
        User FindUser(string name);

        /// <summary>
        /// Find UserGroup by name
        /// </summary>
        /// <param name="name">UserGroup's name</param>
        /// <returns></returns>
        UserGroup FindGroup(string name);

        /// <summary>
        /// Get User's UserGroup (might be null is user is null)
        /// </summary>
        /// <param name="user">Target User</param>
        /// <returns></returns>
        UserGroup FindGroup(User user);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="token">User's access token</param>
        /// <param name="permission">Permission group which should be checked</param>
        /// <param name="address">User's address</param>
        /// <returns>Indicate if result is Pass</returns>
        bool Check(string token, string permission, IPAddress address = null);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permission">Permission's name which should be checked</param>
        /// <param name="address">User's address</param>
        /// <returns>Indicate if result is Pass</returns>
        bool Check(User user, string permission, IPAddress address = null);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="token">User's access token</param>
        /// <param name="permission">Permission's name which should be checked</param>
        /// <param name="result">Verification result</param>
        /// <param name="address">User's address</param>
        /// <returns>Indicate if result is Pass</returns>
        bool Check(string token, string permission, out VerificationResult result, IPAddress address = null);

        /// <summary>
        /// Verify user's permission
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="permission">Permission's name which should be checked</param>
        /// <param name="result">Verification result</param>
        /// <param name="address">User's address</param>
        /// <returns>Indicate if result is Pass</returns>
        bool Check(User user, string permission, out VerificationResult result, IPAddress address = null);

        /// <summary>
        /// Update user's last acsess time to now
        /// </summary>
        /// <param name="token">User's token</param>
        /// <param name="submit">Shoudl submit this update to database automatically</param>
        void UpdateAccessTime(string token, bool submit = true);

        /// <summary>
        /// Update user's last acsess time to now
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="submit">Shoudl submit this update to database automatically</param>
        void UpdateAccessTime(User user, bool submit = true);

        /// <summary>
        /// Update user's last acsess addrress
        /// </summary>
        /// <param name="token">User's token</param>
        /// <param name="address">User's address</param>
        /// <param name="submit">Shoudl submit this update to database automatically</param>
        void UpdateAccessAddress(string token, IPAddress address, bool submit = true);

        /// <summary>
        /// Update user's last acsess addrress
        /// </summary>
        /// <param name="user">User's instance</param>
        /// <param name="address">User's address</param>
        /// <param name="submit">Shoudl submit this update to database automatically</param>
        void UpdateAccessAddress(User user, IPAddress address, bool submit = true);

        /// <summary>
        /// Convert verify result to string
        /// </summary>
        /// <param name="result"></param>
        /// <returns>String value of verify result</returns>
        string ToString(VerificationResult result);
    }
}