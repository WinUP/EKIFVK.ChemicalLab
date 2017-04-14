using System.Collections.Generic;
using System.Linq;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.SearchFilters;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/track")]
    public class TrackController : VerifiableController {

        public TrackController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker)
            : base(database, verifier, tracker) {
        }

        [HttpGet(".count")]
        [Verify("TK:GET")]
        public JsonResult GetTrackCount(TrackSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.TrackHistories.FromSql(query, param.ToArray()).Count());
        }

        [HttpGet(".list")]
        [Verify("TK:GET")]
        public JsonResult GetTrackList(TrackSearchFilter filter) {
            var param = new List<object>();
            var query = QueryGenerator(filter, param);
            return Json(data: Database.TrackHistories.FromSql(query, param.ToArray())
                .Select(e => new {
                    e.Id,
                    Modifier = Database.Users.FirstOrDefault(u => u.Id == e.Modifier).Name,
                    e.HistoryType,
                    e.TargetRecord,
                    e.ModifyTime,
                    e.PreviousData,
                    e.NewData
                }).ToArray());
        }

        [HttpDelete]
        [Verify("TK:DELETE")]
        public JsonResult RemoveRange(string ids) {
            var idList = ids.Split(';').Select(int.Parse).ToArray();
            foreach (var e in idList) {
                var target = Database.TrackHistories.FirstOrDefault(t => t.Id == e);
                if (target != null) Database.TrackHistories.Remove(target);
            }
            Database.SaveChanges();
            return Json();
        }

        private static string QueryGenerator(TrackSearchFilter filter, ICollection<object> param) {
            var condition = new List<string>();
            var paramCount = -1;
            if (filter.HistoryType != null) {
                condition.Add("HistoryType IN (@p" + ++paramCount + ")");
                param.Add(filter.HistoryType.Select(e => e.ToString()).Join(","));
            }
            if (filter.Modifier.HasValue) {
                condition.Add("Modifier = @p" + ++paramCount);
                param.Add(filter.Modifier.Value);
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