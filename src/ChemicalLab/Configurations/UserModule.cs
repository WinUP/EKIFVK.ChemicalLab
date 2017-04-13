namespace EKIFVK.ChemicalLab.Configurations {
    public class UserModule {
        public string InvalidUserName { get; set; }
        public string InvalidGroupName { get; set; }
        public string IncorrectPassword { get; set; }
        public string AlreadyExisted { get; set; }
        public string CannotRemoveSelf { get; set; }
        public string CannotChangeSelf { get; set; }
        public string OperationDenied { get; set; }

        //Server configurations
        public string DefaulPasswordHash { get; set; }
    }
}