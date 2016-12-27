using System;
using System.Linq;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EKIFVK.ChemicalLab.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<ModifyLoggingConfiguration> _configuration;
        private readonly ModifyType _normalType;
        private readonly ModifyType _rejectedType;
        private readonly ModifyType _warningType;

        public LoggingService(ChemicalLabContext database, IOptions<ModifyLoggingConfiguration> configuration)
        {
            _database = database;
            _configuration = configuration;
            _normalType = _database.ModifyTypes.FirstOrDefault(e => e.Name == _configuration.Value.NormalModifyType);
            _rejectedType = _database.ModifyTypes.FirstOrDefault(e => e.Name == _configuration.Value.RejectedModifyType);
            _warningType = _database.ModifyTypes.FirstOrDefault(e => e.Name == _configuration.Value.WarningModifyType);
            if (_normalType == null || _rejectedType == null || _warningType == null)
                throw new MissingMemberException(
                    "At lest one logging type cannot be found, please check your configuration of ModifyLoggingConfiguration section");
        }

        public void Write(string type, User modifier, string table, int record, JObject data = null)
        {
            ModifyType modifyType;
            if (type == _configuration.Value.NormalModifyType)
                modifyType = _normalType;
            else if(type == _configuration.Value.RejectedModifyType)
                modifyType = _rejectedType;
            else if (type == _configuration.Value.WarningModifyType)
                modifyType = _warningType;
            else
                modifyType = _database.ModifyTypes.FirstOrDefault(e => e.Name == type);
            if (modifyType == null) return;
            _database.ModifyHistorys.Add(new ModifyHistory
            {
                ModifierNavigation = modifier,
                ModifyTypeNavigation = modifyType,
                ModifyTime = DateTime.Now,
                TableName = table,
                RecordId = record,
                Data = JsonConvert.SerializeObject(data)
            });
            _database.SaveChanges();
        }

        public void WriteNormal(User modifier, string table, int record, JObject data = null)
        {
            Write(_configuration.Value.NormalModifyType, modifier, table, record, data);
        }

        public void WriteRejected(User modifier, JObject data = null)
        {
            Write(_configuration.Value.RejectedModifyType, modifier, "Permission", -1, data);
        }

        public void WriteWarning(User modifier, string table, int record, JObject data = null)
        {
            Write(_configuration.Value.WarningModifyType, modifier, table, record, data);
        }
    }
}
