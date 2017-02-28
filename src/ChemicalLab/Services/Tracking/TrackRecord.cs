using EKIFVK.ChemicalLab.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Tracking record
    /// </summary>
    public class TrackRecord {
        private readonly JObject _record;

        public TrackRecord(TrackType type, User source) {
            Type = type;
            Source = source;
            Record = 0;
            _record = new JObject();
        }

        public TrackRecord(TrackType type, User source, string table, int record, string column) {
            Type = type;
            Source = source;
            Table = table;
            Record = record;
            Column = column;
            _record = new JObject();
        }

        /// <summary>
        /// Type of this record
        /// </summary>
        public TrackType Type { get; }

        /// <summary>
        /// Modifier of this record
        /// </summary>
        public User Source { get; }

        /// <summary>
        /// Table which this record referenced
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Column which this record referenced
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Database record ID which this record rederenced
        /// </summary>
        public int Record { get; set; }

        /// <summary>
        /// String type data of this record
        /// </summary>
        public string Data => JsonConvert.SerializeObject(_record);

        /// <summary>
        /// Add key value pair to this record
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public TrackRecord Add(string key, JToken value) {
            _record.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add note to this record
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TrackRecord Note(JToken value) {
            _record.Add("d", value);
            return this;
        }

        /// <summary>
        /// Add previous value of database record which this record referenced
        /// </summary>
        /// <param name="value">Previous value of database record</param>
        /// <returns></returns>
        public TrackRecord PreviousData(JToken value) {
            _record.Add("p", value);
            return this;
        }

        /// <summary>
        /// Add new value of database record which this record referenced
        /// </summary>
        /// <param name="value">New value of database record</param>
        /// <returns></returns>
        public TrackRecord NewData(JToken value) {
            _record.Add("n", value);
            return this;
        }
    }
}