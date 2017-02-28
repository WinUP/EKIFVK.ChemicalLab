using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Filters;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    public class TrackController : VerifiableController {

        public TrackController(ChemicalLabContext database, IVerificationService verifier, ITrackService tracker)
            : base(database, verifier, tracker) {
        }

        [HttpGet(".count")]
        [PermissionCheck("TRACK:GET")]
        public JsonResult GetTrackCount(TrackSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return FormattedResponse(data: Database.TrackHistories.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [PermissionCheck("TRACK:GET")]
        public JsonResult GetTrackList(TrackSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return
                FormattedResponse(
                    data: Database.TrackHistories.FromSql(query, param.ToArray()).Select(e => new Hashtable {
                        {"id", e.Id},
                        {"modifier", Database.Users.FirstOrDefault(u => u.Id == e.Modifier).Name},
                        {"type", e.HistoryType},
                        {"target", e.TargetTable},
                        {"record", e.TargetRecord},
                        {"column", e.TargetColumn},
                        {"time", e.ModifyTime},
                        {"data", e.Data}
                    }).ToArray());
        }

        [HttpDelete]
        [PermissionCheck("TRACK:DELETE")]
        public JsonResult RemoveRange(int startId, int endId) {
            var list = Database.TrackHistories.Where(e => e.Id >= startId && e.Id <= endId);
            foreach (var e in list) {
                Database.TrackHistories.Remove(e);
            }
            Database.SaveChanges();
            return FormattedResponse();
        }

        private static string QueryGenerator(TrackSearchFilter filter, ICollection<object> param) {
            //? MySql connector for .net core still does not support Take() and Skip() in this version
            //? which means we can only form SQL query manually
            //? Also, LIMIT in mysql has significant performnce issue so we will not use LIMIT
            var condition = new List<string>();
            var paramCount = -1;
            if (!string.IsNullOrEmpty(filter.HistoryType)) {
                condition.Add("HistoryType = '@p" + ++paramCount + "'");
                param.Add(filter.HistoryType);
            }
            if (!string.IsNullOrEmpty(filter.TargetTable)) {
                condition.Add("TargetTable = '@p" + ++paramCount + "'");
                param.Add(filter.TargetTable);
            }
            if (!string.IsNullOrEmpty(filter.StartTime)) {
                condition.Add("ModifyTime >= '@p" + ++paramCount + "'");
                param.Add(filter.StartTime);
            }
            if (!string.IsNullOrEmpty(filter.EndTime)) {
                condition.Add("ModifyTime <= '@p" + ++paramCount + "'");
                param.Add(filter.EndTime);
            }
            var query = "";
            if (condition.Count > 0) query = string.Join(" AND ", condition);
            if (filter.Skip.HasValue && filter.Skip.Value > 0) {
                query = "SELECT * FROM TrackHistory WHERE ID >= (SELECT ID FROM TrackHistory WHERE " + query +
                        " ORDER BY ID LIMIT @p" + ++paramCount +
                        ",1)" + (query.Length > 0 ? " AND " : "") + query;
                param.Add(filter.Skip.Value);
            }
            else
                query = "SELECT * FROM TrackHistory WHERE " + query;
            if (filter.Take.HasValue) {
                query += " LIMIT @p" + ++paramCount;
                param.Add(filter.Take.Value);
            }
            return query;
        }
    }
}