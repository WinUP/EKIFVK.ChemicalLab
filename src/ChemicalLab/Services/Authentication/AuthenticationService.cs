using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;

namespace EKIFVK.ChemicalLab.Services.Authentication {
    public class AuthenticationService : IAuthentication {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<AuthenticationConfiguration> _config;

        public AuthenticationService(ChemicalLabContext database, IOptions<AuthenticationConfiguration> config) {
            _database = database;
            _config = config;
        }

        private static bool IsTokenValid(string token) {
            return token.ToUpper() == token && token.Length == 36;
        }

        public string FindToken(IHeaderDictionary header) {
            return !header.TryGetValue(_config.Value.TokenHttpHeaderKey, out var token) ? "" : token.ToString();
        }

        public VerifyResult Verify(string token, string permissionGroup, IPAddress address = null) {
            return IsTokenValid(token)
                ? Verify(_database.Users.FirstOrDefault(e => e.AccessToken == token), permissionGroup, address)
                : VerifyResult.InvalidFormat;
        }

        public VerifyResult Verify(User user, string permissionGroup, IPAddress address = null) {
            if (user == null) return VerifyResult.NonexistentToken;
            if (user.Disabled) return VerifyResult.Denied;
            var group = user.UserGroupNavigation ?? _database.UserGroups.FirstOrDefault(e => e.Id == user.UserGroup);
            if (group.Disabled) return VerifyResult.Denied;
            if (!user.LastAccessTime.HasValue ||
                user.LastAccessTime.Value.AddMinutes(_config.Value.TokenAvaliableMinutes) < DateTime.Now)
                return VerifyResult.Expired;
            if (address != null && !string.IsNullOrEmpty(user.LastAccessAddress) &&
                user.LastAccessAddress != address.ToString() && !user.AllowMultiAddressLogin)
                return VerifyResult.Denied;
            UpdateAccessTime(user);
            UpdateAccessAddress(user, address);
            if (string.IsNullOrEmpty(permissionGroup)) return VerifyResult.Passed;
            var permissionGroupInstance = _database.PermissionGroups.FirstOrDefault(e => e.Name == permissionGroup);
            if (permissionGroupInstance == null) return VerifyResult.NonexistentGroup;
            var permissions = permissionGroupInstance.Permission.Split(' ');
            if (permissions.Length == 0 || permissions.Length == 1 && string.IsNullOrEmpty(permissions[0]))
                return VerifyResult.Passed;
            var userPermissions = group.Permission.Split(' ');
            return userPermissions.Any(e => permissions.All(p => p != e)) ? VerifyResult.Denied : VerifyResult.Passed;
        }

        public void UpdateAccessTime(string token) {
            if (!IsTokenValid(token)) return;
            var user = _database.Users.FirstOrDefault(e => e.AccessToken == token);
            UpdateAccessTime(user);
        }

        public void UpdateAccessTime(User user) {
            user.LastAccessTime = DateTime.Now;
            user.LastUpdate = DateTime.Now;
            _database.SaveChanges();
        }

        public void UpdateAccessAddress(string token, IPAddress address) {
            if (!IsTokenValid(token)) return;
            var user = _database.Users.FirstOrDefault(e => e.AccessToken == token);
            UpdateAccessAddress(user, address);
        }

        public void UpdateAccessAddress(User user, IPAddress address) {
            user.LastAccessAddress = address?.ToString();
            user.LastUpdate = DateTime.Now;
            _database.SaveChanges();
        }

        public string ToString(VerifyResult result) {
            switch (result) {
                case VerifyResult.Passed:
                    return _config.Value.VerifyPassed;
                case VerifyResult.Denied:
                    return _config.Value.VerifyDenied;
                case VerifyResult.NonexistentToken:
                    return _config.Value.VerifyNonexistentToken;
                case VerifyResult.InvalidFormat:
                    return _config.Value.VerifyInvalid;
                case VerifyResult.Expired:
                    return _config.Value.VerifyExpired;
                case VerifyResult.NonexistentGroup:
                    return _config.Value.VerifyNonexistentGroup;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "Cannot convert unknow result");
            }
        }
    }
}