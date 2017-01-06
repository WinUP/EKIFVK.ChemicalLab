using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Room : BasicRecordableSimpleTable {
        public Room() {
            Locations = new HashSet<Location>();
        }

        public virtual ICollection<Location> Locations { get; set; }
    }
}