using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Experiment {
        public Experiment() {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}