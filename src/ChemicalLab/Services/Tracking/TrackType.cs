namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// Track history type
    /// </summary>
    public enum TrackType {
        /// <summary>
        /// Database changed
        /// </summary>
        I1D,

        /// <summary>
        /// Permission request passed
        /// </summary>
        I2P,

        /// <summary>
        /// Information returned
        /// </summary>
        I3I,

        /// <summary>
        /// Database changing failed
        /// </summary>
        E1D,

        /// <summary>
        /// Permission request failed
        /// </summary>
        E2P,

        /// <summary>
        /// System error
        /// </summary>
        E3S
    }
}