using System;

namespace EKIFVK.ChemicalLab.Models {
    public partial class TrackHistory {
        public int Id { get; set; }
        public int Modifier { get; set; }
        public string HistoryType { get; set; }
        public string TargetTable { get; set; }
        public int? TargetRecord { get; set; }
        public string TargetColumn { get; set; }
        public DateTime ModifyTime { get; set; }
        public string Data { get; set; }

        public virtual User ModifierNavigation { get; set; }
    }
}