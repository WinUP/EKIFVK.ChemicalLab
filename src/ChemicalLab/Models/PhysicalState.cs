using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class PhysicalState : BasicRecordableSimpleTable
    {
        public PhysicalState()
        {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}
