using System;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class ModifyHistory
    {
        public int Id { get; set; }
        public int Modifier { get; set; }
        public string ModifyType { get; set; }
        public DateTime ModifyTime { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Data { get; set; }

        public virtual User ModifierNavigation { get; set; }
    }
}
