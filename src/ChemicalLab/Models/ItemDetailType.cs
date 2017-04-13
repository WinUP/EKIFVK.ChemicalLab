using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class ItemDetailType {
        public ItemDetailType() {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool RequireCas { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}