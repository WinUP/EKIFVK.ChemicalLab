using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EKIFVK.ChemicalLab.Configurations
{
    public class UserModuleConfiguration
    {
        public string NotSignIn { get; set; }
        public string NoTargetUser { get; set; }
        public string InvalidUsernameFormat { get; set; }
        public string InvalidPasswordFormat { get; set; }
        public string UserAlreadyExist { get; set; }

        public int NormalUserGroup { get; set; }

        public string UserManagePermission { get; set; }
        public string UserAddingPermission { get; set; }
    }
}
