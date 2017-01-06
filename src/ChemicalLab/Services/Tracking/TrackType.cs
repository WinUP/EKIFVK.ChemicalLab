namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Track history type
    /// </summary>
    public enum TrackType {
        /// <summary>
        /// Database changed
        /// </summary>
        InfoL1,

        /// <summary>
        /// Permission request passed
        /// </summary>
        InfoL2,

        /// <summary>
        /// Information returned
        /// </summary>
        InfoL3,

        /// <summary>
        /// Database changing failed
        /// </summary>
        ErrorL1,

        /// <summary>
        /// Permission request failed
        /// </summary>
        ErrorL2,

        /// <summary>
        /// System error
        /// </summary>
        ErrorL3
    }
}