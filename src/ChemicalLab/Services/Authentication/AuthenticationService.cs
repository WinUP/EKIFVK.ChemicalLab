using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;

namespace EKIFVK.ChemicalLab.Services.Authentication
{
    public class AuthenticationService : IAuthentication
    {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<AuthenticationConfiguration> _config;

        public AuthenticationService(ChemicalLabContext database, IOptions<AuthenticationConfiguration> config)
        {
            _database = database;
            _config = config;
        }

        private static bool IsTokenValid(string token)
        {
            return token.ToUpper() == token && token.Length == 36;
        }

        public string FindToken(IHeaderDictionary header)
        {
            return !header.TryGetValue("X-Access-Token", out var token) ? "" : token.ToString();
        }

        public VerifyResult Verify(string token, string[] permissions, string ip = "")
        {
            if (!IsTokenValid(token)) return VerifyResult.Invalid;
            var user = _database.Users.FirstOrDefault(e => e.AccessToken == token);
            return Verify(user, permissions, ip);
        }

        public VerifyResult Verify(string token, string permission, string ip = "")
        {
            return Verify(token, new[] {permission}, ip);
        }

        public VerifyResult Verify(User user, string[] permissions, string ip = "")
        {
            if (user == null) return VerifyResult.Nonexistent;
            if (user.Disabled) return VerifyResult.Denied;
            var group = user.UserGroupNavigation ?? _database.UserGroups.FirstOrDefault(e => e.Id == user.UserGroup);
            if (group.Disabled) return VerifyResult.Denied;
            if (!user.LastAccessTime.HasValue ||
                user.LastAccessTime.Value.AddMinutes(_config.Value.TokenAvaliableMinutes) < DateTime.Now)
                return VerifyResult.Expired;
            if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(user.LastAccessAddress) &&
                user.LastAccessAddress != ip && !user.AllowMultiAddressLogin) return VerifyResult.Denied;
            UpdateAccessTime(user);
            if (permissions.Length == 0 || permissions.Length == 1 && string.IsNullOrEmpty(permissions[0]))
                return VerifyResult.Passed;
            var userPermissions = group.Permission.Split(' ');
            return userPermissions.Any(e => permissions.All(p => p != e)) ? VerifyResult.Denied : VerifyResult.Passed;
        }

        public VerifyResult Verify(User user, string permission, string ip = "")
        {
            return Verify(user, new[] { permission }, ip);
        }

        public void UpdateAccessTime(string token)
        {
            if (!IsTokenValid(token)) return;
            var user = _database.Users.FirstOrDefault(e => e.AccessToken == token);
            UpdateAccessTime(user);
        }

        public void UpdateAccessTime(User user)
        {
            user.LastAccessTime = DateTime.Now;
            _database.SaveChanges();
        }

        public string ToString(VerifyResult result)
        {
            switch (result)
            {
                case VerifyResult.Passed:
                    return _config.Value.VerifyPassed;
                case VerifyResult.Denied:
                    return _config.Value.VerifyDenied;
                case VerifyResult.Nonexistent:
                    return _config.Value.VerifyNonexistent;
                case VerifyResult.Invalid:
                    return _config.Value.VerifyInvalid;
                case VerifyResult.Expired:
                    return _config.Value.VerifyExpired;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "Cannot convert unknow result");
            }
        }
    }
}