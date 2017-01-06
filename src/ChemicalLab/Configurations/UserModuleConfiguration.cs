namespace EKIFVK.ChemicalLab.Configurations {
    public class UserModuleConfiguration {
        //Basic consts
        public string NoTargetUser { get; set; }
        public string NoTargetGroup { get; set; }
        public string InvalidUsernameFormat { get; set; }
        public string UserAlreadyExist { get; set; }
        public string WrongPassword { get; set; }
        public string DisabledUser { get; set; }
        public string CannotSingOutOthers { get; set; }
        public string CannotChangeSelfGroup { get; set; }
        public string CannotDisableSelf { get; set; }
        public string InvalidGroupNameFormat { get; set; }
        public string GroupAlreadyExist { get; set; }
        public string CannotChangeSelfGroupDisabled { get; set; }

        //Server configurations
        public string DefaulPasswordHash { get; set; }

        //Permission groups
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

        //Table name confidurations
        public string UserTable { get; set; }
        public string UserGroupTable { get; set; }

        //Table column name configurations
        public string UserTableAccessToken { get; set; }
        public string UserTableDisabled { get; set; }
        public string UserTablePassword { get; set; }
        public string UserTableGroup { get; set; }
        public string UserTableAllowMultipleLogin { get; set; }
        public string UserGroupTableDisabled { get; set; }
        public string UserGroupTableName { get; set; }
        public string UserGroupTableNote { get; set; }
        public string UserGroupTablePermission { get; set; }

        //Extended consts
        public string GetUserInfo { get; set; }
        public string AddUser { get; set; }
        public string SingIn { get; set; }
        public string SingOut { get; set; }
        public string DisableUser { get; set; }
        public string ResetPassword { get; set; }
        public string ChangePassword { get; set; }
        public string ChangeUserGroup { get; set; }
        public string ChangeUserAllowMultipleLogin { get; set; }
        public string ChangeUserDisabled { get; set; }
        public string AddGroup { get; set; }
        public string DisableGroup { get; set; }
        public string ChangeGroupName { get; set; }
        public string ChangeGroupNote { get; set; }
        public string ChangeGroupPermission { get; set; }
        public string ChangeGroupDisabled { get; set; }
    }
}