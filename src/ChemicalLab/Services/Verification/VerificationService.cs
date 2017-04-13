using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;

namespace EKIFVK.ChemicalLab.Services.Verification {
    public class VerificationService : IVerificationService {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<AuthenticationModule> _config;

        public VerificationService(ChemicalLabContext database, IOptions<AuthenticationModule> config) {
            _database = database;
            _config = config;
        }

        public string FindToken(IHeaderDictionary header) {
            return !header.TryGetValue(_config.Value.TokenHttpHeaderKey, out var token) ? null : token.ToString();
        }

        public User FindUser(string name) {
            return _database.Users.FirstOrDefault(e => e.Name == name);
        }

        public UserGroup FindGroup(string name) {
            return _database.UserGroups.FirstOrDefault(e => e.Name == name);
        }

        public UserGroup FindGroup(User user) {
            if (user == null) return null;
            return user.UserGroupNavigation ?? _database.UserGroups.FirstOrDefault(e => e.Id == user.UserGroup);
        }

        public bool Check(string token, string permission, IPAddress address) {
            return Check(_database.Users.FirstOrDefault(e => e.AccessToken == token), permission, address);
        }

        public bool Check(User user, string permission, IPAddress address) {
            Check(user, permission, out var result, address);
            return result == VerificationResult.Authorized;
        }

        public bool Check(string token, string permission, out VerificationResult result, IPAddress address = null) {
            return Check(_database.Users.FirstOrDefault(e => e.AccessToken == token), permission, out result, address);
        }

        public bool Check(User user, string permission, out VerificationResult result, IPAddress address = null) {
            if (user == null) {
                result = VerificationResult.Invalid;
                return false;
            }
            if (user.Disabled) {
                result = VerificationResult.Denied;
                return false;
            }
            var group = FindGroup(user);
            if (group.Disabled) {
                result = VerificationResult.Denied;
                return false;
            }
            if (!user.LastAccessTime.HasValue ||
                user.LastAccessTime.Value.AddMinutes(_config.Value.TokenAvaliableMinutes) < DateTime.Now) {
                result = VerificationResult.Overdue;
                return false;
            }
            if (address != null && !string.IsNullOrEmpty(user.LastAccessAddress) && user.LastAccessAddress != address.ToString()) {
                result = VerificationResult.Denied;
                return false;
            }
            UpdateAccessTime(user, false);
            UpdateAccessAddress(user, address);
            if (string.IsNullOrEmpty(permission) || group.Permission != null && group.Permission.Split(' ').Any(e => e == permission)) {
                result = VerificationResult.Authorized;
                return true;
            }
            result = VerificationResult.Denied;
            return false;
        }

        public void UpdateAccessTime(string token, bool submit = true) {
            UpdateAccessTime(_database.Users.FirstOrDefault(e => e.AccessToken == token), submit);
        }

        public void UpdateAccessTime(User user, bool submit = true) {
            if (user == null) return;
            user.LastAccessTime = DateTime.Now;
            user.LastUpdate = DateTime.Now;
            if (submit) _database.SaveChanges();
        }

        public void UpdateAccessAddress(string token, IPAddress address, bool submit = true) {
            UpdateAccessAddress(_database.Users.FirstOrDefault(e => e.AccessToken == token), address, submit);
        }

        public void UpdateAccessAddress(User user, IPAddress address, bool submit = true) {
            if (user == null) return;
            user.LastAccessAddress = address?.ToString();
            user.LastUpdate = DateTime.Now;
            if (submit) _database.SaveChanges();
        }

        public string ToString(VerificationResult result) {
            switch (result) {
                case VerificationResult.Authorized:
                    return _config.Value.AuthorizedString;
                case VerificationResult.Denied:
                    return _config.Value.DeniedString;
                case VerificationResult.Invalid:
                    return _config.Value.InvalidString;
                case VerificationResult.Overdue:
                    return _config.Value.OverdueString;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "Cannot convert unknow result");
            }
        }
    }
}