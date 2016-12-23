using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class Unit : BasicRecordableSimpleTable
    {
        public Unit()
        {
            ItemDetails = new HashSet<ItemDetail>();
        }

        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
    }
}
