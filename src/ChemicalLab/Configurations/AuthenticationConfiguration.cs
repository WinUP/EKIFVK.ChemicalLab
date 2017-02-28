namespace EKIFVK.ChemicalLab.Configurations {
    public class AuthenticationConfiguration {
        public double TokenAvaliableMinutes { get; set; }
        public string TokenHttpHeaderKey { get; set; }
        public string VerifyPassString { get; set; }
        public string VerifyRejectString { get; set; }
        public string VerifyExpiredString { get; set; }
        public string VerifyInvalidString { get; set; }
    }
}