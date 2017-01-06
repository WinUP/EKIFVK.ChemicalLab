namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Tracking service
    /// </summary>
    public interface ITrackService {
        /// <summary>
        /// Add a new tracking history to database <br />
        /// All changes for database will be saved
        /// </summary>
        /// <param name="record">Tracking record</param>
        void Write(TrackRecord record);
    }
}