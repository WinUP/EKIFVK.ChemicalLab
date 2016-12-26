using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class User : BasicDisableSimpleTable
    {
        public User()
        {
            Items = new HashSet<Item>();
        }

        public string DisplayName { get; set; }
        public string Password { get; set; }
        public int UserGroup { get; set; }
        public string AccessToken { get; set; }
        public DateTime? LastAccessTime { get; set; }
        public bool AllowMultiAddressLogin { get; set; }
        public string LastAccessAddress { get; set; }

        public virtual UserGroup UserGroupNavigation { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<ModifyHistory> ModifyHistories { get; set; }
    }
}
