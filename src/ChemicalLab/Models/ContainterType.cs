using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class ContainterType {
        public ContainterType() {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}