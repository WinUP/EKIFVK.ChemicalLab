using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Vendor {
        public Vendor() {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public bool Disabled { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}