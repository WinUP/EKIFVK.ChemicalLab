namespace EKIFVK.ChemicalLab.Filters {
    public class UserSearchFilter : BasicSearchFilter {
        public string Name { get; set; }
        public string Group { get; set; }
        public bool? Disabled { get; set; }
    }
}