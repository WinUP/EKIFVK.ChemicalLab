using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class User {
        public User() {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public int UserGroup { get; set; }
        public string AccessToken { get; set; }
        public DateTime? LastAccessTime { get; set; }
        public string LastAccessAddress { get; set; }
        public bool Disabled { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual UserGroup UserGroupNavigation { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<TrackHistory> ModifyHistories { get; set; }
    }
}