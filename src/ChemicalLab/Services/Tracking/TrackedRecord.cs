using System;
using EKIFVK.ChemicalLab.Models;

namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Tracking record
    /// </summary>
    public class TrackedRecord {
        private readonly ITrackerService _tracker;

        public User Modifier { get; private set; }
        public Operation Operation { get; }
        public DateTime Time { get; }
        public string Previous { get; private set; }
        public string After { get; private set; }
        public int Target { get; private set; }

        public TrackedRecord(Operation operation, ITrackerService tracker) {
            Operation = operation;
            Time = DateTime.Now;
            _tracker = tracker;
        }

        public TrackedRecord By(User user) {
            Modifier = user;
            return this;
        }

        public TrackedRecord From(string data) {
            Previous = data;
            return this;
        }

        public TrackedRecord Do(Action action) {
            action.Invoke();
            return this;
        }

        public TrackedRecord To(string data) {
            After = data;
            return this;
        }

        public TrackedRecord At(int id) {
            Target = id;
            return this;
        }

        public void Save(bool submit = true) {
            _tracker.Save(this, submit);
        }
    }
}