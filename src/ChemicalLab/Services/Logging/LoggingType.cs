namespace EKIFVK.ChemicalLab.Services.Logging
{
    public enum LoggingType
    {
        /// <summary>
        /// Database change
        /// </summary>
        InfoLevel1,
        /// <summary>
        /// Permission request passed
        /// </summary>
        InfoLevel2,
        /// <summary>
        /// Information returned
        /// </summary>
        InfoLevel3,
        /// <summary>
        /// Any other error
        /// </summary>
        ErrorLevel1,
        /// <summary>
        /// Permission request failed
        /// </summary>
        ErrorLevel2,
        /// <summary>
        /// System error
        /// </summary>
        ErrorLevel3
    }
}