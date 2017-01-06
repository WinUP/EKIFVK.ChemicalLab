using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class ItemDetailType : BasicRecordableSimpleTable {
        public ItemDetailType() {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public bool RequireCas { get; set; }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}