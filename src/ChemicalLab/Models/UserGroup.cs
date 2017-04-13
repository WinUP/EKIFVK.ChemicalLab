using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class UserGroup {
        public UserGroup() {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Permission { get; set; }
        public bool Disabled { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}