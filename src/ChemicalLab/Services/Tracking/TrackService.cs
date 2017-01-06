using System;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;

namespace EKIFVK.ChemicalLab.Services.Tracking {
    public class TrackService : ITrackService {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<ModifyLoggingConfiguration> _configuration;

        public TrackService(ChemicalLabContext database, IOptions<ModifyLoggingConfiguration> configuration) {
            _database = database;
            _configuration = configuration;
        }

        public void Write(TrackRecord record) {
            string loggingType;
            switch (record.Type) {
                case TrackType.InfoL1:
                    loggingType = _configuration.Value.InfoLevel1;
                    break;
                case TrackType.InfoL2:
                    loggingType = _configuration.Value.InfoLevel2;
                    break;
                case TrackType.InfoL3:
                    loggingType = _configuration.Value.InfoLevel3;
                    break;
                case TrackType.ErrorL1:
                    loggingType = _configuration.Value.ErrorLevel1;
                    break;
                case TrackType.ErrorL2:
                    loggingType = _configuration.Value.ErrorLevel2;
                    break;
                case TrackType.ErrorL3:
                    loggingType = _configuration.Value.ErrorLevel3;
                    break;
                default:
                    return;
            }
            var history = new TrackHistory {
                ModifierNavigation = record.Source,
                HistoryType = loggingType,
                ModifyTime = DateTime.Now,
                Data = record.Data
            };
            if (!string.IsNullOrEmpty(record.Column) && !string.IsNullOrEmpty(record.Table) && record.Record != 0) {
                history.TargetTable = record.Table;
                history.TargetColumn = record.Column;
                history.TargetRecord = record.Record;
            }
            _database.TrackHistories.Add(history);
            _database.SaveChanges();
        }
    }
}