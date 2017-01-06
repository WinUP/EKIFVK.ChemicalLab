using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class ContainterType : BasicRecordableSimpleTable {
        public ContainterType() {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}