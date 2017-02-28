namespace EKIFVK.ChemicalLab.Configurations {
    public class UserModuleConfiguration {
        //Basic consts
        public string EmptyUser { get; set; }
        public string EmptyGroup { get; set; }
        public string InvalidFormat { get; set; }
        public string UserAlreadyExist { get; set; }
        public string WrongPassword { get; set; }
        public string DisabledUser { get; set; }
        public string SignOutOthers { get; set; }
        public string ChangeSelfGroup { get; set; }
        public string DisableSelf { get; set; }
        public string GroupAlreadyExist { get; set; }

        //Server configurations
        public string DefaulPasswordHash { get; set; }

        //Table name confidurations
        public string UserTable { get; set; }
        public string UserGroupTable { get; set; }

        //Table column name configurations
        public string UserTableDisplayName { get; set; }
        public string UserTablePassword { get; set; }
        public string UserTableGroup { get; set; }
        public string UserTableAccessToken { get; set; }
        public string UserTableDisabled { get; set; }
        public string UserGroupTableName { get; set; }
        public string UserGroupTableNote { get; set; }
        public string UserGroupTablePermission { set; get; }
        public string UserGroupTableDisabled { get; set; }

        //Extended consts
        public string GetUserInfo { get; set; }
        public string AddUser { get; set; }
        public string SingIn { get; set; }
        public string SingOut { get; set; }
        public string DisableUser { get; set; }
        public string ResetPassword { get; set; }
        public string ChangePassword { get; set; }
        public string ChangeUserGroup { get; set; }
        public string ChangeUserDisplayName { get; set; }
        public string AddGroup { get; set; }
        public string ChangeGroupName { get; set; }
        public string ChangeGroupNote { get; set; }
        public string ChangeGroupPermission { get; set; }
        public string ChangeGroupDisabled { get; set; }
    }
}