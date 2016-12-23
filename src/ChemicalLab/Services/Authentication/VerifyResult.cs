namespace EKIFVK.ChemicalLab.Services.Authentication
{
    /// <summary>
    /// Authentication verify result
    /// </summary>
    public enum VerifyResult
    {
        /// <summary>
        /// Passed
        /// </summary>
        Passed,
        /// <summary>
        /// Token expired
        /// </summary>
        Expired,
        /// <summary>
        /// Token is invalid
        /// </summary>
        Invalid,
        /// <summary>
        /// Cannot find token
        /// </summary>
        Nonexistent,
        /// <summary>
        /// Denied
        /// </summary>
        Denied
    }
}
