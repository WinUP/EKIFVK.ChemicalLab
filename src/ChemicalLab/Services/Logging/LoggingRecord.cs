using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EKIFVK.ChemicalLab.Models;

namespace EKIFVK.ChemicalLab.Services.Logging
{
    public class LoggingRecord
    {
        private readonly JObject _record;
        private readonly JObject _data;


        public LoggingRecord(LoggingType type, User source)
        {
            Type = type;
            Source = source;
            _data = new JObject();
            _record = new JObject { { "d", _data } };
        }

        public LoggingRecord(LoggingType type, User source, string table, int record)
        {
            Type = type;
            Source = source;
            Table = table;
            Record = record;
            _data = new JObject();
            _record = new JObject {{"d", _data } };
        }

        public LoggingType Type { get; }
        public User Source { get; }
        public string Table { get; set; }
        public int Record { get; set; }
        public string Data => JsonConvert.SerializeObject(_record);

        public LoggingRecord Add(string key, JToken value)
        {
            _record.Add(key, value);
            return this;
        }

        public LoggingRecord AddContent(string value)
        {
            _record.Add("c", value);
            return this;
        }

        public LoggingRecord AddData(string key, JToken value)
        {
            _data.Add(key, value);
            return this;
        }
    }
}
