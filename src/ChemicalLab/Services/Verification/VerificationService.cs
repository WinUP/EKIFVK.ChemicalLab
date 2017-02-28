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
        private readonly IOptions<AuthenticationConfiguration> _conf;

        public VerificationService(ChemicalLabContext database, IOptions<AuthenticationConfiguration> config) {
            _database = database;
            _conf = config;
        }

        public string ExtractToken(IHeaderDictionary header) {
            return !header.TryGetValue(_conf.Value.TokenHttpHeaderKey, out var token) ? null : token.ToString();
        }

        public User FindUser(string name) {
            return _database.Users.FirstOrDefault(e => e.Name == name);
        }

        public UserGroup FindGroup(User user) {
            if (user == null) return null;
            return user.UserGroupNavigation ?? _database.UserGroups.FirstOrDefault(e => e.Id == user.UserGroup);
        }

        public VerificationResult Check(string token, string permission, IPAddress address = null) {
            return Check(_database.Users.FirstOrDefault(e => e.AccessToken == token), permission, address);
        }

        public VerificationResult Check(User user, string permission, IPAddress address = null) {
            if (user == null)
                return VerificationResult.Invalid;
            if (user.Disabled)
                return VerificationResult.Reject;
            var group = FindGroup(user);
            if (group.Disabled)
                return VerificationResult.Reject;
            if (!user.LastAccessTime.HasValue ||
                user.LastAccessTime.Value.AddMinutes(_conf.Value.TokenAvaliableMinutes) < DateTime.Now)
                return VerificationResult.Expired;
            if (address != null && !string.IsNullOrEmpty(user.LastAccessAddress) && user.LastAccessAddress != address.ToString())
                return VerificationResult.Reject;
            UpdateAccessTime(user);
            UpdateAccessAddress(user, address);
            if (string.IsNullOrEmpty(permission))
                return VerificationResult.Pass;
            var userPermissions = group.Permission.Split(' ');
            return userPermissions.Any(e => e == permission)
                ? VerificationResult.Pass
                : VerificationResult.Reject;
        }

        public bool Check(string token, string permission, out VerificationResult verificationResult,
            IPAddress address = null) {
            verificationResult = Check(token, permission, address);
            return verificationResult == VerificationResult.Pass;
        }

        public bool Check(User user, string permission, out VerificationResult verificationResult,
            IPAddress address = null) {
            verificationResult = Check(user, permission, address);
            return verificationResult == VerificationResult.Pass;
        }

        public void UpdateAccessTime(string token) {
            UpdateAccessTime(_database.Users.FirstOrDefault(e => e.AccessToken == token));
        }

        public void UpdateAccessTime(User user) {
            if (user == null) return;
            user.LastAccessTime = DateTime.Now;
            user.LastUpdate = DateTime.Now;
            _database.SaveChanges();
        }

        public void UpdateAccessAddress(string token, IPAddress address) {
            UpdateAccessAddress(_database.Users.FirstOrDefault(e => e.AccessToken == token), address);
        }

        public void UpdateAccessAddress(User user, IPAddress address) {
            if (user == null) return;
            user.LastAccessAddress = address?.ToString();
            user.LastUpdate = DateTime.Now;
            _database.SaveChanges();
        }

        public string ToString(VerificationResult result) {
            switch (result) {
                case VerificationResult.Pass:
                    return _conf.Value.VerifyPassString;
                case VerificationResult.Reject:
                    return _conf.Value.VerifyRejectString;
                case VerificationResult.Invalid:
                    return _conf.Value.VerifyInvalidString;
                case VerificationResult.Expired:
                    return _conf.Value.VerifyExpiredString;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "Cannot convert unknow result");
            }
        }
    }
}