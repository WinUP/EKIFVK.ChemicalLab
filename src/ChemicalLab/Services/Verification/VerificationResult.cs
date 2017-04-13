namespace EKIFVK.ChemicalLab.Services.Verification {
    /// <summary>
    /// User authentication verify result
    /// </summary>
    public enum VerificationResult {
        /// <summary>
        /// Authorized
        /// </summary>
        Authorized,
        /// <summary>
        /// Token is overdue
        /// </summary>
        Overdue,
        /// <summary>
        /// Cannot find token
        /// </summary>
        Invalid,
        /// <summary>
        /// Denied
        /// </summary>
        Denied
    }
}