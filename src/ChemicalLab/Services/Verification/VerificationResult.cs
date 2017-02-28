namespace EKIFVK.ChemicalLab.Services.Verification {
    /// <summary>
    /// User authentication verify result
    /// </summary>
    public enum VerificationResult {
        /// <summary>
        /// Passed
        /// </summary>
        Pass,

        /// <summary>
        /// Token is expired
        /// </summary>
        Expired,

        /// <summary>
        /// Cannot find token
        /// </summary>
        Invalid,

        /// <summary>
        /// Denied
        /// </summary>
        Reject
    }
}