using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Place {
        public Place() {
            Locations = new HashSet<Location>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
    }
}