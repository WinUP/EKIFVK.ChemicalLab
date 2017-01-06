using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Vendor : BasicDisableSimpleTable {
        public Vendor() {
            Items = new HashSet<Item>();
        }

        public string Number { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}