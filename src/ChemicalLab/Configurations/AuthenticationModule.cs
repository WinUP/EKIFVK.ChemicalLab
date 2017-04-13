namespace EKIFVK.ChemicalLab.Configurations {
    public class AuthenticationModule {
        public double TokenAvaliableMinutes { get; set; }
        public string TokenHttpHeaderKey { get; set; }
        public string AuthorizedString { get; set; }
        public string DeniedString { get; set; }
        public string OverdueString { get; set; }
        public string InvalidString { get; set; }
    }
}