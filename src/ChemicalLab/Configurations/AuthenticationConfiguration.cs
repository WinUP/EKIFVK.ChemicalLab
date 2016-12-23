namespace EKIFVK.ChemicalLab.Configurations
{
    public class AuthenticationConfiguration
    {
        public double TokenAvaliableMinutes { get; set; }

        public string VerifyPassed { get; set; }
        public string VerifyDenied { get; set; }
        public string VerifyNonexistent { get; set; }
        public string VerifyInvalid { get; set; }
        public string VerifyExpired { get; set; }
    }
}
