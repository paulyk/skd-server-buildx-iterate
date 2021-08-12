

namespace SKD.Seed {
   
    public class Component_MockData_DTO {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class CmponentStation_McckData_DTO {
        public string ComponentCode { get; set; }
        public string StationCode { get; set; }
    }

    public class ProductionStation_Mock_DTO {
        public string Code { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }
}