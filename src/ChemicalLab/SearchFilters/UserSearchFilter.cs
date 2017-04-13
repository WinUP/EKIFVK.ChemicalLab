namespace EKIFVK.ChemicalLab.SearchFilters {
    public class UserSearchFilter : BasicSearchFilter {
        public string Name { get; set; }
        public string Group { get; set; }
        public bool? Disabled { get; set; }
    }
}