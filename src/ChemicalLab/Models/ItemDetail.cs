using System;
using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models {
    public partial class ItemDetail {
        public ItemDetail() {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Cas { get; set; }
        public int Unit { get; set; }
        public double Size { get; set; }
        public int ContainerType { get; set; }
        public int PhysicalState { get; set; }
        public int DetailType { get; set; }
        public string Msds { get; set; }
        public DateTime MsdsDate { get; set; }
        public int Required { get; set; }
        public string Note { get; set; }
        public bool Disabled { get; set; }
        public DateTime LastUpdate { get; set; }

        public virtual Unit UnitNavigation { get; set; }
        public virtual ContainterType ContainterTypeNavigation { get; set; }
        public virtual PhysicalState PhysicalStateNavigation { get; set; }
        public virtual ItemDetailType DetailTypeNavigation { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}