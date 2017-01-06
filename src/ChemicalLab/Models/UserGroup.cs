using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class UserGroup : BasicDisableSimpleTable {
        public UserGroup() {
            Users = new HashSet<User>();
        }

        public string Note { get; set; }
        public string Permission { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}