using System;
using Microsoft.Extensions.Options;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;

namespace EKIFVK.ChemicalLab.Services.Tracking {
    public class TrackService : ITrackService {
        private readonly ChemicalLabContext _database;
        private readonly IOptions<TrackModuleConfiguration> _configuration;

        public TrackService(ChemicalLabContext database, IOptions<TrackModuleConfiguration> configuration) {
            _database = database;
            _configuration = configuration;
        }

        public void Write(TrackRecord record) {
            string loggingType;
            switch (record.Type) {
                case TrackType.I1D:
                    loggingType = _configuration.Value.InfoLevel1;
                    break;
                case TrackType.I2P:
                    loggingType = _configuration.Value.InfoLevel2;
                    break;
                case TrackType.I3I:
                    loggingType = _configuration.Value.InfoLevel3;
                    break;
                case TrackType.E1D:
                    loggingType = _configuration.Value.ErrorLevel1;
                    break;
                case TrackType.E2P:
                    loggingType = _configuration.Value.ErrorLevel2;
                    break;
                case TrackType.E3S:
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