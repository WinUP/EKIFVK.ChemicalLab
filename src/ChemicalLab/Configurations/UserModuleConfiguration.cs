namespace EKIFVK.ChemicalLab.Configurations
{
    public class UserModuleConfiguration
    {
        public string NoTargetUser { get; set; }
        public string NoTargetGroup { get; set; }
        public string InvalidUsernameFormat { get; set; }
        public string InvalidPasswordFormat { get; set; }
        public string UserAlreadyExist { get; set; }
        public string WrongPassword { get; set; }
        public string DisabledUser { get; set; }
        public string CannotRemoveSelf { get; set; }
        public string CannotSingOutOthers { get; set; }
        public string CannotChangeSelfGroup { get; set; }
        public string CannotDisableSelf { get; set; }

        public int DefaultUserGroup { get; set; }
        public string DefaulPasswordHash { get; set; }

        public string UserManagePermission { get; set; }
        public string UserAddingPermission { get; set; }
        public string UserDeletePermission { get; set; }
        public string UserResetPasswordPermission { get; set; }
        public string UserChangeGroupPermission { get; set; }
        public string UserModifyPermission { get; set; }
        public string UserDisablePermission { get; set; }
    }
}
