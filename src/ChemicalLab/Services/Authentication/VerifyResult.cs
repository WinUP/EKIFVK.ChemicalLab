namespace EKIFVK.ChemicalLab.Services.Authentication
{
    /// <summary>
    /// User authentication verify result
    /// </summary>
    public enum VerifyResult
    {
        /// <summary>
        /// Passed
        /// </summary>
        Passed,
        /// <summary>
        /// Token is expired
        /// </summary>
        Expired,
        /// <summary>
        /// Token's format is invalid
        /// </summary>
        InvalidFormat,
        /// <summary>
        /// Cannot find permission group
        /// </summary>
        NonexistentGroup,
        /// <summary>
        /// Cannot find token
        /// </summary>
        NonexistentToken,
        /// <summary>
        /// Denied
        /// </summary>
        Denied
    }
}
