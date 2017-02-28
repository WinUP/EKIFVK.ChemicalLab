using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EKIFVK.ChemicalLab.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Filters;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Verification;
using EKIFVK.ChemicalLab.Services.Tracking;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// API for UserGroup Management
    /// </summary>
    [Route("api/v1/usergroup")]
    public class UserGroupController : VerifiableController
    {
        private readonly IOptions<UserModuleConfiguration> _conf;

        public UserGroupController(ChemicalLabContext database, IVerificationService verifier, ITrackService tracker,
            IOptions<UserModuleConfiguration> configuration)
            : base(database, verifier, tracker) {
            _conf = configuration;
        }

        private UserGroup FindGroup(string name) {
            return Database.UserGroups.FirstOrDefault(e => e.Name == name);
        }

        private TrackRecord T(TrackType trackType, int record, string column)
        {
            return new TrackRecord(trackType, CurrentUser, _conf.Value.UserGroupTable, record, column);
        }

        [HttpGet("{name}")]
        public JsonResult GetInfo(string name) {
            var group = FindGroup(name);
            if (group == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyGroup);
            return FormattedResponse(data: new Hashtable {
                {"name", group.Name},
                {"note", group.Note},
                {"permission", group.Permission},
                {"user", Database.Users.Count(e => e.UserGroup == group.Id)}
            });
        }

        [HttpPost("{name}")]
        [PermissionCheck("GROUP:ADD")]
        public JsonResult Add(string name, [FromBody] Hashtable parameter) {
            if (string.IsNullOrEmpty(name) ||
                name.IndexOf("/", StringComparison.Ordinal) > -1 ||
                name.IndexOf("\\", StringComparison.Ordinal) > -1 ||
                name.IndexOf("?", StringComparison.Ordinal) > -1 ||
                name.IndexOf(".", StringComparison.Ordinal) == 0)
                return FormattedResponse(StatusCodes.Status400BadRequest, _conf.Value.InvalidFormat);
            var group = FindGroup(name);
            if (group != null)
                return FormattedResponse(StatusCodes.Status409Conflict, _conf.Value.GroupAlreadyExist);
            group = new UserGroup {
                Name = name,
                Note = parameter["note"].ToString(),
                Permission = parameter["permission"].ToString(),
                LastUpdate = DateTime.Now
            };
            Database.UserGroups.Add(group);
            Tracker.Write(T(TrackType.I3I, group.Id, "").Note(_conf.Value.AddGroup));
            return FormattedResponse(data: group.Id);
        }

        [HttpPatch("{name}")]
        [PermissionCheck("")]
        public JsonResult ChangeGroupInformation(string name, [FromBody] Hashtable parameter) {
            var target = FindGroup(name);
            if (target == null)
                return FormattedResponse(StatusCodes.Status404NotFound, _conf.Value.EmptyGroup);
            var finalData = new JObject();
            if (parameter.ContainsKey("name")) {
                if (!Verifier.Check(CurrentUser, "GROUP:MANAGE", out var verifyResult)) {
                    finalData.Add("name", false);
                    WritePermissionRejected(verifyResult, "GROUP:MANAGE");
                }
                else {
                    var newName = parameter["name"].ToString();
                    if (FindGroup(newName) != null)
                        return FormattedResponse(StatusCodes.Status409Conflict, _conf.Value.GroupAlreadyExist, finalData);
                    var previous = target.Name;
                    target.Name = newName;
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserGroupTableName)
                        .Note(_conf.Value.ChangeGroupName)
                        .PreviousData(previous)
                        .NewData(target.Name));
                    finalData.Add("name", true);
                }
            }
            if (parameter.ContainsKey("note")) {
                if (!Verifier.Check(CurrentUser, "GROUP:MANAGE", out var verifyResult)) {
                    finalData.Add("note", false);
                    WritePermissionRejected(verifyResult, "GROUP:MANAGE");
                }
                else {
                    var previous = target.Note;
                    target.Note = parameter["note"].ToString();
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserGroupTableNote)
                        .Note(_conf.Value.ChangeGroupNote)
                        .PreviousData(previous)
                        .NewData(target.Note));
                    finalData.Add("note", true);
                }
            }
            if (parameter.ContainsKey("permission")) {
                if (!Verifier.Check(CurrentUser, "GROUP:PERM", out var verifyResult)) {
                    finalData.Add("note", false);
                    WritePermissionRejected(verifyResult, "GROUP:PERM");
                }
                else {
                    var previous = target.Permission;
                    target.Permission = parameter["permission"].ToString();
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserGroupTablePermission)
                        .Note(_conf.Value.ChangeGroupPermission)
                        .PreviousData(previous)
                        .NewData(target.Permission));
                    finalData.Add("permission", true);
                }
            }
            if (parameter.ContainsKey("disabled")) {
                if (CurrentUser.UserGroup == target.Id) {
                    Tracker.Write(T(TrackType.E1D, CurrentUser.Id, "").Note(_conf.Value.DisableSelf));
                    finalData.Add("disable", false);
                }
                else if (!Verifier.Check(CurrentUser, "GROUP:STATUS", out var verifyResult)) {
                    finalData.Add("disable", false);
                    WritePermissionRejected(verifyResult, "GROUP:STATUS");
                }
                else {
                    var previous = target.Disabled;
                    target.Disabled = (bool)parameter["disabled"];
                    target.LastUpdate = DateTime.Now;
                    Tracker.Write(T(TrackType.I1D, target.Id, _conf.Value.UserGroupTableDisabled)
                        .Note(_conf.Value.ChangeGroupDisabled)
                        .PreviousData(previous)
                        .NewData(target.Permission));
                    finalData.Add("disabled", true);
                }
            }
            return FormattedResponse(data: finalData);
        }
        
        [HttpGet(".count")]
        public JsonResult GetGroupCount(GroupSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return FormattedResponse(data: Database.UserGroups.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [PermissionCheck("GROUP:MANAGE")]
        public JsonResult GetGroupList(GroupSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return FormattedResponse(data: Database.UserGroups.FromSql(query, param.ToArray()).Select(e => new Hashtable {
                {"name", e.Name},
                {"note", e.Note},
                {"permission", e.Permission},
                {"user", Database.Users.Count(u => u.UserGroup == e.Id)}
            }).ToArray());
        }

        private static string QueryGenerator(GroupSearchFilter filter, ICollection<object> param) {
            //? MySql connector for .net core still does not support Take() and Skip() in this version
            //? which means we can only form SQL query manually
            //? Also, LIMIT in mysql has significant performnce issue so we will not use LIMIT
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.Name)) {
                condition.Add("Name LIKE CONCAT('%',@p" + ++paramCount + ",'%')");
                param.Add(filter.Name);
            }
            if (filter.Disabled.HasValue) {
                condition.Add("Disabled = @p" + ++paramCount);
                param.Add(filter.Disabled.Value ? 1 : 0);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0) {
                query = "SELECT * FROM UserGroup WHERE ID >= (SELECT ID FROM UserGroup WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM UserGroup WHERE " + query;
            if (filter.Take.HasValue) {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}