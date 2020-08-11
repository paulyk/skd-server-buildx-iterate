namespace SKD.VCS.Model {

    public class ProductionStation : EntityBase {
        public string Code { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }
}