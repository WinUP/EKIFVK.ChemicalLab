using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Location : BasicRecordableTable {
        public Location() {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public int Place { get; set; }
        public int Room { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual Place PlaceNavigation { get; set; }
        public virtual Room RoomNavigation { get; set; }
    }
}