namespace EKIFVK.ChemicalLab.SearchFilters {
    public class ItemSearchFilter : BasicSearchFilter {
        public string ReceivedTimeMin { get; set; }
        public string ReceivedTimeMax { get; set; }
        public string OpenedTimeMin { get; set; }
        public string OpenedTimeMax { get; set; }
        public int[] Location { get; set; }
        public string Owner { get; set; }
        public int[] Experiment { get; set; }
        public int[] Vendor { get; set; }
        public double? UsedMin { get; set; }
        public double? UsedMax { get; set; }
        public bool? Disabled { get; set; }
    }
}