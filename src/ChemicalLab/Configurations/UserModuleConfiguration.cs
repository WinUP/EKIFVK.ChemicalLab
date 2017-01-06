namespace EKIFVK.ChemicalLab.Configurations
{
    public class UserModuleConfiguration
    {
        public string NoTargetUser { get; set; }
        public string NoTargetGroup { get; set; }
        public string InvalidUsernameFormat { get; set; }
        public string UserAlreadyExist { get; set; }
        public string WrongPassword { get; set; }
        public string DisabledUser { get; set; }
        public string CannotRemoveSelf { get; set; }
        public string CannotSingOutOthers { get; set; }
        public string CannotChangeSelfGroup { get; set; }
        public string CannotDisableSelf { get; set; }
        public string InvalidGroupNameFormat { get; set; }
        public string GroupAlreadyExist { get; set; }
        public string CannotChangeSelfGroupDisabled { get; set; }

        public string DefaulPasswordHash { get; set; }

        public string UserManagePermission { get; set; }
        public string UserAddingPermission { get; set; }
        public string UserModifyDisabledPermission { get; set; }
        public string UserResetPasswordPermission { get; set; }
        public string UserChangeGroupPermission { get; set; }
        public string UserModifyPermission { get; set; }
        public string UserDisablePermission { get; set; }
        public string GroupManagePermission { get; set; }
        public string GroupAddingPermission { get; set; }
        public string GroupModifyPermissionPermission { get; set; }
        public string GroupModifyDisabledPermission { get; set; }

        public string GetUserInfoLog { get; set; }
        public string RegisterExistentUserLog { get; set; }
        public string RegisterUserLog { get; set; }
        public string SingInLog { get; set; }
        public string TrySignOutOtherUserLog { get; set; }
        public string SingOutLog { get; set; }
        public string TryDisableSelfLog { get; set; }
        public string DisableUserLog { get; set; }
        public string ResetPasswordLog { get; set; }
        public string ChangePasswordLog { get; set; }
        public string TryChangeSelfGroupLog { get; set; }
        public string ChangeUserGroupLog { get; set; }
        public string ChangeUserAllowMultipleLoginLog { get; set; }
        public string ChangeUserDisabledLog { get; set; }
        public string AddGroupLog { get; set; }
        public string DisableGroupLog { get; set; }
        public string ChangeGroupNameLog { get; set; }
        public string ChangeGroupNoteLog { get; set; }
        public string ChangeGroupPermissionLog { get; set; }
        public string ChangeGroupDisabledLog { get; set; }
    }
}
