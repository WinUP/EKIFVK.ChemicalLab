namespace EKIFVK.ChemicalLab.SearchFilters {
    public class TrackSearchFilter : BasicSearchFilter {
        public int[] HistoryType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int? Modifier { get; set; }
    }
}
