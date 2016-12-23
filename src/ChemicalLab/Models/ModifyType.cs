using System.Collections.Generic;

namespace EKIFVK.ChemicalLab.Models
{
    public partial class ModifyType
    {
        public ModifyType()
        {
            ModifyHistories = new HashSet<ModifyHistory>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ModifyHistory> ModifyHistories { get; set; }
    }
}
