namespace EKIFVK.ChemicalLab.Services.Tracking {
    /// <summary>
    /// History type
    /// </summary>
    public enum Operation {
        AddNewUser = 0,
        DeleteUser = 1,
        ChangeUserDisplayName = 2,
        ResetUserPassword = 3,
        ChangeUserPassowrd = 4,
        ChangeUserGroup = 5,
        ChangeUserDisabled = 6,
        AddNewUserGroup = 100,
        DeleteUserGroup = 101,
        ChangeUserGroupName = 102,
        ChangeUserGroupNote = 103,
        ChangeUserGroupPermission = 104,
        ChangeUserGroupDisabled = 105,
        AddNewRoom = 200,
        AddNewPlace = 201,
        AddNewLocation = 202
    }
}