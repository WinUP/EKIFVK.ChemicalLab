using System;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Services.Logging
{
    public class LoggingService : ILoggingService
    {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<ModifyLoggingConfiguration> _configuration;

        public LoggingService(ChemicalLabContext database, IOptions<ModifyLoggingConfiguration> configuration)
        {
            _database = database;
            _configuration = configuration;
        }

        public void Write(LoggingRecord record)
        {
            string loggingType;
            switch (record.Type)
            {
                case LoggingType.InfoLevel1:
                    loggingType = _configuration.Value.InfoLevel1;
                    break;
                case LoggingType.InfoLevel2:
                    loggingType = _configuration.Value.InfoLevel2;
                    break;
                case LoggingType.InfoLevel3:
                    loggingType = _configuration.Value.InfoLevel3;
                    break;
                case LoggingType.ErrorLevel1:
                    loggingType = _configuration.Value.ErrorLevel1;
                    break;
                case LoggingType.ErrorLevel2:
                    loggingType = _configuration.Value.ErrorLevel2;
                    break;
                case LoggingType.ErrorLevel3:
                    loggingType = _configuration.Value.ErrorLevel3;
                    break;
                default:
                    return;
            }
            _database.ModifyHistories.Add(new ModifyHistory
            {
                ModifierNavigation = record.Source,
                ModifyType= loggingType,
                ModifyTime = DateTime.Now,
                TableName = record.Table,
                RecordId = record.Record,
                Data = record.Data
            });
            _database.SaveChanges();
        }
    }
}
