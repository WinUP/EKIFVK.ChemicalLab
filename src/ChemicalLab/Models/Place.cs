using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Place : BasicRecordableSimpleTable {
        public Place() {
            Locations = new HashSet<Location>();
        }

        public virtual ICollection<Location> Locations { get; set; }
    }
}