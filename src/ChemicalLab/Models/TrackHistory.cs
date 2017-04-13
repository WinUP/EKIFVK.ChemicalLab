using System;

namespace EKIFVK.ChemicalLab.Models {
    public partial class TrackHistory {
        public int Id { get; set; }
        public int Modifier { get; set; }
        public int HistoryType { get; set; }
        public int? TargetRecord { get; set; }
        public DateTime ModifyTime { get; set; }
        public string PreviousData { get; set; }
        public string NewData { get; set; }

        public virtual User ModifierNavigation { get; set; }
    }
}