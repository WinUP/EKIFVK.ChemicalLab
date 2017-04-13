using EKIFVK.ChemicalLab.Models;

namespace EKIFVK.ChemicalLab.Services.Tracking {
    public class TrackerService : ITrackerService {
        private readonly ChemicalLabContext _database;

        public TrackerService(ChemicalLabContext database) {
            _database = database;
        }

        public TrackedRecord Get(Operation operation) {
            return new TrackedRecord(operation, this);
        }

        public void Save(TrackedRecord record)
        {
            var history = new TrackHistory
            {
                ModifierNavigation = record.Modifier,
                HistoryType = (int) record.Operation,
                ModifyTime = record.Time,
                PreviousData = record.Previous,
                NewData = record.After,
                TargetRecord = record.Target
            };
            _database.TrackHistories.Add(history);
            _database.SaveChanges();
        }
    }
}