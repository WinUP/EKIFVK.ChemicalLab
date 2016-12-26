namespace EKIFVK.ChemicalLab.SearchFilter
{
    public class UserSearchFilter
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public bool? Disabled { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
