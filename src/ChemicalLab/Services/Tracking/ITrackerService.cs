namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Tracking service
    /// </summary>
    public interface ITrackerService {

        /// <summary>
        /// Get a new tracking history from one history type
        /// </summary>
        /// <param name="type">History type</param>
        /// <returns></returns>
        TrackedRecord Get(Operation type);
        /// <summary>
        /// Add a new tracking history to database <br />
        /// All changes for database will be saved
        /// </summary>
        /// <param name="record">Tracking record</param>
        /// <param name="submit"></param>
        void Save(TrackedRecord record, bool submit = true);
    }
}