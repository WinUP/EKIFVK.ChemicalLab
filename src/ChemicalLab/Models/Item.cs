using System;

namespace EKIFVK.ChemicalLab.Models {
    public partial class Item {
        public int Id { get; set; }
        public int Detail { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime ReceivedTime { get; set; }
        public DateTime? OpenedTime { get; set; }
        public int Location { get; set; }
        public int Owner { get; set; }
        public int Experiment { get; set; }
        public int Vendor { get; set; }
        public double Used { get; set; }
        public bool Disabled { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual ItemDetail DetailNavigation { get; set; }
        public virtual Location LocationNavigation { get; set; }
        public virtual User OwnerNavigation { get; set; }
        public virtual Experiment ExperimentNavigation { get; set; }
        public virtual Vendor VendorNavigation { get; set; }
    }
}