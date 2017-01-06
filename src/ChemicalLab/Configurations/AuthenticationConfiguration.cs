namespace EKIFVK.ChemicalLab.Configurations {
    public class AuthenticationConfiguration {
        public double TokenAvaliableMinutes { get; set; }
        public string TokenHttpHeaderKey { get; set; }

        public string VerifyPassed { get; set; }
        public string VerifyDenied { get; set; }
        public string VerifyNonexistentGroup { get; set; }
        public string VerifyNonexistentToken { get; set; }
        public string VerifyInvalid { get; set; }
        public string VerifyExpired { get; set; }
    }
}