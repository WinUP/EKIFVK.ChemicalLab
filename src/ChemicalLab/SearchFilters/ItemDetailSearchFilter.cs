namespace EKIFVK.ChemicalLab.SearchFilters {
    public class ItemDetailSearchFilter : BasicSearchFilter {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Cas { get; set; }
        public int? Unit { get; set; }
        public double? SizeMin { get; set; }
        public double? SizeMax { get; set; }
        public int? Container { get; set; }
        public int? State { get; set; }
        public int? Detail { get; set; }
        public string Msds { get; set; }
        public string MsdsDateMin { get; set; }
        public string MsdsDateMax { get; set; }
        public int? RequiredMin { get; set; }
        public int? RequiredMax { get; set; }
        public bool? Disabled { get; set; }
    }
}