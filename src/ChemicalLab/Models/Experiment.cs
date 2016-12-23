using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class Experiment : BasicRecordableSimpleTable
    {
        public Experiment()
        {
            Items = new HashSet<Item>();
        }

        public virtual ICollection<Item> Items { get; set; }
    }
}
